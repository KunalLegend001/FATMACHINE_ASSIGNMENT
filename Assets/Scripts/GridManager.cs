using UnityEngine;

public class GridManager : MonoBehaviour
{
    public GameObject tilePrefab;
    public Transform gridParent;

    public int gridSizeX = 2;
    public int gridSizeZ = 2;

    void Start()
    {
        GenerateGrid();
    }

    public void GenerateGrid()
    {
        if (!tilePrefab)
        {
            Debug.LogWarning("Tile prefab not assigned.");
            return;
        }

        // Get bounds of one tile to determine spacing
        Renderer tileRenderer = tilePrefab.GetComponentInChildren<Renderer>();
        if (!tileRenderer)
        {
            Debug.LogError("Tile prefab has no Renderer to calculate size.");
            return;
        }

        Vector3 tileSize = tileRenderer.bounds.size;

        // Spawn grid of tiles
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                Vector3 position = new Vector3(
                    x * tileSize.x,
                    0f,
                    z * tileSize.z
                );

                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity, gridParent);
                tile.name = $"Tile_{x}_{z}";
            }
        }
    }
   
}
