using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.BusStop
{
    public class TileManager : MonoBehaviour
    {
        private PoolGeneric<Transform> cellPool;
        private static PoolGeneric<Transform> borderPool;
        private PoolGeneric<Transform> outerCornerPool;
        private PoolGeneric<Transform> innerCornerPool;

        [SerializeField] int topTilesWidth = 10;

        private Dictionary<Direction, BorderInfo>[,] borders;

        private class BorderInfo
        {
            public Transform border;
            public float scaleOffset;
            public Vector3 offset;

            public Vector3 position;

            public void SetPos(Vector3 pos, Quaternion rot)
            {
                if(border == null) border = borderPool.GetPooledComponent();

                border.transform.rotation = rot;
                position = pos;

                border.position = position + offset;
                border.localScale = Vector3.one - Vector3.right * scaleOffset;
            }

            public void SetOffset(Vector3 posOffset, float scaleOffset)
            {
                offset += posOffset;
                this.scaleOffset += scaleOffset;

                if(border != null)
                {
                    border.position = position + offset;
                    border.localScale = Vector3.one - Vector3.right * this.scaleOffset;
                }
            }
        }

        private BorderInfo GetBorderInfo(int i, int j, Direction direction)
        {
            if (borders == null) borders = new Dictionary<Direction, BorderInfo>[LevelController.LoadedStageData.Width, LevelController.LoadedStageData.Height];

            var cellBorders = borders[i, j];
            if (cellBorders == null)
            {
                cellBorders = new Dictionary<Direction, BorderInfo>();
                borders[i, j] = cellBorders;
            }

            if (cellBorders.ContainsKey(direction))
            {
                return cellBorders[direction];
            } else
            {
                var info = new BorderInfo();

                cellBorders.Add(direction, info);

                return info;
            }
        }

        public void Init(TileData data)
        {
            StartCoroutine(ClearPools(cellPool, borderPool, outerCornerPool, innerCornerPool));

            cellPool = new PoolGeneric<Transform>(new PoolSettings(data.cellPrefab, 50, true));
            borderPool = new PoolGeneric<Transform>(new PoolSettings(data.borderPrefab, 50, true));
            outerCornerPool = new PoolGeneric<Transform>(new PoolSettings(data.outerCornerPrefab, 20, true));
            innerCornerPool = new PoolGeneric<Transform>(new PoolSettings(data.innerCornerPrefab, 20, true));
        }

        private IEnumerator ClearPools(params Pool[] pools)
        {
            var wait = new WaitForSeconds(0.2f);

            for (int i = 0; i < pools.Length; i++)
            {
                var pool = pools[i];

                if(pool == null) continue;

                yield return wait;

                pool.Clear();
            }
        }

        public void GenerateTileMap(ElementTypeMap map)
        {
            borderPool.ReturnToPoolEverything();
            outerCornerPool.ReturnToPoolEverything();
            innerCornerPool.ReturnToPoolEverything();
            cellPool.ReturnToPoolEverything();

            borders = new Dictionary<Direction, BorderInfo>[map.Width, map.Height];

            var width = map.Width;
            var depth = map.Height;

            var startX = -width / 2f;
            var startZ = LevelController.StartZ;

            for (int z = -2; z < 0; z++)
            {
                for (int x = -topTilesWidth; x < width + topTilesWidth; x++)
                {
                    var cellPos = new Vector3(startX + x + 0.5f, LevelController.PosY, startZ - z - 0.5f);

                    var cell = cellPool.GetPooledComponent();
                    cell.transform.position = cellPos;
                    cell.transform.localScale = Vector3.one;
                }
            }

            for(int x = 0; x < width; x++)
            {
                for(int z = 0; z < depth; z++)
                {
                    var cellType = map[x, z];

                    var cellPos = new Vector3(startX + x + 0.5f, LevelController.PosY, startZ - z - 0.5f);

                    if (cellType != LevelElement.Type.Wall)
                    {
                        var cell = cellPool.GetPooledComponent();
                        cell.transform.position = cellPos;
                        cell.transform.localScale = Vector3.one;

                        // RIGHT
                        if (x == width - 1)
                        {
                            var border = GetBorderInfo(x, z, Direction.Right);
                            border.SetPos(cellPos + Vector3.right * 0.5f, Quaternion.Euler(0, -90, 0));

                            if (z == 0)
                            {
                                border.SetOffset(Vector3.back * 0.08f, 0.16f);

                                var corner = innerCornerPool.GetPooledComponent();

                                corner.transform.position = cellPos + Vector3.right * 0.5f + Vector3.forward * 0.66f;

                                corner.rotation = Quaternion.Euler(0, 0, 0);
                            }
                        }

                        // LEFT
                        if (x == 0)
                        {
                            var border = GetBorderInfo(x, z, Direction.Left);
                            border.SetPos(cellPos + Vector3.left * 0.5f, Quaternion.Euler(0, 90, 0));

                            if (z == 0)
                            {
                                border.SetOffset(Vector3.back * 0.08f, 0.16f);

                                var corner = innerCornerPool.GetPooledComponent();

                                corner.transform.position = cellPos + Vector3.left * 0.5f + Vector3.forward * 0.66f;

                                corner.rotation = Quaternion.Euler(0, 90, 0);
                            }
                        }

                        // BOTTOM
                        if (z == depth - 1)
                        {
                            var border = GetBorderInfo(x, z, Direction.Back);
                            border.SetPos(cellPos + Vector3.back * 0.5f, Quaternion.Euler(0, 0, 0));
                        }

                        // TOP
                        if (z == 0)
                        {

                        }

                        if(x == 0 && z == depth - 1)
                        {
                            var corner = outerCornerPool.GetPooledComponent();

                            corner.transform.position = cellPos + Vector3.left * 0.5f + Vector3.back * 0.5f;

                            corner.rotation = Quaternion.Euler(0, 90, 0);

                            GetBorderInfo(x, z, Direction.Left).SetOffset(Vector3.forward * 0.08f, 0.16f);
                            GetBorderInfo(x, z, Direction.Back).SetOffset(Vector3.right * 0.08f, 0.16f);
                        }

                        if (x == width - 1 && z == depth - 1)
                        {
                            var corner = outerCornerPool.GetPooledComponent();

                            corner.transform.position = cellPos + Vector3.right * 0.5f + Vector3.back * 0.5f;

                            corner.rotation = Quaternion.Euler(0, 0, 0);

                            GetBorderInfo(x, z, Direction.Right).SetOffset(Vector3.forward * 0.08f, 0.16f);
                            GetBorderInfo(x, z, Direction.Back).SetOffset(Vector3.left * 0.08f, 0.16f);
                        }
                    } 
                    else
                    {

                        // RIGHT
                        if (x != width - 1)
                        {
                            if(map[x + 1, z] != LevelElement.Type.Wall)
                            {
                                var borderData = GetBorderInfo(x, z, Direction.Right);
                                borderData.SetPos(cellPos + Vector3.right * 0.5f, Quaternion.Euler(0, 90, 0));
                                var border = borderData.border;

                                // RIGHT TOP CORNER

                                if(z == 0)
                                {
                                    var corner = innerCornerPool.GetPooledComponent();

                                    corner.transform.position = cellPos + Vector3.right * 0.5f + Vector3.forward * 0.66f;

                                    corner.rotation = Quaternion.Euler(0, 90, 0);

                                    borderData.SetOffset(Vector3.back * 0.08f, 0.16f);
                                    GetBorderInfo(x, z, Direction.Forward).SetOffset(Vector3.left * 0.16f, 0.32f);

                                } else if (map[x, z - 1] != LevelElement.Type.Wall)
                                {
                                    var corner = innerCornerPool.GetPooledComponent();

                                    corner.transform.position = cellPos + Vector3.right * 0.5f + Vector3.forward * 0.5f;

                                    corner.rotation = Quaternion.Euler(0, 90, 0);

                                    var borderScale = border.transform.localScale;
                                    borderScale.x -= 0.32f;
                                    border.transform.localScale = borderScale;

                                    borderData.SetOffset(Vector3.back * 0.16f, 0.32f);

                                    GetBorderInfo(x, z, Direction.Forward).SetOffset(Vector3.left * 0.16f, 0.32f);
                                }

                                // RIGHT BOTTOM CORNER

                                if(z == depth - 1)
                                {
                                    var corner = outerCornerPool.GetPooledComponent();

                                    corner.transform.position = cellPos + Vector3.right * 0.5f + Vector3.back * 0.5f;

                                    corner.rotation = Quaternion.Euler(0, 90, 0);

                                    borderData.SetOffset(Vector3.forward * 0.08f, 0.16f);

                                    GetBorderInfo(x, z, Direction.Back).SetOffset(Vector3.left * 0.16f, 0.32f);
                                } else if (map[x, z + 1] != LevelElement.Type.Wall)
                                {
                                    var corner = innerCornerPool.GetPooledComponent();

                                    corner.transform.position = cellPos + Vector3.right * 0.5f + Vector3.back * 0.5f;

                                    corner.rotation = Quaternion.Euler(0, 180, 0);

                                    borderData.SetOffset(Vector3.forward * 0.16f, 0.32f);

                                    GetBorderInfo(x, z, Direction.Back).SetOffset(Vector3.left * 0.16f, 0.32f);
                                }
                            } else
                            {
                                if(z != 0)
                                {
                                    if(map[x + 1, z - 1] != LevelElement.Type.Wall && map[x, z - 1] == LevelElement.Type.Wall)
                                    {
                                        var corner = outerCornerPool.GetPooledComponent();

                                        corner.transform.position = cellPos + Vector3.right * 0.5f + Vector3.forward * 0.5f;

                                        corner.rotation = Quaternion.Euler(0, 90, 0);

                                        //GetBorderInfo(x, y, Direction.Up).SetOffset(Vector3.left * 0.16f, 0.32f);

                                        GetBorderInfo(x, z - 1, Direction.Right).SetOffset(Vector3.forward * 0.08f, 0.16f);
                                        GetBorderInfo(x + 1, z, Direction.Forward).SetOffset(Vector3.right * 0.08f, 0.16f);
                                    }
                                }

                                if(z != depth - 1)
                                {
                                    if (map[x + 1, z + 1] != LevelElement.Type.Wall && map[x, z + 1] == LevelElement.Type.Wall)
                                    {
                                        var corner = outerCornerPool.GetPooledComponent();

                                        corner.transform.position = cellPos + Vector3.right * 0.5f + Vector3.back * 0.5f;

                                        corner.rotation = Quaternion.Euler(0, 180, 0);

                                        //GetBorderInfo(x, y, Direction.Up).SetOffset(Vector3.left * 0.16f, 0.32f);

                                        GetBorderInfo(x, z + 1, Direction.Right).SetOffset(Vector3.back * 0.08f, 0.16f);
                                        GetBorderInfo(x + 1, z, Direction.Back).SetOffset(Vector3.right * 0.08f, 0.16f);
                                    }
                                }
                            }
                        } else
                        {
                            if (z != 0 && map[x, z - 1] != LevelElement.Type.Wall)
                            {
                                var corner = outerCornerPool.GetPooledComponent();

                                corner.transform.position = cellPos + Vector3.right * 0.5f + Vector3.forward * 0.5f;
                                corner.rotation = Quaternion.Euler(0, 0, 0);

                                GetBorderInfo(x, z, Direction.Forward).SetOffset(Vector3.left * 0.08f, 0.16f);
                                GetBorderInfo(x, z - 1, Direction.Right).SetOffset(Vector3.forward * 0.08f, 0.16f);
                            }

                            if (z != depth - 1 && map[x, z + 1] != LevelElement.Type.Wall)
                            {
                                var corner = outerCornerPool.GetPooledComponent();

                                corner.transform.position = cellPos + Vector3.right * 0.5f + Vector3.back * 0.5f;
                                corner.rotation = Quaternion.Euler(0, -90, 0);

                                GetBorderInfo(x, z, Direction.Back).SetOffset(Vector3.left * 0.08f, 0.16f);
                                GetBorderInfo(x, z + 1, Direction.Right).SetOffset(Vector3.back * 0.08f, 0.16f);
                            }
                        }

                        // LEFT
                        if (x != 0)
                        {
                            if (map[x - 1, z] != LevelElement.Type.Wall)
                            {
                                var borderData = GetBorderInfo(x, z, Direction.Left);
                                borderData.SetPos(cellPos + Vector3.left * 0.5f, Quaternion.Euler(0, -90, 0));
                                var border = borderData.border;

                                // LEFT TOP CORNER
                                if (z == 0)
                                {
                                    var corner = innerCornerPool.GetPooledComponent();

                                    corner.transform.position = cellPos + Vector3.left * 0.5f + Vector3.forward * 0.66f;

                                    corner.rotation = Quaternion.Euler(0, 0, 0);

                                    borderData.SetOffset(Vector3.back * 0.08f, 0.16f);

                                    GetBorderInfo(x, z, Direction.Forward).SetOffset(Vector3.right * 0.16f, 0.32f);

                                }
                                else if(map[x, z - 1] != LevelElement.Type.Wall)
                                {
                                    var corner = innerCornerPool.GetPooledComponent();

                                    corner.transform.position = cellPos + Vector3.left * 0.5f + Vector3.forward * 0.5f;

                                    corner.rotation = Quaternion.Euler(0, 0, 0);

                                    borderData.SetOffset(Vector3.back * 0.16f, 0.32f);

                                    GetBorderInfo(x, z, Direction.Forward).SetOffset(Vector3.right * 0.16f, 0.32f);
                                }

                                // LEFT BOTTOM CORNER

                                if (z == depth - 1)
                                {
                                    var corner = outerCornerPool.GetPooledComponent();

                                    corner.transform.position = cellPos + Vector3.left * 0.5f + Vector3.back * 0.5f;

                                    corner.rotation = Quaternion.Euler(0, 0, 0);

                                    borderData.SetOffset(Vector3.forward * 0.08f, 0.16f);

                                    GetBorderInfo(x, z, Direction.Back).SetOffset(Vector3.right * 0.16f, 0.32f);
                                }
                                else if (map[x, z + 1] != LevelElement.Type.Wall)
                                {
                                    var corner = innerCornerPool.GetPooledComponent();

                                    corner.transform.position = cellPos + Vector3.left * 0.5f + Vector3.back * 0.5f;

                                    corner.rotation = Quaternion.Euler(0, -90, 0);

                                    borderData.SetOffset(Vector3.forward * 0.16f, 0.32f);

                                    GetBorderInfo(x, z, Direction.Back).SetOffset(Vector3.right * 0.16f, 0.32f);
                                }
                            } else
                            {
                                if (z != 0)
                                {
                                    if (map[x - 1, z - 1] != LevelElement.Type.Wall && map[x, z - 1] == LevelElement.Type.Wall)
                                    {
                                        var corner = outerCornerPool.GetPooledComponent();

                                        corner.transform.position = cellPos + Vector3.left * 0.5f + Vector3.forward * 0.5f;

                                        corner.rotation = Quaternion.Euler(0, 0, 0);

                                        GetBorderInfo(x, z - 1, Direction.Left).SetOffset(Vector3.forward * 0.08f, 0.16f);
                                        GetBorderInfo(x - 1, z, Direction.Forward).SetOffset(Vector3.left * 0.08f, 0.16f);
                                    }
                                }

                                if (z != depth - 1)
                                {
                                    if (map[x - 1, z + 1] != LevelElement.Type.Wall && map[x, z + 1] == LevelElement.Type.Wall)
                                    {
                                        var corner = outerCornerPool.GetPooledComponent();

                                        corner.transform.position = cellPos + Vector3.left * 0.5f + Vector3.back * 0.5f;

                                        corner.rotation = Quaternion.Euler(0, -90, 0);

                                        GetBorderInfo(x, z + 1, Direction.Left).SetOffset(Vector3.back * 0.08f, 0.16f);
                                        GetBorderInfo(x - 1, z, Direction.Back).SetOffset(Vector3.left * 0.08f, 0.16f);
                                    }
                                }
                            }
                        } else
                        {
                            if (z != 0 && map[x, z - 1] != LevelElement.Type.Wall)
                            {
                                var corner = outerCornerPool.GetPooledComponent();

                                corner.transform.position = cellPos + Vector3.left * 0.5f + Vector3.forward * 0.5f;
                                corner.rotation = Quaternion.Euler(0, 90, 0);

                                GetBorderInfo(x, z, Direction.Forward).SetOffset(Vector3.right * 0.08f, 0.16f);
                                GetBorderInfo(x, z - 1, Direction.Left).SetOffset(Vector3.forward * 0.08f, 0.16f);
                            }

                            if (z != depth - 1 && map[x, z + 1] != LevelElement.Type.Wall)
                            {
                                var corner = outerCornerPool.GetPooledComponent();

                                corner.transform.position = cellPos + Vector3.left * 0.5f + Vector3.back * 0.5f;
                                corner.rotation = Quaternion.Euler(0, 180, 0);

                                GetBorderInfo(x, z, Direction.Back).SetOffset(Vector3.right * 0.08f, 0.16f);
                                GetBorderInfo(x, z + 1, Direction.Left).SetOffset(Vector3.back * 0.08f, 0.16f);
                            }
                        }

                        // TOP
                        if (z != 0)
                        {
                            if(map[x, z - 1] != LevelElement.Type.Wall)
                            {
                                var border = GetBorderInfo(x, z, Direction.Forward);

                                border.SetPos(cellPos + Vector3.forward * 0.5f, Quaternion.Euler(0, 0, 0));
                            }
                        } else
                        {
                            var border = GetBorderInfo(x, z, Direction.Forward);

                            border.SetPos(cellPos + Vector3.forward * 0.66f, Quaternion.Euler(0, 0, 0));
                        }

                        // BOTTOM
                        if (z != depth - 1)
                        {
                            if(map[x, z + 1] != LevelElement.Type.Wall)
                            {
                                var border = GetBorderInfo(x, z, Direction.Back);

                                border.SetPos(cellPos + Vector3.back * 0.5f, Quaternion.Euler(0, 180, 0));
                            }
                        } else
                        {
                            if(x != 0)
                            {
                                if (map[x - 1, z] != LevelElement.Type.Wall)
                                {
                                    GetBorderInfo(x - 1, z, Direction.Back).SetOffset(Vector3.left * 0.08f, 0.16f);
                                }
                            }

                            if (x != width - 1)
                            {
                                if (map[x + 1, z] != LevelElement.Type.Wall)
                                {
                                    GetBorderInfo(x + 1, z, Direction.Back).SetOffset(Vector3.right * 0.08f, 0.16f);
                                }
                            }
                        }
                    }
                }
            }

            var leftBorder = borderPool.GetPooledComponent();
            var rightBorder = borderPool.GetPooledComponent();

            var leftOffset = 10f;
            if (map[0, 0] != LevelElement.Type.Wall) leftOffset += 0.32f;

            var rightOffset = 10f;
            if (map[width - 1, 0] != LevelElement.Type.Wall) rightOffset += 0.32f;

            leftBorder.position = new Vector3(startX - leftOffset, LevelController.PosY, startZ + 0.16f);
            leftBorder.localScale = new Vector3(20, 1, 1);
            leftBorder.rotation = Quaternion.Euler(0, 0, 0);

            rightBorder.position = new Vector3(startX + width + rightOffset, LevelController.PosY, startZ + 0.16f);
            rightBorder.localScale = new Vector3(20, 1, 1);
            rightBorder.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
}