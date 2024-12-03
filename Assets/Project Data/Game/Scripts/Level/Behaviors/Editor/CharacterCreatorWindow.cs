using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Watermelon.BusStop;

namespace Watermelon
{
    public class CharacterCreatorWindow : EditorWindow
    {
        private const string LOADING_TITLE = "Character Creation";

        private GameObject characterObject;

        [MenuItem("Tools/Editor/Character Creator")]
        public static CharacterCreatorWindow ShowWindow()
        {
            CharacterCreatorWindow window = GetWindow<CharacterCreatorWindow>(true);
            window.titleContent = new GUIContent("Character Creator");

            return window;
        }    
        
        [MenuItem("GameObject/Add to Character Creator", false, -200)]
        private static void ContextMenu(MenuCommand command)
        {
            GameObject body = (GameObject)command.context;
            if(body != null)
            {
                CharacterCreatorWindow window = ShowWindow();
                window.characterObject = body;
            }
        }

        [MenuItem("GameObject/Add to Character Creator", true)]
        private static bool ContextMenu()
        {
            return Selection.activeGameObject != null;
        }

        private IEnumerator CharacterCoroutine(string folderPath, GameObject characterObject)
        {
            EditorUtility.DisplayProgressBar(LOADING_TITLE, "Preparing folders...", 0);

            if (Directory.Exists(folderPath))
            {
                string materialFolderPath = Path.Combine(folderPath, "Materials");
                if (!Directory.Exists(materialFolderPath))
                {
                    Directory.CreateDirectory(materialFolderPath);

                    yield return null;

                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

                    yield return null;
                }
            }

            string path = folderPath.Replace(Application.dataPath, "Assets") + "/";
            string materialPath = path + "Materials/";

            EditorUtility.DisplayProgressBar(LOADING_TITLE, "Validating folders...", 0.1f);

            while (!AssetDatabase.IsValidFolder(materialPath))
            {
                yield return null;
            }

            EditorUtility.DisplayProgressBar(LOADING_TITLE, "Validating materials...", 0.2f);

            Material baseMaterial = null;
            UnityEngine.Shader shader = UnityEngine.Shader.Find("Universal Render Pipeline/Lit");

            Renderer characterRenderer = characterObject.GetComponentInChildren<Renderer>();
            if(characterRenderer != null)
            {
                if(characterRenderer.sharedMaterial != null)
                {
                    baseMaterial = characterRenderer.sharedMaterial;

                    if(baseMaterial != null)
                    {
                        shader = baseMaterial.shader;
                    }
                }
            }

            if(shader == null)
            {
                Debug.LogError("[Character Creator]: Failed to create character material!");

                EditorUtility.ClearProgressBar();

                yield break;
            }

            if((baseMaterial != null && !ReferenceEquals(baseMaterial, null) && AssetDatabase.GetAssetPath(baseMaterial).StartsWith("Packages")) || baseMaterial == null)
            {
                Material newMaterial = new Material(shader);
                newMaterial.name = characterObject.name + " Material";

                AssetDatabase.CreateAsset(newMaterial, materialPath + newMaterial.name + ".mat");

                baseMaterial = newMaterial;
            }

            characterRenderer.sharedMaterial = baseMaterial;

            EditorUtility.DisplayProgressBar(LOADING_TITLE, "Creating main prefab...", 0.3f);

            GameObject characterPrefab = PrefabUtility.SaveAsPrefabAsset(characterObject, path + characterObject.name + ".prefab");

            yield return null;

            EditorUtility.DisplayProgressBar(LOADING_TITLE, "Creating sub-prefabs...", 0.4f);

            Material blueMaterialVariant = CreateMaterial(materialPath, " (Blue)", baseMaterial, Color.blue);
            GameObject blueCharacterVariant = DuplicatePrefabs(path, " (Blue)", characterPrefab, blueMaterialVariant);

            EditorUtility.DisplayProgressBar(LOADING_TITLE, "Creating sub-prefabs...", 0.45f);

            Material greenMaterialVariant = CreateMaterial(materialPath, " (Green)", baseMaterial, Color.green);
            GameObject greenCharacterVariant = DuplicatePrefabs(path, " (Green)", characterPrefab, greenMaterialVariant);

            EditorUtility.DisplayProgressBar(LOADING_TITLE, "Creating sub-prefabs...", 0.5f);

            Material pinkMaterialVariant = CreateMaterial(materialPath, " (Pink)", baseMaterial, new Color(1, 0, 1));
            GameObject pinkCharacterVariant = DuplicatePrefabs(path, " (Pink)", characterPrefab, pinkMaterialVariant);

            EditorUtility.DisplayProgressBar(LOADING_TITLE, "Creating sub-prefabs...", 0.55f);

            Material purpleMaterialVariant = CreateMaterial(materialPath, " (Purple)", baseMaterial, new Color(0.5f, 0, 1));
            GameObject purpleCharacterVariant = DuplicatePrefabs(path, " (Purple)", characterPrefab, purpleMaterialVariant);

            EditorUtility.DisplayProgressBar(LOADING_TITLE, "Creating sub-prefabs...", 0.6f);

            Material redMaterialVariant = CreateMaterial(materialPath, " (Red)", baseMaterial, Color.red);
            GameObject redCharacterVariant = DuplicatePrefabs(path, " (Red)", characterPrefab, redMaterialVariant);

            EditorUtility.DisplayProgressBar(LOADING_TITLE, "Creating sub-prefabs...", 0.65f);

            Material tealMaterialVariant = CreateMaterial(materialPath, " (Teal)", baseMaterial, new Color(0, 1, 1));
            GameObject tealCharacterVariant = DuplicatePrefabs(path, " (Teal)", characterPrefab, tealMaterialVariant);

            EditorUtility.DisplayProgressBar(LOADING_TITLE, "Creating sub-prefabs...", 0.7f);

            Material yellowMaterialVariant = CreateMaterial(materialPath, " (Yellow)", baseMaterial, Color.yellow);
            GameObject yellowCharacterVariant = DuplicatePrefabs(path, " (Yellow)", characterPrefab, yellowMaterialVariant);

            EditorUtility.DisplayProgressBar(LOADING_TITLE, "Creating sub-prefabs...", 0.75f);

            GameObject.DestroyImmediate(characterObject);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            EditorUtility.ClearProgressBar();

            yield return null;

            if(EditorUtility.DisplayDialog("Character Creator", "Add new characters to database?", "Add", "Skip"))
            {
                LevelDatabase levelDatabase = EditorUtils.GetAsset<LevelDatabase>();
                if(levelDatabase != null)
                {
                    SerializedObject serializedObject = new SerializedObject(levelDatabase);
                    SerializedProperty levelElementsProperty = serializedObject.FindProperty("levelElements");

                    serializedObject.Update();

                    InsertElement(levelElementsProperty, LevelElement.Type.Block_Blue, blueCharacterVariant);
                    InsertElement(levelElementsProperty, LevelElement.Type.Block_Green, greenCharacterVariant);
                    InsertElement(levelElementsProperty, LevelElement.Type.Block_Pink, pinkCharacterVariant);
                    InsertElement(levelElementsProperty, LevelElement.Type.Block_Purple, purpleCharacterVariant);
                    InsertElement(levelElementsProperty, LevelElement.Type.Block_Red, redCharacterVariant);
                    InsertElement(levelElementsProperty, LevelElement.Type.Block_Teal, tealCharacterVariant);
                    InsertElement(levelElementsProperty, LevelElement.Type.Block_Yellow, yellowCharacterVariant);

                    serializedObject.ApplyModifiedProperties();

                    EditorUtility.SetDirty(levelDatabase);

                    Selection.activeObject = levelDatabase;
                }
                else
                {
                    Debug.LogError("[Character Creator]: LevelDatabase can't be found!");
                }
            }
            else
            {
                Selection.activeObject = characterPrefab;
            }

            Debug.Log("[Character Creator]: Character created successfully!");

            Close();
        }

