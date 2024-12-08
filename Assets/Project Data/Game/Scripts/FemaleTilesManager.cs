using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.Table;

namespace Watermelon.BusStop
{
    public class FemaleTileManager : MonoBehaviour
    {
        [SerializeField] private int rows = 5; // Number of rows
        [SerializeField] private int columns = 3; // Number of columns
        [SerializeField] private Vector3 tileSpacing = new Vector3(2f, 0, 2f); // Spacing between tiles
        [SerializeField] private GameObject tilePrefab; // Prefab for the 3D tile
        [SerializeField] private List<GameObject> characterPrefabs; // Array of character prefabs to spawn
        [SerializeField] private List<GameObject> allCharacterPrefabs; // Array of character prefabs to spawn
        [SerializeField] private Transform tileParent; // Parent object for tiles
        [SerializeField] private KeyCode removeKey = KeyCode.Space; // Key to remove a random character
        [SerializeField] private float moveSpeed = 2f; // Speed for lerping characters

        [SerializeField] public Transform[,] tiles; // Array to store tile transforms
        [SerializeField] public GameObject[,] characters; // Array to store characters
        [SerializeField] public bool isBusy = false; // Tracks if characters are currently being moved

        [SerializeField] public GameObject[] lowermostCharacters; // List of lowermost characters for each column
        [SerializeField] public LevelController _levelController; // List of lowermost characters for each column

        public List<GameObject> allMaleCharacters;
        public List<GameObject> femaleObjectsPool;

        void Start()
        {
            updateCharacters();
            InitializeTiles();
            InitializeCharacters();
            InitializeLowermostCharacters();
        }

        void updateCharacters() 
        {
            List<GameObject> tempMale = new List<GameObject>();
            List<MaleBehavior> tempMale2 = new List<MaleBehavior>();
            tempMale2 = FindObjectsOfType<MaleBehavior>().ToList();
            foreach (MaleBehavior obj in tempMale2) 
            {
                tempMale.Add(obj.gameObject);
            }
            AddUniqueGameObjects(tempMale);
            //allMaleCharacters = tempMale;
            //FilterUniqueCharacters();
        }

        public void AddUniqueGameObjects(List<GameObject> tempMale)
        {
            // Use a HashSet to track unique GameObjects in allMaleCharacters
            HashSet<GameObject> uniqueGameObjects = new HashSet<GameObject>(allMaleCharacters);

            // Iterate through tempMale and add only unique items
            foreach (GameObject obj in tempMale)
            {
                if (!uniqueGameObjects.Contains(obj))
                {
                    allMaleCharacters.Add(obj); // Add to the main list
                    //Create 3 female counterparts of newly detected male characters
                    for (int i = 0; i < 3; i++) 
                    {
                        foreach (GameObject obj2 in allCharacterPrefabs)
                        {
                            if (obj.GetComponent<HumanoidCharacterBehavior>().color == obj2.GetComponent<HumanoidCharacterBehavior>().color)
                            {
                                Debug.Log("zak1");
                                femaleObjectsPool.Add(obj2);
                            }
                        }
                    }
                }
            }
        }

        /*void FilterUniqueCharacters()
        {
            List<GameObject> uniqueGameObjects = GetUniqueGameObjectsByColor(allMaleCharacters);
            List<GameObject> uniqueGameObjects2 = new List<GameObject>();
            foreach (GameObject obj in uniqueGameObjects)
            {
                foreach (GameObject obj2 in allCharacterPrefabs)
                {
                    if (obj.GetComponent<HumanoidCharacterBehavior>().color == obj2.GetComponent<HumanoidCharacterBehavior>().color)
                    {
                        uniqueGameObjects2.Add(obj2);
                    }
                }
            }

            characterPrefabs.Clear();
            characterPrefabs = uniqueGameObjects2;
        }

        public static List<GameObject> GetUniqueGameObjectsByColor(List<GameObject> gameObjects)
        {
            Dictionary<string, GameObject> uniqueObjects = new Dictionary<string, GameObject>();

            foreach (GameObject obj in gameObjects)
            {
                if (obj != null)
                {
                    // Get the HumanoidCharacterBehavior component
                    HumanoidCharacterBehavior behavior = obj.GetComponent<HumanoidCharacterBehavior>();

                    // Check if the component exists and use its Color property
                    if (behavior != null && !uniqueObjects.ContainsKey(behavior.color))
                    {
                        uniqueObjects[behavior.color] = obj;
                    }
                }
            }

            return new List<GameObject>(uniqueObjects.Values);
        }*/

        void Update()
        {
            // Only allow input when not busy
            if (!isBusy && Input.GetKeyDown(removeKey))
            {
                RemoveRandomLowermostCharacter();
            }
        }

        /*//Get current level
        var ab = _levelController.database.levels[_levelController.levelIndex];
        Debug.Log("zak = " + ab.name);
        */

        void InitializeTiles()
        {
        //Get current level
            if (tilePrefab == null)
            {
                Debug.LogError("Tile Prefab is not assigned!");
                return;
            }

            tiles = new Transform[rows, columns];
            for (int col = 0; col < columns; col++) // Column-first numbering
            {
                for (int row = 0; row < rows; row++)
                {
                    Vector3 localPosition = new Vector3(col * tileSpacing.x, 0, -row * tileSpacing.z);
                    GameObject tile = Instantiate(tilePrefab, tileParent);
                    tile.transform.localPosition = localPosition;
                    tile.name = $"Tile_{(col * rows) + row + 1}";
                    tiles[row, col] = tile.transform;
                }
            }
        }

