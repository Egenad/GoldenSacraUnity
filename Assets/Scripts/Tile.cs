using System;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public TileType type;
    public Coord position;
    public List<NeighbourTile> neighbours = new List<NeighbourTile>();

    // Flags
    public bool canStepUp = false;
    public bool canStart = false;
    public bool reserved = false;

    // Minimap
    public bool seen = false;
    public bool startTile = false;

    [Header("Minimap")]
    public MinimapTileRepresentation minimapRepresentation;
    public GameObject minimapRepPrefab;

    [Header("Start Marker")]
    public GameObject startSphere;

    // References
    public TileMap tilemap;


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SeeTile()
    {
        if (!seen && minimapRepresentation != null)
        {
            seen = true;
            minimapRepresentation.TileHasBeenSeen();
        }

        // Check neighbours
        foreach (Directions direction in Enum.GetValues(typeof(Directions)))
        {
            /*if (neighbours.TryGetValue(direction, out Tile tileNeighbour) && tileNeighbour != null)
            {
                if (!tileNeighbour.seen && tileNeighbour.minimapRepresentation != null)
                {
                    tileNeighbour.seen = true;
                    tileNeighbour.minimapRepresentation.TileHasBeenSeen();
                }
            }*/
        }
    }

    public MinimapTileRepresentation CreateMinimapRepresentation()
    {
        Vector3 locationToSpawn = transform.position;
        locationToSpawn.z += tilemap.minimapZOffset;

        GameObject instance = Instantiate(minimapRepPrefab, locationToSpawn, Quaternion.identity);
        minimapRepresentation = instance.GetComponent<MinimapTileRepresentation>();

        return minimapRepresentation;
    }

    /*public void StartGameHere()
    {
        if (canStart)
        {
            startTile = true;
            if (startSphere != null)
                startSphere.SetActive(true);

            foreach (TileData t in tilemap.tiles)
            {
                if (t != null && t.startTile && t != this)
                {
                    t.startTile = false;

                    //if (t.startSphere != null)
                    //    t.startSphere.SetActive(false);
                }
            }
        }
    }
    */

    /*public void ChangeTileTypeByType()
    {
        //Spawn a new tile in my exact location of desired new type
        GameObject newType = tilemap.GetTilePrefabByType(type);

        if (newType != null)
        {
            ChangeTile(newType);
        }
    }

    [ContextMenu("Change Tile Type")]
    private void ChangeTileType()
    {
        GameObject newType = tilemap.GetTilePrefabByType(type);

        if (newType != null)
        {
            ChangeTile(newType);
        }
    }

    public void ChangeTile(GameObject newTypePrefab)
    {
        Vector3 locationToSpawn = transform.position;

        GameObject newTileGO = Instantiate(newTypePrefab, locationToSpawn, Quaternion.identity, transform.parent);
        Tile newTile = newTileGO.GetComponent<Tile>();

        if (newTile != null)
        {
            newTile.tilemap = tilemap;
            newTile.neighbours = new List<NeighbourTile>(neighbours);

            // Set this tile to the neighbours

            for (int i = 0; i < (int)Directions.End; i++)
            {
                Directions direction = (Directions)i;
                NeighbourTile neighbourEntry = neighbours.Find(n => n.direction == direction);

                if (neighbourEntry != null && neighbourEntry.tile != null)
                {
                    Tile tileNeighbour = neighbourEntry.tile.GetComponent<Tile>();

                    if (tileNeighbour != null)
                    {

                        int inverse = (i + 2) % (int)Directions.End;
                        Directions inverseDirection = (Directions)inverse;

                        NeighbourTile inverseEntry = tileNeighbour.neighbours.Find(n => n.direction == inverseDirection);

                        if (inverseEntry != null)
                        {
                            inverseEntry.tile = newTile.gameObject;
                        }
                        else
                        {
                            tileNeighbour.neighbours.Add(new NeighbourTile(inverseDirection, newTile));
                        }
                    }
                }
            }
            // Delete neighbours references
            neighbours.Clear();

            newTile.position = position;
            newTile.type = type;

            if (minimapRepresentation != null)
            {
                Destroy(minimapRepresentation);
            }

            //newTile.minimapRepresentation = newTile.CreateMinimapRepresentation();

            // Replace in tilemap
            int index = tilemap.GetTileByCoordinates(position);
            if (index != -1 && tilemap.tiles.Count > 0)
            {
                tilemap.tiles[index] = newTile;
            }

            //Destroy(gameObject);
        }
    }*/
}
