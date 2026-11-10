using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sylvan Rush - Procedural Forest Environment (Clean 3D Edition, No Floating Geometry Artifacts)
/// Spawns authentic 3D trees, rocks, bushes, and flowers from SimpleNaturePack.
/// </summary>
public class ProceduralForestEnvironment : MonoBehaviour
{
    [Header("Path Settings")]
    public float pathLength = 160f;
    public float pathWidth = 9.6f;

    private List<GameObject> pathTiles = new List<GameObject>();
    private List<GameObject> forestProps = new List<GameObject>();

    private GameObject[] treePrefabs;
    private GameObject[] bushPrefabs;
    private GameObject[] rockPrefabs;
    private GameObject[] mushroomPrefabs;
    private GameObject[] flowerPrefabs;

    void Awake()
    {
        LoadNatureAssets();
    }

    void Start()
    {
        CreateGroundPath();
        CreateSidelineForestProps();
    }

    void LoadNatureAssets()
    {
        treePrefabs = new GameObject[] {
            Resources.Load<GameObject>("Nature/Tree_01"),
            Resources.Load<GameObject>("Nature/Tree_02"),
            Resources.Load<GameObject>("Nature/Tree_03"),
            Resources.Load<GameObject>("Nature/Tree_04"),
            Resources.Load<GameObject>("Nature/Tree_05")
        };

        bushPrefabs = new GameObject[] {
            Resources.Load<GameObject>("Nature/Bush_01"),
            Resources.Load<GameObject>("Nature/Bush_02"),
            Resources.Load<GameObject>("Nature/Bush_03")
        };

        rockPrefabs = new GameObject[] {
            Resources.Load<GameObject>("Nature/Rock_01"),
            Resources.Load<GameObject>("Nature/Rock_02"),
            Resources.Load<GameObject>("Nature/Rock_03")
        };

        mushroomPrefabs = new GameObject[] {
            Resources.Load<GameObject>("Nature/Mushroom_01"),
            Resources.Load<GameObject>("Nature/Mushroom_02")
        };

        flowerPrefabs = new GameObject[] {
            Resources.Load<GameObject>("Nature/Flowers_01"),
            Resources.Load<GameObject>("Nature/Flowers_02")
        };
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.currentState != GameState.Playing)
            return;

        float speed = GameManager.Instance.currentSpeed;
        float distDelta = speed * Time.deltaTime;

        // Scroll ground tiles along +Z
        foreach (var tile in pathTiles)
        {
            tile.transform.position += Vector3.forward * distDelta;
            if (tile.transform.position.z > 25f)
            {
                tile.transform.position -= Vector3.forward * (pathTiles.Count * 40f);
            }
        }