        void InitializeCharacters()
        {
            characters = new GameObject[rows, columns];
            for (int col = 0; col < columns; col++)
            {
                for (int row = 0; row < rows; row++)
                {
                    SpawnCharacter(row, col);
                }
            }
        }

        void InitializeLowermostCharacters()
        {
            lowermostCharacters = new GameObject[columns]; // Fixed size array for columns

            for (int col = 0; col < columns; col++)
            {
                lowermostCharacters[col] = characters[rows - 1, col];
            }
        }

        void SpawnCharacter(int row, int col)
        {
            if (femaleObjectsPool.Count == 0) return;
            int rand = Random.Range(0, femaleObjectsPool.Count);
            GameObject characterPrefab = femaleObjectsPool[rand];
            Debug.Log("zak2");
            GameObject character = Instantiate(characterPrefab, tiles[row, col]);
            femaleObjectsPool.RemoveAt(rand);
            character.transform.localPosition = Vector3.zero;
            characters[row, col] = character;

            // Assign row and column numbers to the character
            femaleInfo _femaleInfo = character.GetComponent<femaleInfo>();
            if (_femaleInfo != null)
            {
                _femaleInfo.Row = row;
                _femaleInfo.Column = col;
            }
        }

        public void RemoveCharacter(int row, int col)
        {
            if (row < 0 || row >= rows || col < 0 || col >= columns) return;

            if (characters[row, col] != null)
            {
                //Destroy(characters[row, col]);
                characters[row, col].transform.parent = null;
                characters[row, col] = null;
                StartCoroutine(MoveCharactersDown(col));
            }

            updateCharacters();
        }

        IEnumerator MoveCharactersDown(int col)
        {
            isBusy = true; // Prevent further input during movement

            List<GameObject> movingCharacters = new List<GameObject>();
            List<Vector3> targetPositions = new List<Vector3>();
            List<femaleInfo> characterInfos = new List<femaleInfo>();

            // Collect characters and their target positions
            for (int row = rows - 1; row > 0; row--)
            {
                if (characters[row - 1, col] != null)
                {
                    GameObject character = characters[row - 1, col];
                    movingCharacters.Add(character);
                    targetPositions.Add(tiles[row, col].position);
                    characters[row, col] = character;
                    characters[row - 1, col] = null;

                    // Collect CharacterInfo for updating row numbers
                    femaleInfo characterInfo = character.GetComponent<femaleInfo>();
                    if (characterInfo != null)
                    {
                        characterInfos.Add(characterInfo);
                    }
                }
            }

            // Move all characters simultaneously
            float elapsedTime = 0;
            while (elapsedTime < 1f)
            {
                for (int i = 0; i < movingCharacters.Count; i++)
                {
                    if (movingCharacters[i] != null)
                    {
                        movingCharacters[i].transform.position = Vector3.Lerp(
                            movingCharacters[i].transform.position,
                            targetPositions[i],
                            elapsedTime
                        );
                    }
                }

                elapsedTime += Time.deltaTime * moveSpeed;
                yield return null;
            }

            // Snap characters to final positions and update CharacterInfo row numbers
            for (int i = 0; i < movingCharacters.Count; i++)
            {
                if (movingCharacters[i] != null)
                {
                    movingCharacters[i].transform.position = targetPositions[i];

                    // Update row number in CharacterInfo
                    if (movingCharacters[i].GetComponent<femaleInfo>() != null)
                    {
                        movingCharacters[i].GetComponent<femaleInfo>().Row += 1; // Increment row as character moves down
                    }
                }
            }

            // Spawn a new character at the top of the column
            SpawnCharacter(0, col);
            isBusy = false; // Allow further input after movement
            UpdateLowermostCharacters(col); // Update lowermost characters list after characters are done moving
        }

        void RemoveRandomLowermostCharacter()
        {
            // Collect indices of valid columns (columns with non-null lowermost characters)
            List<int> validColumns = new List<int>();

            for (int col = 0; col < lowermostCharacters.Length; col++)
            {
                if (lowermostCharacters[col] != null)
                {
                    validColumns.Add(col);
                }
            }

            // If no valid columns exist, return early
            if (validColumns.Count == 0) return;

            // Choose a random valid column and remove the lowermost character
            int randomCol = validColumns[Random.Range(0, validColumns.Count)];
            GameObject characterToRemove = lowermostCharacters[randomCol];

            if (characterToRemove != null)
            {
                femaleInfo info = characterToRemove.GetComponent<femaleInfo>();
                if (info != null)
                {
                    RemoveCharacter(info.Row, info.Column); // Remove the character based on its row and column
                }
            }
        }

        void UpdateLowermostCharacters(int col)
        {
            lowermostCharacters[col] = null; // Default to null if the column is empty

            for (int row = rows - 1; row >= 0; row--)
            {
                if (characters[row, col] != null)
                {
                    lowermostCharacters[col] = characters[row, col];
                    break; // Stop once the first non-null character is found from the bottom
                }
            }
        }
    }
}