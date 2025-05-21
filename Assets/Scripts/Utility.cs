using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public enum Directions
{
    Up,
    Right,
    Down,
    Left,
    End
}

public enum TileType {
    Null,
    Void,
    Terrain,
    Wall,
    Ice,
    Gate,
    Lamp,
    Decoration
}

public enum RoomType
{
    Room,
    Corridor
}

[System.Serializable]
public struct Coord
{
    public int x;
    public int y;

    public Coord(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public static bool operator ==(Coord a, Coord b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Coord a, Coord b)
    {
        return !(a == b);
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Coord)) return false;
        Coord other = (Coord)obj;
        return this == other;
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ y.GetHashCode();
    }
}

[System.Serializable]
public class DirectionData
{
    public Coord direction;
    public Vector3 rotation;

    public DirectionData(Coord dir, Vector3 rot)
    {
        this.direction = dir;
        this.rotation = rot;
    }
}

[System.Serializable]
public class ProceduralRoom
{
    public Coord coordinates;
    public int width;
    public int height;
    public RoomType roomType;

    public ProceduralRoom(Coord coordinates, int width, int height, RoomType roomType)
    {
        this.coordinates = coordinates;
        this.width = width;
        this.height = height;
        this.roomType = roomType;
    }
}

[System.Serializable]
public class TileTypeEntry
{
    public TileType type;
    public GameObject prefab;
}

[System.Serializable]
public class NeighbourTile
{
    public Directions direction;
    public GameObject tile;
    private TileData neighbour;

    public NeighbourTile(Directions direction, TileData neighbour)
    {
        this.direction = direction;
        this.neighbour = neighbour;
    }
}

public class TileData
{
    public TileType type;
    public List<DirectionData> directions = new List<DirectionData>();
    public List<NeighbourTile> neighbours = new List<NeighbourTile>();
    public Coord position;
    public bool startTile = false;
    public bool reserved = false;

    public TileData()
    {
    }

    public TileData(Coord position, TileType type)
    {
        this.position = position;
        this.type = type;
    }

    public bool CanStart()
    {
        return type == TileType.Terrain;
    }
}