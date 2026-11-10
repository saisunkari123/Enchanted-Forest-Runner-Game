using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sylvan Rush - Procedural Forest Environment
/// Spawns and scrolls the enchanted forest path, sideline trees, and glowing flora.
/// </summary>
public class ProceduralForestEnvironment : MonoBehaviour
{
    [Header("Path Settings")]
    public float pathLength = 120f;
    public float pathWidth = 9.5f;

    private List<GameObject> pathTiles = new List<GameObject>();
    private List<GameObject> forestProps = new List<GameObject>();

    void Start()
    {
        CreateGroundPath();
        CreateSidelineTrees();
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.currentState != GameState.Playing)
            return;

        float speed = GameManager.Instance.currentSpeed;
        float distDelta = speed * Time.deltaTime;

        // Scroll ground tiles
        foreach (var tile in pathTiles)
        {
            tile.transform.position += Vector3.forward * distDelta;
            if (tile.transform.position.z > 25f)
            {
                tile.transform.position -= Vector3.forward * (pathTiles.Count * 40f);
            }
        }

        // Scroll sideline trees & rocks
        foreach (var prop in forestProps)
        {
            prop.transform.position += Vector3.forward * distDelta;
            if (prop.transform.position.z > 20f)
            {
                prop.transform.position -= Vector3.forward * 140f;
            }
        }
    }

    void CreateGroundPath()
    {
        Material pathMat = new Material(Shader.Find("Standard"));
        pathMat.color = new Color(0.04f, 0.08f, 0.12f); // Deep forest floor
        pathMat.SetFloat("_Glossiness", 0.4f);

        for (int i = 0; i < 4; i++)
        {
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane.name = "PathTile_" + i;
            plane.transform.parent = transform;
            plane.transform.localScale = new Vector3(pathWidth / 10f, 1f, 4f); // 40m long
            plane.transform.position = new Vector3(0f, -0.01f, -i * 40f);
            plane.GetComponent<Renderer>().material = pathMat;

            // Remove mesh collider from path tile to prevent physics interference
            Destroy(plane.GetComponent<Collider>());
            pathTiles.Add(plane);
        }
    }

    void CreateSidelineTrees()
    {
        Material trunkMat = new Material(Shader.Find("Standard"));
        trunkMat.color = new Color(0.18f, 0.11f, 0.06f);

        Material foliageMat = new Material(Shader.Find("Standard"));
        foliageMat.color = new Color(0.08f, 0.45f, 0.38f);
        foliageMat.EnableKeyword("_EMISSION");
        foliageMat.SetColor("_EmissionColor", new Color(0f, 0.25f, 0.2f));

        for (int i = 0; i < 30; i++)
        {
            // Left and right trees
            for (int side = -1; side <= 1; side += 2)
            {
                GameObject tree = new GameObject("ForestTree_" + i + "_" + side);
                tree.transform.parent = transform;
                float x = side * (pathWidth * 0.5f + Random.Range(1.8f, 6.5f));
                float z = -i * 4.8f;
                tree.transform.position = new Vector3(x, 0f, z);

                // Trunk
                GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                trunk.transform.parent = tree.transform;
                trunk.transform.localPosition = new Vector3(0f, 3.5f, 0f);
                trunk.transform.localScale = new Vector3(0.9f, 3.5f, 0.9f);
                trunk.GetComponent<Renderer>().material = trunkMat;
                Destroy(trunk.GetComponent<Collider>());

                // Foliage canopy
                GameObject foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                foliage.transform.parent = tree.transform;
                foliage.transform.localPosition = new Vector3(0f, 7.2f, 0f);
                foliage.transform.localScale = new Vector3(3.4f, 3.2f, 3.4f);
                foliage.GetComponent<Renderer>().material = foliageMat;
                Destroy(foliage.GetComponent<Collider>());

                forestProps.Add(tree);
            }
        }
    }
}
