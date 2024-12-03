using UnityEditor;
using System.Collections.Generic;

namespace Watermelon
{
    public class DefinesPostprocessor : AssetPostprocessor
    {
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void AssemblyReload()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += AssemblyReload;

                return;
            }

            EditorApplication.delayCall += UpdateDefines;
        }

        public static void UpdateDefines()
        {
            DefineManager.CheckAutoDefines();
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            if (!deletedAssets.IsNullOrEmpty())
            {
                List<DefineState> markedDefines = new List<DefineState>();
                List<RegisteredDefine> registeredDefines = DefinesSettings.GetDynamicDefines();
                foreach (string str in deletedAssets)
                {
                    foreach (var registeredDefine in registeredDefines)
                    {
                        if (registeredDefine.ContainsFile(str))
                        {
                            markedDefines.Add(new DefineState(registeredDefine.Define, false));
                        }
                    }
                }

                DefineManager.ChangeAutoDefinesState(markedDefines);
            }
        }
    }
}