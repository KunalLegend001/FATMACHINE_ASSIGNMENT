using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class GridVisualizer : MonoBehaviour
{
    private Transform gridParent;
    public float fixedY = 1f;
    public Color gizmoColor = Color.green;
    public float gizmoRadius = 0.1f;

    public List<Vector3> gridPoints = new List<Vector3>();
    private void Start()
    {
        GameObject found = GameObject.Find("GridContainer");
        if (found != null)
        {
            gridParent = found.GetComponent<Transform>();
        }

    }
    void Update()
    {
        GenerateGridPoints();
    }

    void GenerateGridPoints()
    {
        gridPoints.Clear();

        if (!gridParent) return;

        foreach (Transform tile in gridParent)
        {
            Renderer tileRenderer = tile.GetComponentInChildren<Renderer>();
            if (!tileRenderer) continue;

            Bounds bounds = tileRenderer.bounds;
            Vector3 center = bounds.center;
            Vector3 size = bounds.size;

            // Divide tile into 2x2 and calculate centers
            for (int dx = 0; dx < 2; dx++)
            {
                for (int dz = 0; dz < 2; dz++)
                {
                    float offsetX = (-size.x / 4f) + dx * (size.x / 2f);
                    float offsetZ = (-size.z / 4f) + dz * (size.z / 2f);

                    Vector3 point = new Vector3(
                        center.x + offsetX,
                        fixedY,
                        center.z + offsetZ
                    );

                    gridPoints.Add(point);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        foreach (Vector3 point in gridPoints)
        {
            Gizmos.DrawSphere(point, gizmoRadius);
        }
    }
}
