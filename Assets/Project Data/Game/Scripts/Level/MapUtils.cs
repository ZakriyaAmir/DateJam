using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Watermelon.BusStop
{
    public static class MapUtils
    {
        private static readonly int[] OFFSET_X = new int[4] { -1, 0, 1, 0 };
        private static readonly int[] OFFSET_Y = new int[4] { 0, 1, 0, -1 };

        private static readonly int2[] OFFSET = new int2[4] { new int2(-1, 0), new int2(0, 1), new int2(1, 0), new int2(0, -1) };

        public static List<ElementPosition> GetAvailableElementsNew(ElementTypeMap elementsMap)
        {
            var width = elementsMap.Width;
            var height = elementsMap.Height;

            var map = new bool[width, height];
            var availableElements = new List<ElementPosition>();

            var reachableEmptySpaces = new List<ElementPosition>();
            var currentGen = new List<ElementPosition>();

            // First Row
            for (int x = 0; x < width; x++)
            {
                map[x, 0] = elementsMap[x, 0] == LevelElement.Type.Empty;

                if (map[x, 0])
                {
                    reachableEmptySpaces.Add(new ElementPosition(x, 0));
                    currentGen.Add(new ElementPosition(x, 0));
                }

                if(LevelElement.IsCharacterElement(elementsMap[x, 0]))
                {
                    availableElements.Add(new ElementPosition(x, 0));
                }
            }

            while(currentGen.Count > 0)
            {
                var nextGen = new List<ElementPosition>();

                for(int i = 0; i < currentGen.Count; i++)
                {
                    var emptySpace = currentGen[i];
                    for(int j = 0; j < OFFSET.Length; j++)
                    {
                        var testSpace = emptySpace + OFFSET[j];

                        if (!ValidatePos(emptySpace + OFFSET[j])) continue;

                        if(elementsMap[testSpace] == LevelElement.Type.Empty)
                        {
                            if (!reachableEmptySpaces.Contains(testSpace))
                            {
                                nextGen.Add(testSpace);
                                reachableEmptySpaces.Add(testSpace);
                            }
                        } else if(LevelElement.IsCharacterElement(elementsMap[testSpace]) && !availableElements.Contains(testSpace))
                        {
                            availableElements.Add(testSpace);
                        }
                    }
                }

                currentGen = nextGen;
            }

            return availableElements;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ValidatePos(ElementPosition pos)
        {
            var width = LevelController.LoadedStageData.Width;
            var height = LevelController.LoadedStageData.Height;

            return pos.X >= 0 && pos.X < width && pos.Y >= 0 && pos.Y < height;
        }

        public static List<ElementPosition> GetAvailableElements(ElementTypeMap elementsMap)
        {
            List<ElementPosition> availableElements = new List<ElementPosition>();

            int levelWidth = elementsMap.Width;
            int levelHeight = elementsMap.Height;

            bool isEmptyElementExists = false;

            for (int x = 0; x < levelWidth; x++)
            {
                if(LevelElement.IsCharacterElement(elementsMap[x, 0]))
                {
                    availableElements.Add(new ElementPosition(x, 0));
                }
                
                if(elementsMap[x, 0] == LevelElement.Type.Empty)
                {
                    if (levelHeight > 1)
                    {
                        if (LevelElement.IsCharacterElement(elementsMap[x, 1]))
                        {
                            availableElements.Add(new ElementPosition(x, 1));
                        }
                    }

                    isEmptyElementExists = true;
                }
            }

            if(isEmptyElementExists)
            {
                for (int y = 1; y < levelHeight; y++)
                {
                    isEmptyElementExists = false;

                    for (int x = 0; x < levelWidth; x++)
                    {
                        if (elementsMap[x, y] == LevelElement.Type.Empty)
                        {
                            for (int f = 0; f < 4; f++)
                            {
                                int tempX = x + OFFSET_X[f];
                                int tempY = y + OFFSET_Y[f];

                                if (tempX >= 0 && tempX < levelWidth && tempY >= 0 && tempY < levelHeight)
                                {
                                    if (LevelElement.IsCharacterElement(elementsMap[tempX, tempY]))
                                    {
                                        if(availableElements.FindIndex(x => x.Equals(tempX, tempY)) == -1)
                                            availableElements.Add(new ElementPosition(tempX, tempY));
                                    }
                                }
                            }

                            isEmptyElementExists = true;
                        }
                    }

                    if (!isEmptyElementExists)
                        break;
                }
            }

            return availableElements;
        }

        private static int[,] GetPath(int startX, int startY, ElementTypeMap elementsMap, out ElementPosition targetPosition)
        {
            int levelWidth = elementsMap.Width;
            int levelHeight = elementsMap.Height;

            int[,] map = new int[levelWidth, levelHeight];

            int step = 0;

            for (int x = 0; x < levelWidth; x++)
            {
                for (int y = 0; y < levelHeight; y++)
                {
                    if (elementsMap[x, y] == LevelElement.Type.Empty)
                        map[x, y] = -1;
                    else
                        map[x, y] = -2;
                }
            }

            map[startX, startY] = 0;

            while (step <= levelWidth * levelHeight)
            {
                for (int x = 0; x < levelWidth; x++)
                {
                    for (int y = 0; y < levelHeight; y++)
                    {
                        if (map[x, y] == step)
                        {
                            if (x - 1 >= 0 && map[x - 1, y] != -2 && map[x - 1, y] == -1)
                            {
                                map[x - 1, y] = step + 1;
                            }
                            if (x + 1 < levelWidth && map[x + 1, y] != -2 && map[x + 1, y] == -1)
                            {
                                map[x + 1, y] = step + 1;
                            }
                            if (y + 1 < levelHeight && map[x, y + 1] != -2 && map[x, y + 1] == -1)
                            {
                                map[x, y + 1] = step + 1;
                            }
                            if (y - 1 >= 0 && map[x, y - 1] != -2 && map[x, y - 1] == -1)
                            {
                                map[x, y - 1] = step + 1;

                                if (y - 1 == 0)
                                {
                                    targetPosition = new ElementPosition(x, y - 1);

                                    return map;
                                }
                            }
                        }
                    }
                }

                step++;
            }

            targetPosition = new ElementPosition(-1, -1);

            return null;
        }

        public static Vector3[] CalculatePath(int startX, int startY, ElementTypeMap elementsMap)
        {
            if(startY == 0) return new Vector3[] { LevelController.GetPosition(startX, startY - 1) };

            int levelWidth = elementsMap.Width;
            int levelHeight = elementsMap.Height;

            ElementPosition targetPosition;
            int[,] map = GetPath(startX, startY, elementsMap, out targetPosition);

            if (map == null) return null;

            List<Vector3> path = new List<Vector3>();

            int finalSteps = map[targetPosition.X, targetPosition.Y] - 1;

            int currentPosX = targetPosition.X;
            int currentPosY = targetPosition.Y;

            path.Add(LevelController.GetPosition(currentPosX, currentPosY - 1));

            if (finalSteps > 0)
            {
                for (int i = 0; i < finalSteps; i++)
                {
                    for (int f = 0; f < 4; f++)
                    {
                        int tempX = currentPosX + OFFSET_X[f]; // travel in an adiacent cell from the current position
                        int tempY = currentPosY + OFFSET_Y[f];

                        if (tempX >= 0 && tempX < levelWidth && tempY >= 0 && tempY < levelHeight) //here you should insert whatever conditions should apply for your position (xx, yy)
                        {
                            if (map[tempX, tempY] >= 0 && map[tempX, tempY] < map[currentPosX, currentPosY])
                            {
                                currentPosX = tempX;
                                currentPosY = tempY;

                                path.Insert(0, LevelController.GetPosition(tempX, tempY));

                                break;
                            }
                        }
                    }
                }
            }

            return path.ToArray();
        }
    }
}
