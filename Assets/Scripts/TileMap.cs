using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using NUnit.Framework.Interfaces;
using UnityEngine;

public class TileMap : MonoBehaviour
{

    public List<Tile> tilesGOs = new List<Tile>();
    public List<TileData> tiles = new List<TileData>();
    public List<TileTypeEntry> tileTypes = new List<TileTypeEntry>();
    private Dictionary<TileType, GameObject> typeMap;
    public Dictionary<Directions, DirectionData> directionMap = new();

    public float minimapZOffset = -1500f;

    public int defaultWidth = 5;
    public int defaultHeight = 5;
    public int dungeonWidth = 30;
    public int dungeonHeight = 30;
    public int maxFeatures = 30;
    public int roomChance = 50;
    public int gateChance = 50;

    public List<ProceduralRoom> rooms = new List<ProceduralRoom>();
    public List<ProceduralRoom> exits = new List<ProceduralRoom>();

    public Coord playerStartCoord = new Coord(0, 0);
    public Vector3 playerStartRotation = Vector3.zero;
    public Player player;
    public DirectionData playerStart = new DirectionData(new Coord(0, 0), Vector3.zero);

    private int width = 5;
    private int height = 5;
    private int tileSize = 13;

    private int proceduralWidth = 10;//30;
    private int proceduralHeight = 10;//30;
    public bool proceduralCreation = false;
    int maxProceduralFeatures = 30;

    const int maxRoomSize = 6;
    const int minRoomSize = 3;
    const int maxCorridorSize = 6;
    const int minCorridorSize = 3;


    void Awake()
    {
        InitDirectionMap();
        InitTypeDictionary();
    }

    void Start()
    {
        GenerateProceduralTilemap();
        TeleportPlayerToStart();
        InitPlayerRotation(playerStart.rotation);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void InitTypeDictionary()
    {
        typeMap = new Dictionary<TileType, GameObject>();

        foreach (TileTypeEntry entry in tileTypes)
        {
            if (!typeMap.ContainsKey(entry.type))
            {
                typeMap.Add(entry.type, entry.prefab);
            }
        }
    }

    void InitDirectionMap()
    {
        DirectionData data;

        data = new DirectionData(new Coord(0, -1), new Vector3(0f, 0f, -90f));
        directionMap.Add(Directions.Up, data);

        data = new DirectionData(new Coord(0, 1), new Vector3(0f, 0f, 90f));
        directionMap.Add(Directions.Down, data);

        data = new DirectionData(new Coord(-1, 0), new Vector3(0f, 0f, 180f));
        directionMap.Add(Directions.Left, data);

        data = new DirectionData(new Coord(1, 0), new Vector3(0f, 0f, 0f));
        directionMap.Add(Directions.Right, data);
    }

    void TeleportPlayerToStart()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
        }