        // Scroll sideline trees, rocks, and flora along +Z
        foreach (var prop in forestProps)
        {
            prop.transform.position += Vector3.forward * distDelta;
            if (prop.transform.position.z > 25f)
            {
                prop.transform.position -= Vector3.forward * 160f;
            }
        }
    }

    void CreateGroundPath()
    {
        Material pathMat = SylvanMaterialHelper.CreateMaterial(
            new Color(0.04f, 0.12f, 0.08f),
            new Color(0.01f, 0.04f, 0.03f),
            0.2f
        );

        Material stonePathMat = SylvanMaterialHelper.CreateMaterial(
            new Color(0.14f, 0.17f, 0.18f),
            new Color(0.02f, 0.03f, 0.04f),
            0.4f
        );

        Material laneBorderMat = SylvanMaterialHelper.CreateMaterial(
            new Color(0f, 0.96f, 0.83f),
            new Color(0f, 0.96f, 0.83f) * 2.8f
        );

        for (int i = 0; i < 4; i++)
        {
            GameObject tileRoot = new GameObject("PathTile_" + i);
            tileRoot.transform.parent = transform;
            tileRoot.transform.position = new Vector3(0f, -0.01f, -i * 40f);

            // Wide Forest Loam Floor
            GameObject mossFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mossFloor.name = "MossFloor";
            mossFloor.transform.parent = tileRoot.transform;
            mossFloor.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            mossFloor.transform.localScale = new Vector3(32f, 1f, 40f);
            mossFloor.GetComponent<Renderer>().material = pathMat;
            Destroy(mossFloor.GetComponent<Collider>());

            // Center Stone Runner Track
            GameObject stoneTrack = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stoneTrack.name = "StoneTrack";
            stoneTrack.transform.parent = tileRoot.transform;
            stoneTrack.transform.localPosition = new Vector3(0f, -0.48f, 0f);
            stoneTrack.transform.localScale = new Vector3(pathWidth, 1f, 40f);
            stoneTrack.GetComponent<Renderer>().material = stonePathMat;
            Destroy(stoneTrack.GetComponent<Collider>());

            // Glowing Boundary Runes
            CreateBoundaryLine(tileRoot.transform, -pathWidth * 0.49f, laneBorderMat);
            CreateBoundaryLine(tileRoot.transform, pathWidth * 0.49f, laneBorderMat);

            pathTiles.Add(tileRoot);
        }
    }

    void CreateBoundaryLine(Transform parent, float x, Material mat)
    {
        GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        strip.transform.parent = parent;
        strip.transform.localPosition = new Vector3(x, 0.025f, 0f);
        strip.transform.localScale = new Vector3(0.2f, 0.04f, 40f);
        strip.GetComponent<Renderer>().material = mat;
        Destroy(strip.GetComponent<Collider>());
    }

    void CreateSidelineForestProps()
    {
        for (int i = 0; i < 32; i++)
        {
            float z = -i * 5.0f;

            for (int side = -1; side <= 1; side += 2)
            {
                float x = side * (pathWidth * 0.5f + Random.Range(1.8f, 7.5f));

                GameObject cluster = new GameObject("ForestCluster_" + i + "_" + side);
                cluster.transform.parent = transform;
                cluster.transform.position = new Vector3(x, 0f, z);

                // 1. Spawn Real 3D Tree
                if (treePrefabs != null && treePrefabs.Length > 0)
                {
                    GameObject tPrefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                    if (tPrefab != null)
                    {
                        GameObject tree = Instantiate(tPrefab, cluster.transform);
                        float scale = Random.Range(1.1f, 1.8f);
                        tree.transform.localScale = Vector3.one * scale;
                        tree.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    }
                }

                // 2. Spawn Real 3D Rocks or Stumps
                if (Random.value < 0.6f && rockPrefabs != null && rockPrefabs.Length > 0)
                {
                    GameObject rPrefab = rockPrefabs[Random.Range(0, rockPrefabs.Length)];
                    if (rPrefab != null)
                    {
                        GameObject rock = Instantiate(rPrefab, cluster.transform);
                        rock.transform.localPosition = new Vector3(side * Random.Range(-1.5f, 1.5f), 0f, Random.Range(-2f, 2f));
                        rock.transform.localScale = Vector3.one * Random.Range(0.8f, 1.6f);
                        rock.transform.localRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    }
                }

                // 3. Spawn Real 3D Bushes or Flowers
                if (Random.value < 0.7f && bushPrefabs != null && bushPrefabs.Length > 0)
                {
                    GameObject bPrefab = bushPrefabs[Random.Range(0, bushPrefabs.Length)];
                    if (bPrefab != null)
                    {
                        GameObject bush = Instantiate(bPrefab, cluster.transform);
                        bush.transform.localPosition = new Vector3(side * Random.Range(-1.2f, 1.2f), 0f, Random.Range(-1.5f, 1.5f));
                        bush.transform.localScale = Vector3.one * Random.Range(0.9f, 1.5f);
                    }
                }

                forestProps.Add(cluster);
            }
        }
    }
}