        private void InsertElement(SerializedProperty arrayProperty, LevelElement.Type type, GameObject prefab)
        {
            bool elementInserted = false;

            for (int i = 0; i < arrayProperty.arraySize; i++)
            {
                SerializedProperty levelElementProperty = arrayProperty.GetArrayElementAtIndex(i);
                SerializedProperty elementType = levelElementProperty.FindPropertyRelative("elementType");

                if ((LevelElement.Type)elementType.intValue == type)
                {
                    SerializedProperty elementPrefab = levelElementProperty.FindPropertyRelative("prefab");
                    elementPrefab.objectReferenceValue = prefab;

                    elementInserted = true;

                    break;
                }
            }

            if(!elementInserted)
            {
                Debug.LogError(string.Format("[Character Creator]: Failed to insert {0} element!", type));
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(WatermelonEditor.Styles.box);
            characterObject = EditorGUILayout.ObjectField(new GUIContent("Character"), characterObject, typeof(GameObject), true) as GameObject;
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Create"))
            {
                if(characterObject == null)
                {
                    Debug.LogError("[Character Creator]: Prefab can't be null!");

                    return;
                }

                Renderer characterRenderer = characterObject.GetComponentInChildren<Renderer>();
                if(characterRenderer == null)
                {
                    Debug.LogError("[Character Creator]: Render component can't be found!");

                    return;
                }

                string path = EditorUtility.SaveFolderPanel("Select Character folder", "Assets/", "");
                if(!string.IsNullOrEmpty(path))
                {
                    GameObject parentObject = new GameObject(characterObject.name);
                    parentObject.transform.ResetGlobal();

                    GameObject graphicsObject = new GameObject("Graphics");
                    graphicsObject.transform.SetParent(parentObject.transform);
                    graphicsObject.transform.localPosition = new Vector3(0, 0.15f, 0);

                    characterObject.transform.SetParent(graphicsObject.transform);
                    characterObject.transform.ResetLocal();

                    Animator characterAnimator = characterObject.GetComponent<Animator>();
                    if (characterAnimator != null && characterAnimator.avatar != null && characterAnimator.isHuman)
                    {
                        AnimatorController animatorController = RuntimeEditorUtils.GetAssetByName<AnimatorController>("Character Animation Controller");
                        if (animatorController != null)
                        {
                            characterAnimator.runtimeAnimatorController = animatorController;
                        }

                        characterAnimator.applyRootMotion = false;

                        parentObject.AddComponent<HumanoidCharacterBehavior>();
                    }
                    else
                    {
                        parentObject.AddComponent<GenericCharacterBehavior>();
                    }

                    characterObject.SetActive(true);

                    EditorCoroutines.Execute(CharacterCoroutine(path, parentObject));
                }
            }
        }

        private GameObject DuplicatePrefabs(string folderPath, string name, GameObject file, Material material)
        {
            GameObject instanceVariant = (GameObject)PrefabUtility.InstantiatePrefab(file);

            Renderer renderer = instanceVariant.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material = material;
            }

            // Save prefab variant
            GameObject prefabVariant = PrefabUtility.SaveAsPrefabAsset(instanceVariant, folderPath + file.name + name + ".prefab");

            GameObject.DestroyImmediate(instanceVariant);

            return prefabVariant;
        }

        private Material CreateMaterial(string folderPath, string name, Material material, Color color)
        {
            Material variantMaterial = new Material(material);
            variantMaterial.parent = material;
            variantMaterial.color = color;

            AssetDatabase.CreateAsset(variantMaterial, folderPath + material.name + name + ".mat");

            return variantMaterial;
        }
    }
}