        if (player != null)
        {
            bool teleported = false;

            foreach (TileData tile in tiles)
            {
                if (tile != null && tile.startTile)
                {
                    TeleportPlayerToTile(tile.position);
                    teleported = true;
                    break;
                }
            }

            if (!teleported)
            {
                TeleportPlayerToTile(playerStart.direction);
            }
        }
    }

    void TeleportPlayerToTile(Coord coordinates)
    {
        if (player != null)
        {
            TileData targetTile = null;
            bool validTile = false;

            int index = GetTileByCoordinates(coordinates);

            if (index != -1)
            {
                targetTile = tiles[index];
                if (targetTile.CanStart())
                {
                    validTile = true;
                }
            }

            if (!validTile)
            {
                targetTile = tiles.FirstOrDefault(t => t.CanStart() && t.position == coordinates);
                if (targetTile == null)
                {
                    targetTile = tiles.FirstOrDefault(t => t.CanStart());
                }
            }

            if (targetTile != null)
            {
                targetTile.reserved = true;
                player.currentTile = targetTile;

                Vector3 position = new Vector3(targetTile.position.x * tileSize, 0f, targetTile.position.y * tileSize);

                player.transform.position = position;
                player.actualLocation = position;
            }
        }
    }

    void InitPlayerRotation(Vector3 rotation)
    {
        if (player != null)
        {
            bool success = false;
            Directions newDirection = GetDirectionByRotation(rotation, out success);
            Vector3 newRotation = new Vector3(0f, 0f, -90f);

            if (success)
            {
                newRotation = rotation;
            }

            player.transform.rotation = Quaternion.Euler(newRotation);
            player.focusedTile = newDirection;
        }
    }

    Directions GetDirectionByRotation(Vector3 rotation, out bool success)
    {
        const float epsilon = 0.01f;
        float r_z = rotation.y;

        foreach (var kvp in directionMap)
        {
            if (Mathf.Abs(kvp.Value.rotation.y - r_z) < epsilon)
            {
                success = true;
                return kvp.Key;
            }
        }

        success = false;
        return Directions.Up;
    }

    public int GetTileByCoordinates(Coord coordinates)
    {
        int index = ((width * coordinates.y) + coordinates.x);

        if (index >= 0 && index < (width * height))
        {
            return index;
        }

        return -1;
    }

    void GenerateProceduralTilemap()
    {
        proceduralWidth = dungeonWidth;
        proceduralHeight = dungeonHeight;

        if (proceduralWidth < 30) proceduralWidth = 30;
        if (proceduralHeight < 30) proceduralHeight = 30;

        DeleteTileMap();

        if (typeMap != null && typeMap.Count > 0)
        {
            proceduralCreation = true;
            GenerateTilemap();

            if (GenerateProceduralRoom(proceduralWidth / 2, proceduralHeight / 2, (Directions)Random.Range(0, 4), true)) {
                for (int i = 0; i < maxProceduralFeatures; i++)
                {
                    if (!GenerateProceduralFeature())
                    {
                        break;
                    }
                }
            }
        }

        foreach (TileData tile in tiles)
        {

            GameObject newTypePrefab = GetTilePrefabByType(tile.type);
            Vector3 locationToSpawn = new Vector3(tile.position.x * tileSize, 0f, tile.position.y * tileSize);
            
            GameObject newTileGO = Instantiate(newTypePrefab, locationToSpawn, Quaternion.identity, transform.parent);
            tilesGOs.Add(newTileGO.GetComponent<Tile>()); 
            
        }

        proceduralCreation = false;
    }

    void GenerateTilemap()
    {
        if (typeMap != null && typeMap.Count > 0)
        {
            if (!proceduralCreation)
            {
                width = defaultWidth;
                height = defaultHeight;
            }
            else
            {
                width = proceduralWidth;
                height = proceduralHeight;
            }

            tiles.Clear();
            Coord tilePosition = new Coord(0, 0);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    TileType tileType = TileType.Terrain;

                    if (tilePosition.x == 0 || j == (width - 1) || tilePosition.y == 0 || i == (height - 1) || proceduralCreation)
                    {
                        tileType = TileType.Wall;
                    }

                    TileData newTile = new TileData(new Coord(j, i), tileType);
                    tiles.Add(newTile);

                    tilePosition.x += tileSize;
                }

                tilePosition.x = 0;
                tilePosition.y += tileSize;
            }

            foreach (var tile in tiles)
            {
                SetTileNeighbours(tile);
            }
        }
    }

    bool GenerateProceduralRoom(int x, int y, Directions d, bool init)
    {
        int roomWidth = Random.Range(minRoomSize, maxRoomSize + 1);
        int roomHeight = Random.Range(minRoomSize, maxRoomSize + 1);

        int xRoom = x;
        int yRoom = y;

        switch (d)
        {
            case Directions.Down: // SOUTH
                xRoom = x - roomWidth / 2;
                yRoom = y + 1;
                break;
            case Directions.Up: // NORTH
                xRoom = x - roomWidth / 2;
                yRoom = y - roomHeight;
                break;
            case Directions.Left: // WEST
                xRoom = x - roomWidth;
                yRoom = y - roomHeight / 2;
                break;
            case Directions.Right: // EAST
                xRoom = x + 1;
                yRoom = y - roomHeight / 2;
                break;
        }

        if (CreateRoom(xRoom, yRoom, roomWidth, roomHeight, TileType.Terrain))
        {
            // Save the room itself
            ProceduralRoom firstRoom = new ProceduralRoom(new Coord(xRoom, yRoom), roomWidth, roomHeight, RoomType.Room);
            rooms.Add(firstRoom);

            // Save possible exits of the room

            if (d != Directions.Down || init)
                exits.Add(new ProceduralRoom (new Coord(xRoom, yRoom - 1), roomWidth, 1, RoomType.Room ));

            if (d != Directions.Up || init)
                exits.Add(new ProceduralRoom (new Coord(xRoom, yRoom + roomHeight), roomWidth, 1, RoomType.Room ));

            if (d != Directions.Right || init)
                exits.Add(new ProceduralRoom (new Coord(xRoom - 1, yRoom), 1, roomHeight, RoomType.Room ));

            if (d != Directions.Left || init)
                exits.Add(new ProceduralRoom (new Coord(xRoom + roomWidth, yRoom), 1, roomHeight, RoomType.Room ));

            return true;
        }

        return false;
    }

    bool CreateRoom(int x, int y, int newWidth, int newHeight, TileType tileType)
    {
        if (x < 1 || y < 1 || x + newWidth > proceduralWidth - 1 || y + newHeight > proceduralHeight - 1)
            return false;

        for (int i = x - 1; i < x + newWidth + 1; i++)
        {
            for (int j = y - 1; j < y + newHeight + 1; j++)
            {
                int index = GetTileByCoordinates(new Coord(i, j));

                if (index == -1 || tiles[index] == null || tiles[index].type != TileType.Wall)
                    return false;
            }
        }

        //  Create the room
        for (int i = x - 1; i < x + newWidth + 1; i++)
        {
            for (int j = y - 1; j < y + newHeight + 1; j++)
            {
                int index = GetTileByCoordinates(new Coord(i, j));
                if (index != -1 && tiles[index] != null)
                {
                    TileData tile = tiles[index];
                    if (i != x - 1 && j != y - 1 && i != x + newWidth && j != y + newHeight)
                    {
                        tile.type = tileType;
                    }

                    //tile.ChangeTileTypeByType();
                    //Destroy(tile);
                }
            }
        }

        return true;
    }

    public GameObject GetTilePrefabByType(TileType tileType)
    {
        if (typeMap != null && typeMap.TryGetValue(tileType, out GameObject prefab))
        {
            return prefab;
        }

        return null;
    }

    GameObject GenerateTileType(Coord coordinates, Vector3 location, Vector3 rotation, Vector3 scale, TileType tileType)
    {
        if (typeMap != null && typeMap.Count > 0 && typeMap.ContainsKey(tileType))
        {
            GameObject tilePrefab = typeMap[tileType];
            //GameObject newTile = Instantiate(tilePrefab, location, Quaternion.Euler(rotation));
            //newTile.transform.localScale = scale;

            /*Tile tileComponent = newTile.GetComponent<Tile>();

            if (tileComponent != null)
            {
                tileComponent.position = coordinates;
                tileComponent.type = tileType;
                tileComponent.tilemap = this;
                //tileComponent.CreateMinimapRepresentation();

                return tileComponent;
            }*/
            return tilePrefab;
        }

        return null;
    }

    bool GenerateProceduralFeature()
    {
        // Feature means a new room or a new corridor

        bool result = false;

        for (int i = 0; i < 1000; i++)
        {
            if (exits.Count == 0)
            {
                break;
            }

            int randomRoomIndex = Random.Range(0, exits.Count - 1);
            int x = Random.Range(
                exits[randomRoomIndex].coordinates.x,
                exits[randomRoomIndex].coordinates.x + exits[randomRoomIndex].width
            );

            int y = Random.Range(
                exits[randomRoomIndex].coordinates.y,
                exits[randomRoomIndex].coordinates.y + exits[randomRoomIndex].height
            );

            for (int j = 0; j < 4; j++)
            {
                if (GenerateFeature(x, y, (Directions)j))
                {
                    // If we could create a feature, erase the exit from the array.
                    exits.RemoveAt(randomRoomIndex);
                    return true;
                }
            }
        }

        return result;
    }

    bool GenerateFeature(int x, int y, Directions d)
    {
        int directionX = 0;
        int directionY = 0;

        switch (d)
        {
            case Directions.Up: // NORTH
                directionY = 1;
                break;
            case Directions.Down: // SOUTH
                directionY = -1;
                break;
            case Directions.Left: // WEST
                directionX = -1;
                break;
            case Directions.Right: // EAST
                directionX = 1;
                break;
        }

        int tileIndex = GetTileByCoordinates(new Coord(x + directionX, y + directionY));

        if (tileIndex != -1 && tileIndex < tiles.Count && tiles[tileIndex] != null && tiles[tileIndex].type == TileType.Terrain)
        {
            if (Random.Range(0, 100) < roomChance)
            {
                // Create a new room
                if (GenerateProceduralRoom(x, y, d, false))
                {
                    int newTileIndex = GetTileByCoordinates(new Coord(x, y));
                    if (newTileIndex != -1 && tiles[newTileIndex] != null)
                    {
                        tiles[newTileIndex].type = TileType.Terrain;
                    }
                    return true;
                }
            }
            else
            {
                // Create a new corridor
                if (GenerateProdecuralCorridor(x, y, d))
                {
                    int newTileIndex = GetTileByCoordinates(new Coord(x, y));
                    if (newTileIndex != -1 && tiles[newTileIndex] != null)
                    {
                        tiles[newTileIndex].type = TileType.Terrain;
                    }
                    return true;
                }
            }
        }

        return false;
    }

    bool GenerateProdecuralCorridor(int x, int y, Directions d)
    {
        ProceduralRoom corridor = new ProceduralRoom(new Coord(x, y), 0, 0, RoomType.Corridor);

        if (Random.value < 0.5f)
        {
            // Horizontal
            corridor.width = Random.Range(minCorridorSize, maxCorridorSize + 1);
            corridor.height = 1;
            corridor.coordinates = new Coord(x, y);

            switch (d)
            {
                case Directions.Up:
                    corridor.coordinates.y = y - 1;
                    if (Random.value < 0.5f)
                    {
                        corridor.coordinates.x = x - corridor.width + 1;
                    }
                    break;

                case Directions.Down:
                    corridor.coordinates.y = y + 1;
                    if (Random.value < 0.5f)
                    {
                        corridor.coordinates.x = x - corridor.width + 1;
                    }
                    break;

                case Directions.Left:
                    corridor.coordinates.x = x - corridor.width;
                    break;

                case Directions.Right:
                    corridor.coordinates.x = x + 1;
                    break;
            }
        }
        else
        {
            // Vertical
            corridor.width = 1;
            corridor.height = Random.Range(minCorridorSize, maxCorridorSize + 1);

            switch (d)
            {
                case Directions.Up:
                    corridor.coordinates.y = y - corridor.height;
                    break;

                case Directions.Down:
                    corridor.coordinates.y = y + 1;
                    break;

                case Directions.Left:
                    corridor.coordinates.x = x - 1;
                    if (Random.value < 0.5f)
                    {
                        corridor.coordinates.y = y - corridor.height + 1;
                    }
                    break;

                case Directions.Right:
                    corridor.coordinates.x = x + 1;
                    if (Random.value < 0.5f)
                    {
                        corridor.coordinates.y = y - corridor.height + 1;
                    }
                    break;
            }

            if(CreateRoom(corridor.coordinates.x, corridor.coordinates.y, corridor.width, corridor.height, TileType.Terrain))
            {
                // Save the room itself
                rooms.Add(corridor);

                // Save possible exits of the room
                if (d != Directions.Down && corridor.height != 1) // North border
                    exits.Add(new ProceduralRoom(new Coord(corridor.coordinates.x, corridor.coordinates.y - 1), corridor.width, 1, RoomType.Corridor));

                if (d != Directions.Up && corridor.height != 1) // South border
                    exits.Add(new ProceduralRoom(new Coord(corridor.coordinates.x, corridor.coordinates.y + corridor.height), corridor.width, 1, RoomType.Corridor));

                if (d != Directions.Left && corridor.height != 1) // East border
                    exits.Add(new ProceduralRoom(new Coord(corridor.coordinates.x + corridor.width, corridor.coordinates.y), 1, corridor.height, RoomType.Corridor));

                if (d != Directions.Right && corridor.height != 1) // West border
                    exits.Add(new ProceduralRoom(new Coord(corridor.coordinates.x - 1, corridor.coordinates.y), 1, corridor.height, RoomType.Corridor));

                return true;
            }
        }
        return false;
    }

    void SetTileNeighbours(TileData tile)
    {
        if (tile != null)
        {

            int cx = tile.position.x;
            int cy = tile.position.y;

            foreach (var kvp in directionMap)
            {
                Directions direction = kvp.Key;

                DirectionData data = kvp.Value;
                int dx = data.direction.x;
                int dy = data.direction.y;

                int aux_x = cx + dx;
                int aux_y = cy + dy;

                if (aux_x >= 0 && aux_y >= 0)
                {
                    int index = GetTileByCoordinates(new Coord(aux_x, aux_y));

                    if (index != -1 && index < tiles.Count)
                    {
                        TileData neighbour = tiles[index];

                        if (neighbour != null)
                        {
                            tile.neighbours.Add(new NeighbourTile(direction, neighbour));
                        }
                    }
                }
            }
        }
    }

    void ResetTileMap()
    {
        DeleteTileMap();
        GenerateProceduralTilemap();
    }

    void DeleteTileMap()
    {
        tiles.Clear();
        /*if (tiles != null && tiles.Count > 0)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i] != null)
                {
                    if (tiles[i].minimapRepresentation != null)
                    {
                        Destroy(tiles[i].minimapRepresentation.gameObject);
                        tiles[i].minimapRepresentation = null;
                    }

                    Destroy(tiles[i].gameObject);
                    tiles[i] = null;
                }
            }
            tiles.Clear();
        }*/
    }

}
