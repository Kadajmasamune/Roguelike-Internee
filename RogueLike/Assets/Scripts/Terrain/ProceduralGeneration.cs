using System;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.InputSystem.Composites;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

//FIXXXXX
//Fix Out of Rooms Bug
public class ProceduralGeneration : MonoBehaviour
{
    [SerializeField] Tilemap TileMap;


    [SerializeField] Tile GroundTile;
    [SerializeField] Tile Wall;
    [SerializeField] Tile Pillar;

    [SerializeField] Grid MapGrid;

    [SerializeField] int TileSpacing = 2;

    [SerializeField] GameObject Player;


    public float NoiseScale = 0.1f;

    private int Width;
    private int Height;

    [SerializeField] int RoomWidth;
    [SerializeField] int RoomHeight;


    const int EMPTY = 0;
    const int WALL = 1;
    const int SOLID = 2;
    const int PILLAR = 3;

    public int CellSize = 16;
    public int RoomID = 0;
    public int[,] Map;
    public int[,] Decor;
    public double fill = 0.4;
    public int smooth = 2;
    public List<List<float[]>> Regions;
    public List<Room> Rooms;
    private float Timer = 0f;

    private int terrainExtensionCount = 0;
    private int[,] lastMap;

    void Start()
    {
        Width = RoomWidth / CellSize + (RoomWidth % CellSize > 0 ? 1 : 0);
        Height = RoomHeight / CellSize + (RoomHeight % CellSize > 0 ? 1 : 0);
        Map = new int[Width, Height];
        lastMap = Map;
        Regions = new List<List<float[]>>();
        Rooms = new List<Room>();
        Timer = Time.time;



        Debug.Log($"Map Size: Width={Width}, Height={Height}");
        RandomFill(Map);

        int open = 0;
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                if (Map[x, y] == 0) open++;
        Debug.Log("Open Cells" + open);


        for (int i = 0; i < smooth; i++)
            SmoothAll(Map);
        SetupRooms(Map);
        ConnectRooms();
        DrawMapWithRoomEdges(Map);
        PrintMapToConsole(Map);
        SpawnPlayerOnPlacedTile();
    }


    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Space))
    //     {
    //         int[,] newMap = new int[Width, Height];

    //         ExtendTerrain(newMap);
    //         lastMap = newMap;
    //         terrainExtensionCount++;
    //     }
    // }



    void ExtendTerrain(int[,] NewMap)
    {

        // 1. Generate new terrain
        RandomFill(NewMap);
        for (int i = 0; i < smooth; i++)
            SmoothAll(NewMap);
        SetupRooms(NewMap);

        // 2. Find rightmost edge from lastMap
        int[] rightEdge = null;
        for (int y = 0; y < Height; y++)
        {
            for (int x = Width - 1; x >= 0; x--)
            {
                if (lastMap[x, y] == EMPTY)
                {
                    rightEdge = new int[] { x, y };
                    break;
                }
            }
            if (rightEdge != null) break;
        }

        // 3. Find leftmost edge from NewMap
        int[] leftEdge = null;
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (NewMap[x, y] == EMPTY)
                {
                    leftEdge = new int[] { x, y };
                    break;
                }
            }
            if (leftEdge != null) break;
        }

        // 4. Draw all terrains so far
        TileMap.ClearAllTiles();
        for (int t = 0; t <= terrainExtensionCount; t++)
        {
            int[,] mapToDraw = (t == terrainExtensionCount) ? NewMap : lastMap;
            int xOffset = t * Width * TileSpacing;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Vector3Int tilePos = new Vector3Int(x * TileSpacing + xOffset, y * TileSpacing, 0);
                    if (mapToDraw[x, y] == EMPTY)
                        TileMap.SetTile(tilePos, GroundTile);
                    else if (mapToDraw[x, y] == SOLID)
                        TileMap.SetTile(tilePos, Wall);
                }
            }
        }

        // 5. Connect the two terrains with a bridge
        if (rightEdge != null && leftEdge != null)
        {
            int xOffset = terrainExtensionCount * Width;
            int[] leftEdgeWorld = new int[] { leftEdge[0] + xOffset, leftEdge[1] };
            int[] rightEdgeWorld = new int[] { rightEdge[0] + (terrainExtensionCount - 1) * Width, rightEdge[1] };
            float len = Vector2.Distance(new Vector2(rightEdgeWorld[0], rightEdgeWorld[1]), new Vector2(leftEdgeWorld[0], leftEdgeWorld[1]));
            for (int i = 0; i <= len; i++)
            {
                float ratio = i / len;
                float x1 = Mathf.Lerp(rightEdgeWorld[0], leftEdgeWorld[0], ratio);
                float y1 = Mathf.Lerp(rightEdgeWorld[1], leftEdgeWorld[1], ratio);
                Vector3Int tilePos = new Vector3Int(Mathf.RoundToInt(x1) * TileSpacing, Mathf.RoundToInt(y1) * TileSpacing, 0);
                TileMap.SetTile(tilePos, GroundTile);
            }
        }
    }





    void DrawMapWithRoomEdges(int[,] Map)
    {
        // Define colors for room edges
        Color[] colors = new Color[]
        {
            Color.red, Color.green, Color.blue, new Color(1f, 0.5f, 0f), Color.cyan, Color.yellow
        };

        TileMap.ClearAllTiles();

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Vector3Int tilePos = new Vector3Int(x * TileSpacing, y * TileSpacing, 0);

                // Floor first
                if (Map[x, y] == EMPTY)
                {
                    TileMap.SetTile(tilePos, GroundTile);
                }

                // Wall logic only if SOLID and adjacent to EMPTY
                else if (Map[x, y] == SOLID)
                {
                    bool isEdge = false;

                    // Check 4 cardinal directions only
                    int[,] dirs = new int[,]
                    {
                        { 0, 1 },
                        { 0, -1 },
                        { 1, 0 },
                        { -1, 0 }
                    };

                    for (int d = 0; d < 4; d++)
                    {
                        int nx = x + dirs[d, 0];
                        int ny = y + dirs[d, 1];

                        // Bounds check
                        if (nx >= 0 && nx < Width && ny >= 0 && ny < Height)
                        {
                            if (Map[nx, ny] == EMPTY)
                            {
                                isEdge = true;
                                break;
                            }
                        }
                    }

                    if (isEdge)
                    {
                        TileMap.SetTile(tilePos, Wall);
                    }
                }
            }
        }

        // Optional: Draw colored room edges
        for (int s = 0; s < Rooms.Count; s++)
        {
            var room = Rooms[s];
            Color edgeColor = colors[s % colors.Length];
            foreach (var pos in room.Edges)
            {
                Vector3Int tilePos = new Vector3Int(pos[0] * TileSpacing, pos[1] * TileSpacing, 0);
                TileMap.SetColor(tilePos, edgeColor);
            }
        }
    }



    void ConnectTerrain()
    {
        //Generates a new Map and connects it with the already Existing one.

        int[,] newMap = new int[Width, Height];


    }



    void PrintMapToConsole(int[,] Map)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int y = Height - 1; y >= 0; y--) // Print top to bottom
        {
            for (int x = 0; x < Width; x++)
            {
                switch (Map[x, y])
                {
                    case 0: sb.Append('.'); break; // EMPTY (floor)
                    case 1: sb.Append('#'); break; // WALL
                    case 2: sb.Append('X'); break; // SOLID
                    default: sb.Append('?'); break;
                }
            }
            sb.AppendLine();
        }

        Debug.Log(sb.ToString());
    }



    void DrawPillars(Vector3 TilePos)
    {


    }


    int[] MapSafe(int i, int j)
    {
        int xto = Mathf.Clamp(i, 0, Width - 1);
        int yto = Mathf.Clamp(j, 0, Height - 1);
        return new int[] { xto, yto };
    }

    int GridVal(int[,] grid, int[] point)
    {
        return grid[point[0], point[1]];
    }

    void RandomFill(int[,] Map)
    {
        for (int j = 0; j < Height; j++)
        {
            for (int i = 0; i < Width; i++)
            {
                if (j == 0 || i == 0 || j == Height - 1 || i == Width - 1)
                {
                    Map[i, j] = SOLID;
                }
                else
                {
                    Map[i, j] = (UnityEngine.Random.value <= fill) ? SOLID : EMPTY;
                }
            }
        }
    }

    int SmoothIndex(int x, int y, int[,] Map)
    {
        int neighbors = 0;
        int[,] list = new int[,]
        {
            { y - 1, x - 1 }, { y + 1, x - 1 }, { y, x - 1 },
            { y - 1, x },     { y + 1, x },
            { y - 1, x + 1 }, { y + 1, x + 1 }, { y, x + 1 }
        };
        int mapValue = Map[x, y];
        for (int f = 0; f < list.GetLength(0); f++)
        {
            int[] pos = MapSafe(list[f, 1], list[f, 0]);
            neighbors += Map[pos[0], pos[1]];
            if (neighbors > 4)
                break;
        }
        if (neighbors < 4)
            return 0;
        else if (neighbors > 4)
            return 1;
        else
            return mapValue;
    }

    void SmoothAll(int[,] Map)
    {
        int Sep = 3;
        int Len = smooth * Sep;
        int Start = 1;
        int Stopj = Height - 1;
        int Stopi = Width - 1;

        for (int j = Start; j < Stopj; j++)
        {
            for (int i = Start; i < Stopi; i++)
            {
                for (int n = 0; n < Len; n += Sep)
                {
                    int sj = (j + n);
                    if (sj >= Stopj)
                    {
                        sj = sj - Stopj + Start;
                        i++;
                        if (i >= Stopi)
                        {
                            i = i - Stopi + Start;
                        }
                    }
                    Map[i, sj] = SmoothIndex(i, sj, Map);
                }
            }
        }
    }

    List<float[]> FindRoom(int[,] checkGrid, int value, int[] pos, int[,] Map)
    {
        List<float[]> Saved = new List<float[]>();
        Stack<int[]> Check = new Stack<int[]>();

        int x = pos[0];
        int y = pos[1];

        if (Map[x, y] == value && checkGrid[x, y] == 0)
        {
            checkGrid[x, y] = 1;
            Check.Push(new int[] { x, y });
        }
        else
        {
            return Saved;
        }

        int[,] Directions = new int[,]
        {
            {1, 0}, {-1, 0}, {0, 1}, {0, -1}
        };
        int[] EdgeMasks = new int[] { 1, 2, 4, 8 };

        while (Check.Count > 0)
        {
            int[] NewPos = Check.Pop();
            int EdgeMask = 0;

            int i = NewPos[0];
            int j = NewPos[1];

            float[] savedEntry = new float[3] { i, j, 0 };
            Saved.Add(savedEntry);

            for (int d = 0; d < 4; d++)
            {
                int[] safe = MapSafe(i + Directions[d, 0], j + Directions[d, 1]);
                int nx = safe[0];
                int ny = safe[1];

                if (Map[nx, ny] == value)
                {
                    if (checkGrid[nx, ny] == 0)
                    {
                        checkGrid[nx, ny] = 1;
                        Check.Push(new int[] { nx, ny });
                    }
                }
                else
                {
                    EdgeMask |= EdgeMasks[d];
                }
            }
            savedEntry[2] = EdgeMask > 0 ? 1 : 0;
        }
        return Saved;
    }

    void SetupRooms(int[,] Map)
    {
        Regions = new List<List<float[]>>();
        Rooms = new List<Room>();

        int[,] checkGrid = new int[Width, Height];

        List<int[]> positions = new List<int[]>();
        for (int j = 0; j < Height; j++)
        {
            for (int i = 0; i < Width; i++)
            {
                if (Map[i, j] == 0)
                {
                    positions.Add(new int[] { i, j });
                }
            }
        }

        while (positions.Count > 0)
        {
            int[] pos = positions[positions.Count - 1];
            positions.RemoveAt(positions.Count - 1);

            var temp = FindRoom(checkGrid, 0, pos, Map);
            int size = temp.Count;

            if (size > 0)
            {
                if (size >= 1)
                {
                    Regions.Add(temp);

                    List<int[]> edges = new List<int[]>();
                    for (int i = 0; i < size; i++)
                    {
                        if (temp[i][2] > 0)
                        {
                            edges.Add(new int[] { (int)temp[i][0], (int)temp[i][1] });
                        }
                    }

                    Room room = new Room(temp, edges);
                    Rooms.Add(room);
                }
                else
                {
                    foreach (var entry in temp)
                    {
                        Map[(int)entry[0], (int)entry[1]] = 1;
                    }
                }
            }
        }

        if (Rooms.Count <= 0)
        {
            Debug.Log("out of rooms");
            return;
        }

        Rooms.Sort((a, b) => b.Tiles.Count - a.Tiles.Count);
    }

    public void FillArray(int xcell, int ycell, int radius, int value, int[,] Map)
    {
        int radiusSquared = radius * radius;
        int minX = Mathf.Max(0, xcell - radius);
        int maxX = Mathf.Min(Width - 1, xcell + radius);
        int minY = Mathf.Max(0, ycell - radius);
        int maxY = Mathf.Min(Height - 1, ycell + radius);
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                int dx = x - xcell;
                int dy = y - ycell;
                int distanceSquared = dx * dx + dy * dy;
                if (distanceSquared <= radiusSquared)
                {
                    Map[x, y] = value;
                }
            }
        }
    }

    public void ConnectRooms()
    {
        var roomsLeft = new List<Room>(Rooms);
        if (roomsLeft.Count == 0) return;
        roomsLeft.RemoveAt(0);
        var blob = new List<int[]>(Rooms[0].Edges);
        int count = roomsLeft.Count;

        while (count > 0)
        {
            float minDist = float.MaxValue;
            int rmIndex = -1;
            int[] p1 = null;
            int[] p2 = null;

            for (int e = 0; e < blob.Count; e++)
            {
                var edgeA = blob[e];
                for (int rm = 0; rm < roomsLeft.Count; rm++)
                {
                    var roomB = roomsLeft[rm];
                    for (int b = 0; b < roomB.Edges.Count; b++)
                    {
                        var edgeB = roomB.Edges[b];
                        float dist = Vector2.Distance(new Vector2(edgeA[0], edgeA[1]), new Vector2(edgeB[0], edgeB[1]));
                        if (minDist > dist)
                        {
                            minDist = dist;
                            rmIndex = rm;
                            p1 = edgeA;
                            p2 = edgeB;
                        }
                    }
                }
            }
            if (roomsLeft.Count > 0 && rmIndex >= 0)
            {
                MakeBridge(p1, p2, Map);
                blob.AddRange(roomsLeft[rmIndex].Edges);
                roomsLeft.RemoveAt(rmIndex);
                count = roomsLeft.Count;
            }
            else
            {
                break;
            }
        }
    }

    public void MakeBridge(int[] pntA, int[] pntB, int[,] Map)
    {
        float len = Vector2.Distance(new Vector2(pntA[0], pntA[1]), new Vector2(pntB[0], pntB[1]));
        for (int i = 0; i <= len; i++)
        {
            float ratio = i / len;
            float x1 = Mathf.Lerp(pntA[0], pntB[0], ratio);
            float y1 = Mathf.Lerp(pntA[1], pntB[1], ratio);
            int r1 = Mathf.Abs((int)(2 * Mathf.Cos(ratio * Mathf.PI))) + 1;
            int xi = Mathf.FloorToInt(Mathf.Clamp(x1, r1 + 1, Width - 2 - r1));
            int yi = Mathf.FloorToInt(Mathf.Clamp(y1, r1 + 1, Height - 2 - r1));
            FillArray(xi, yi, r1, 0, Map);
        }
    }

    private void SpawnPlayerOnPlacedTile()
    {
        List<Vector3Int> placedTiles = new List<Vector3Int>();

        BoundsInt bounds = TileMap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (TileMap.HasTile(pos))
            {
                placedTiles.Add(pos);
            }
        }

        if (placedTiles.Count > 0)
        {
            Vector3Int spawnCell = placedTiles[UnityEngine.Random.Range(0, placedTiles.Count)];
            Vector3 worldPos = TileMap.CellToWorld(spawnCell) + TileMap.layoutGrid.cellSize / 2f;

            Player.transform.position = worldPos;

            Debug.Log($"Spawned player at tile: {spawnCell}, world position: {worldPos}");
        }
        else
        {
            Debug.LogWarning("No placed tiles found on the Tilemap!");
        }
    }



    private void SpawnEnemies()
    {

    }

    List<Tile> GetTiles(string Path)
    {
        var GroundTiles = new List<Tile>();

        if (Directory.Exists(Path))
        {
            string[] Files = Directory.GetFiles(Path, "*.asset", SearchOption.TopDirectoryOnly);
            foreach (string file in Files)
            {
#if UNITY_EDITOR
                var Tile = UnityEditor.AssetDatabase.LoadAssetAtPath<Tile>(file.Replace(Application.dataPath, "Assets"));
                if (Tile != null)
                {
                    GroundTiles.Add(Tile);
                }
#endif
            }
        }
        else
        {
            Debug.Log("Folder Not Found: " + Path);
        }

        return GroundTiles;
    }
}


public class Room
{
    public List<float[]> Tiles;
    public int Size;
    public List<int[]> Edges;

    public Room(List<float[]> tiles, List<int[]> edges)
    {
        Tiles = tiles;
        Edges = edges;
        Size = tiles.Count;
    }
}
