using TMPro;
using UnityEngine;

namespace Watermelon.BusStop
{
    public class DebugGridBehavior : MonoBehaviour
    {
        [SerializeField] TextMeshPro textComponent;
        public TextMeshPro TextComponent => textComponent;

        [SerializeField] SpriteRenderer spriteRenderer;
        public SpriteRenderer SpriteRenderer => spriteRenderer;

        private static DebugGridBehavior[,] activeGridElements;
        public static DebugGridBehavior[,] Grid => activeGridElements;

        public static void SpawnGrid(GameObject prefab, int width, int height)
        {
            if (activeGridElements != null) Clear();

            activeGridElements = new DebugGridBehavior[width, height]; 
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    GameObject gridObject = Instantiate(prefab);
                    gridObject.transform.ResetGlobal();
                    gridObject.transform.position = LevelController.GetPosition(new ElementPosition(x, y));
                    gridObject.transform.position += new Vector3(0, 0, -0.1f);

                    DebugGridBehavior debugGridBehavior = gridObject.GetComponent<DebugGridBehavior>();
                    debugGridBehavior.spriteRenderer.color = new Color(1, 1, 1, 0.3f);
                    debugGridBehavior.textComponent.text = string.Format("({0}:{1})", x, y);

                    activeGridElements[x, y] = debugGridBehavior;
                }
            }
        }

        public static DebugGridBehavior Get(int x, int y)
        {
            int width = activeGridElements.GetLength(0);
            int height = activeGridElements.GetLength(1);

            if (x >= 0 && x < width && y >= 0 && y < height)
                return activeGridElements[x, y];

            return null;
        }

        public static void Set(int x, int y, Color color, string text)
        {
            DebugGridBehavior debugGridBehavior = Get(x, y);
            if(debugGridBehavior != null)
            {
                debugGridBehavior.spriteRenderer.color = color;
                debugGridBehavior.textComponent.text = text;
            }
        }

        public static void DrawDebugPath(int[,] path)
        {
            int width = path.GetLength(0);
            int height = path.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    DebugGridBehavior gridBehavior = DebugGridBehavior.Get(x, y);
                    if (gridBehavior != null)
                    {
                        gridBehavior.TextComponent.text = path[x, y].ToString();
                        gridBehavior.spriteRenderer.color = new Color(1, 1, 1, 0.3f);
                    }
                }
            }
        }

        public static void DrawDebugMap(ElementTypeMap map)
        {
            int width = map.Width;
            int height = map.Height;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    DebugGridBehavior gridBehavior = DebugGridBehavior.Get(x, y);
                    if (gridBehavior != null)
                    {
                        gridBehavior.TextComponent.text = map[x, y].ToString();
                        gridBehavior.spriteRenderer.color = new Color(1, 1, 1, 0.3f);
                    }
                }
            }
        }

        public static void Clear()
        {
            if (activeGridElements == null) return;

            int width = activeGridElements.GetLength(0);
            int height = activeGridElements.GetLength(1);

            for(int x = 0; x < width; x++)
            {
                for(int y = 0; y < height; y++)
                {
                    Destroy(activeGridElements[x, y].gameObject);
                }
            }

            activeGridElements = null;
        }
    }
}
