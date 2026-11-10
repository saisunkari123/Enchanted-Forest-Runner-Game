using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObstacleType
{
    Log,            // Woodland Fallen Log / Stump (jumpable)
    Monolith,       // Ancient 3D Jagged Rock Monolith (lane-dodge)
    OverheadBranch  // Low-hanging Enchanted Branch (requires sliding under)
}

/// <summary>
/// Attached to every spawned obstacle.
/// </summary>
public class Obstacle : MonoBehaviour
{
    public ObstacleType obstacleType;
    public float obstacleHeight = 1.2f;
    public int laneIndex;
    public float zPosition;
    public bool isPassed = false;
}

public class ManaOrb : MonoBehaviour
{
    public int laneIndex;
    public float zPosition;
    public bool isMagnetized = false;

    void Update()
    {
        transform.Rotate(Vector3.up * (Time.deltaTime * 180f), Space.World);

        if (isMagnetized && PlayerController.Instance != null)
        {
            Vector3 target = PlayerController.Instance.transform.position + Vector3.up * 1f;
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 14f);
        }
    }
}

/// <summary>
/// Sylvan Rush - Procedural Obstacle & Reward Spawner (Prominent Big Size & 3D Symbols)
/// Uses large 3D nature props (Rock_04 Monolith, large Stump, wide Branch arch)
/// and prominent arcade-scale 3D symbols for Magnet, Shield, and Dash.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    public static ObstacleSpawner Instance { get; private set; }

    [Header("Wave Spacing Settings")]
    public float initialWaveSpacing = 24f;
    public float minWaveSpacing = 15f;

    [Header("Prefabs (Loaded from Resources or Procedural)")]
    public GameObject logPrefab;
    public GameObject monolithPrefab;
    public GameObject overheadPrefab;
    public GameObject orbPrefab;

    [Header("Power-Up 3D Symbol Prefabs")]
    public GameObject magnetPrefab;
    public GameObject shieldPrefab;
    public GameObject dashPrefab;

    private float spawnZCursor = -65f;
    private List<GameObject> activeObstacles = new List<GameObject>();
    private List<GameObject> activeOrbs = new List<GameObject>();
    private List<GameObject> activePowerUps = new List<GameObject>();

    void Awake()
    {
        Instance = this;
        EnsureDefaultPrefabs();
    }

    void Start()
    {
        EnsureDefaultPrefabs();
        ResetSpawner();
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.currentState != GameState.Playing)
            return;

        float speed = GameManager.Instance.currentSpeed;
        float distDelta = speed * Time.deltaTime;

        spawnZCursor += distDelta;
        while (spawnZCursor > -120f)
        {
            SpawnNextWave();
        }

        // Scroll active obstacles
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            GameObject obs = activeObstacles[i];
            if (obs == null || !obs.activeSelf) continue;

            obs.transform.position += Vector3.forward * distDelta;

            if (obs.transform.position.z > 14f)
            {
                obs.SetActive(false);
            }
        }

        // Scroll active orbs
        for (int i = activeOrbs.Count - 1; i >= 0; i--)
        {
            GameObject orb = activeOrbs[i];
            if (orb == null || !orb.activeSelf) continue;

            orb.transform.position += Vector3.forward * distDelta;

            if (PlayerController.Instance != null && PlayerController.Instance.hasMagnet)
            {
                if (orb.transform.position.z > -26f && orb.transform.position.z < 6f)
                {
                    ManaOrb mo = orb.GetComponent<ManaOrb>();
                    if (mo != null) mo.isMagnetized = true;
                }
            }

            if (orb.transform.position.z > 12f)
            {
                orb.SetActive(false);
            }
        }

        // Scroll active power-ups
        for (int i = activePowerUps.Count - 1; i >= 0; i--)
        {
            GameObject pow = activePowerUps[i];
            if (pow == null || !pow.activeSelf) continue;

            pow.transform.position += Vector3.forward * distDelta;
            pow.transform.Rotate(Vector3.up * (Time.deltaTime * 120f), Space.World);

            if (pow.transform.position.z > 12f)
            {
                pow.SetActive(false);
            }
        }
    }

    public void ResetSpawner()
    {
        EnsureDefaultPrefabs();

        foreach (var o in activeObstacles) if (o != null) o.SetActive(false);
        foreach (var o in activeOrbs) if (o != null) o.SetActive(false);
        foreach (var p in activePowerUps) if (p != null) p.SetActive(false);

        spawnZCursor = -65f;
        for (int i = 0; i < 5; i++)
        {
            SpawnNextWave();
        }
    }

    void SpawnNextWave()
    {
        EnsureDefaultPrefabs();

        float dist = GameManager.Instance != null ? GameManager.Instance.distance : 0f;
        float difficulty = Mathf.Clamp01(dist / 2200f);

        float waveSpacing = Mathf.Lerp(initialWaveSpacing, minWaveSpacing, Mathf.Pow(difficulty, 0.85f));
        spawnZCursor -= waveSpacing;

        List<int> availableLanes = new List<int> { 0, 1, 2 };
        List<int> chosenLanes = new List<int>();

        int obstacleCount = 1;
        if (dist < 320f)
        {
            obstacleCount = 1;
        }
        else if (dist < 900f)
        {
            obstacleCount = Random.value < 0.35f ? 2 : 1;
        }
        else
        {
            obstacleCount = Random.value < 0.55f ? 2 : 1;
        }

        for (int c = 0; c < obstacleCount; c++)
        {
            int idx = Random.Range(0, availableLanes.Count);
            chosenLanes.Add(availableLanes[idx]);
            availableLanes.RemoveAt(idx);
        }

        foreach (int laneIdx in chosenLanes)
        {
            // Clean 50/50 balance between Woodland Tree Stumps and Ancient Stone Monoliths (overhead branch removed!)
            ObstacleType type = (Random.value < 0.50f) ? ObstacleType.Log : ObstacleType.Monolith;

            GameObject obs = GetPooledObstacle(type);
            if (obs == null) continue;

            float posX = PlayerController.Instance != null ? PlayerController.Instance.GetTargetXForLane(laneIdx) : (laneIdx - 1) * 3.0f;
            obs.transform.position = new Vector3(posX, 0f, spawnZCursor);
            obs.SetActive(true);

            Obstacle comp = obs.GetComponent<Obstacle>();
            if (comp != null)
            {
                comp.obstacleType = type;
                comp.laneIndex = laneIdx;
                comp.isPassed = false;
            }
        }

        // Spawn Mana Orbs in open lanes
        if (availableLanes.Count > 0)
        {
            int orbLane = availableLanes[0];
            float orbX = PlayerController.Instance != null ? PlayerController.Instance.GetTargetXForLane(orbLane) : (orbLane - 1) * 3.0f;
            int orbCount = Random.Range(4, 6);

            for (int k = 0; k < orbCount; k++)
            {
                GameObject orb = GetPooledOrb();
                if (orb == null) continue;

                orb.transform.position = new Vector3(orbX, 1.15f, spawnZCursor + 4.0f - k * 2.2f);
                orb.SetActive(true);

                ManaOrb mo = orb.GetComponent<ManaOrb>();
                if (mo != null)
                {
                    mo.laneIndex = orbLane;
                    mo.isMagnetized = false;
                }
            }
        }

        // 3D Power-Up Symbol Spawning (Prominent, High-Visibility Icons)
        float powerChance = dist < 350f ? 0.22f : 0.16f;
        if (Random.value < powerChance && availableLanes.Count > 0)
        {
            int powLane = availableLanes[Random.Range(0, availableLanes.Count)];
            float powX = PlayerController.Instance != null ? PlayerController.Instance.GetTargetXForLane(powLane) : (powLane - 1) * 3.0f;

            PowerUpType[] types = { PowerUpType.Magnet, PowerUpType.Shield, PowerUpType.Dash };
            PowerUpType chosenType = types[Random.Range(0, types.Length)];

            GameObject pow = GetPooledPowerUp(chosenType);
            if (pow != null)
            {
                pow.transform.position = new Vector3(powX, 1.35f, spawnZCursor - waveSpacing * 0.45f);
                pow.SetActive(true);
            }
        }
    }

    GameObject GetPooledObstacle(ObstacleType type)
    {
        foreach (var o in activeObstacles)
        {
            if (o != null && !o.activeSelf)
            {
                Obstacle obsComp = o.GetComponent<Obstacle>();
                if (obsComp != null && obsComp.obstacleType == type)
                {
                    return o;
                }
            }
        }

        GameObject prefabToUse = logPrefab;
        if (type == ObstacleType.Monolith) prefabToUse = monolithPrefab;
        else if (type == ObstacleType.OverheadBranch) prefabToUse = overheadPrefab;

        if (prefabToUse == null)
        {
            EnsureDefaultPrefabs();
            prefabToUse = logPrefab;
            if (type == ObstacleType.Monolith) prefabToUse = monolithPrefab;
            else if (type == ObstacleType.OverheadBranch) prefabToUse = overheadPrefab;
        }

        if (prefabToUse == null) return null;

        GameObject newObj = Instantiate(prefabToUse);
        activeObstacles.Add(newObj);
        return newObj;
    }

    GameObject GetPooledOrb()
    {
        foreach (var orb in activeOrbs)
        {
            if (orb != null && !orb.activeSelf) return orb;
        }

        if (orbPrefab == null)
        {
            EnsureDefaultPrefabs();
        }

        if (orbPrefab == null) return null;

        GameObject newOrb = Instantiate(orbPrefab);
        activeOrbs.Add(newOrb);
        return newOrb;
    }

    GameObject GetPooledPowerUp(PowerUpType type)
    {
        foreach (var p in activePowerUps)
        {
            if (p != null && !p.activeSelf)
            {
                PowerUpItem pi = p.GetComponent<PowerUpItem>();
                if (pi != null && pi.powerUpType == type) return p;
            }
        }

        GameObject prefabToUse = magnetPrefab;
        if (type == PowerUpType.Shield) prefabToUse = shieldPrefab;
        else if (type == PowerUpType.Dash) prefabToUse = dashPrefab;

        if (prefabToUse == null)
        {
            EnsureDefaultPrefabs();
            if (type == PowerUpType.Magnet) prefabToUse = magnetPrefab;
            else if (type == PowerUpType.Shield) prefabToUse = shieldPrefab;
            else if (type == PowerUpType.Dash) prefabToUse = dashPrefab;
        }

        if (prefabToUse == null) return null;

        GameObject newPow = Instantiate(prefabToUse);
        activePowerUps.Add(newPow);
        return newPow;
    }

    public void EnsureDefaultPrefabs()
    {
        // 1. Woodland Tree Stump (2.8m Tall, 2.9m Wide - Fills the Lane!)
        if (logPrefab == null)
        {
            GameObject stumpRes = Resources.Load<GameObject>("Nature/Stump_01");
            logPrefab = new GameObject("Prefab_Log_Root");

            if (stumpRes != null)
            {
                GameObject visual = Instantiate(stumpRes, logPrefab.transform);
                visual.transform.localPosition = Vector3.zero;
                // Impressive ancient trunk: 2.8m tall, 2.9m wide across the lane
                visual.transform.localScale = new Vector3(4.4f, 3.45f, 4.4f);
            }
            else
            {
                GameObject mesh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                mesh.transform.parent = logPrefab.transform;
                mesh.transform.localPosition = new Vector3(0f, 1.4f, 0f);
                mesh.transform.localScale = new Vector3(2.4f, 2.8f, 2.4f);
                mesh.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                Destroy(mesh.GetComponent<Collider>());
                Material barkMat = SylvanMaterialHelper.CreateMaterial(new Color(0.25f, 0.15f, 0.08f), new Color(0.04f, 0.18f, 0.1f), 0.2f);
                mesh.GetComponent<Renderer>().material = barkMat;
            }

            BoxCollider col = logPrefab.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 1.4f, 0f);
            col.size = new Vector3(2.4f, 2.8f, 2.2f);
            col.isTrigger = true;

            Obstacle obs = logPrefab.AddComponent<Obstacle>();
            obs.obstacleType = ObstacleType.Log;
            obs.obstacleHeight = 2.8f;
            logPrefab.SetActive(false);
        }

        // 2. Ancient Stone Monolith (2.95m Tall, 2.6m Wide - Large Ancient Boulder!)
        if (monolithPrefab == null)
        {
            GameObject rockRes = Resources.Load<GameObject>("Nature/Rock_04");
            if (rockRes == null) rockRes = Resources.Load<GameObject>("Nature/Rock_01");

            monolithPrefab = new GameObject("Prefab_Monolith_Root");

            if (rockRes != null)
            {
                GameObject visual = Instantiate(rockRes, monolithPrefab.transform);
                visual.transform.localPosition = Vector3.zero;
                // Impressive large stone: 2.95m tall, 2.6m wide
                visual.transform.localScale = new Vector3(2.5f, 2.6f, 2.5f);
            }
            else
            {
                GameObject mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mesh.transform.parent = monolithPrefab.transform;
                mesh.transform.localPosition = new Vector3(0f, 1.45f, 0f);
                mesh.transform.localScale = new Vector3(2.2f, 2.95f, 1.8f);
                Destroy(mesh.GetComponent<Collider>());
                Material stoneMat = SylvanMaterialHelper.CreateMaterial(new Color(0.12f, 0.22f, 0.25f), new Color(0f, 0.85f, 0.75f), 0.5f);
                mesh.GetComponent<Renderer>().material = stoneMat;
            }

            BoxCollider col = monolithPrefab.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 1.45f, 0f);
            col.size = new Vector3(2.2f, 2.95f, 1.8f);
            col.isTrigger = true;

            Obstacle obs = monolithPrefab.AddComponent<Obstacle>();
            obs.obstacleType = ObstacleType.Monolith;
            obs.obstacleHeight = 2.95f;
            monolithPrefab.SetActive(false);
        }

        // 3. Single-Lane Overhead Branch Arch (2.1m Wide - Confined Strictly to Its Own Lane!)
        if (overheadPrefab == null)
        {
            GameObject branchRes = Resources.Load<GameObject>("Nature/Branch_01");
            overheadPrefab = new GameObject("Prefab_Overhead_Root");

            if (branchRes != null)
            {
                GameObject visual = Instantiate(branchRes, overheadPrefab.transform);
                visual.transform.localPosition = new Vector3(0f, 2.05f, 0f);
                // Scaled to 2.1m wide so it never extends or spills into adjacent lanes!
                visual.transform.localScale = new Vector3(2.6f, 2.0f, 2.0f);
            }
            else
            {
                GameObject mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
                mesh.transform.parent = overheadPrefab.transform;
                mesh.transform.localPosition = new Vector3(0f, 2.05f, 0f);
                mesh.transform.localScale = new Vector3(2.1f, 0.65f, 0.6f);
                Destroy(mesh.GetComponent<Collider>());
                Material archMat = SylvanMaterialHelper.CreateMaterial(new Color(0.18f, 0.12f, 0.06f), new Color(0.08f, 0.45f, 0.2f), 0.3f);
                mesh.GetComponent<Renderer>().material = archMat;
            }

            BoxCollider col = overheadPrefab.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 2.05f, 0f);
            col.size = new Vector3(1.7f, 0.85f, 1.1f);
            col.isTrigger = true;

            Obstacle obs = overheadPrefab.AddComponent<Obstacle>();
            obs.obstacleType = ObstacleType.OverheadBranch;
            obs.obstacleHeight = 1.4f;
            overheadPrefab.SetActive(false);
        }

        // 4. Procedural Mana Orb Prefab
        if (orbPrefab == null)
        {
            orbPrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orbPrefab.name = "Prefab_ManaOrb";
            orbPrefab.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
            Material orbMat = SylvanMaterialHelper.CreateMaterial(new Color(0f, 0.96f, 0.83f), new Color(0f, 0.96f, 0.83f) * 3f, 0.8f);
            orbPrefab.GetComponent<Renderer>().material = orbMat;

            orbPrefab.AddComponent<ManaOrb>();
            orbPrefab.GetComponent<Collider>().isTrigger = true;
            orbPrefab.SetActive(false);
        }

        // 5. Prominent 3D Horseshoe Magnet Power-Up
        if (magnetPrefab == null)
        {
            magnetPrefab = CreateMagnetSymbolPrefab();
            magnetPrefab.SetActive(false);
        }

        // 6. Prominent 3D Bark Shield Power-Up
        if (shieldPrefab == null)
        {
            shieldPrefab = CreateShieldSymbolPrefab();
            shieldPrefab.SetActive(false);
        }

        // 7. Prominent 3D Wind Dash Speed Arrow Power-Up
        if (dashPrefab == null)
        {
            dashPrefab = CreateDashSymbolPrefab();
            dashPrefab.SetActive(false);
        }
    }

    /// <summary>
    /// Creates a prominent, arcade-scale 3D Horseshoe Magnet symbol.
    /// </summary>
    GameObject CreateMagnetSymbolPrefab()
    {
        GameObject root = new GameObject("Prefab_PowerUp_Magnet");

        BoxCollider col = root.AddComponent<BoxCollider>();
        col.size = new Vector3(1.5f, 1.5f, 0.8f);
        col.isTrigger = true;

        PowerUpItem item = root.AddComponent<PowerUpItem>();
        item.powerUpType = PowerUpType.Magnet;

        Material bodyMat = SylvanMaterialHelper.CreateMaterial(new Color(0.75f, 0.35f, 1.0f), new Color(0.60f, 0.22f, 0.95f), 0.7f);
        Material tipMat = SylvanMaterialHelper.CreateMaterial(new Color(1.0f, 1.0f, 1.0f), new Color(0.95f, 0.95f, 1.0f), 0.95f);

        GameObject visualGroup = new GameObject("Visual");
        visualGroup.transform.parent = root.transform;
        visualGroup.transform.localPosition = Vector3.zero;
        visualGroup.transform.localScale = Vector3.one * 1.85f; // Prominent arcade size!

        // Bottom bridge
        GameObject bottom = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bottom.transform.parent = visualGroup.transform;
        bottom.transform.localPosition = new Vector3(0f, -0.25f, 0f);
        bottom.transform.localScale = new Vector3(0.68f, 0.20f, 0.22f);
        Destroy(bottom.GetComponent<Collider>());
        bottom.GetComponent<Renderer>().material = bodyMat;

        // Left vertical prong
        GameObject leftArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftArm.transform.parent = visualGroup.transform;
        leftArm.transform.localPosition = new Vector3(-0.25f, 0.04f, 0f);
        leftArm.transform.localScale = new Vector3(0.18f, 0.44f, 0.22f);
        Destroy(leftArm.GetComponent<Collider>());
        leftArm.GetComponent<Renderer>().material = bodyMat;

        // Right vertical prong
        GameObject rightArm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightArm.transform.parent = visualGroup.transform;
        rightArm.transform.localPosition = new Vector3(0.25f, 0.04f, 0f);
        rightArm.transform.localScale = new Vector3(0.18f, 0.44f, 0.22f);
        Destroy(rightArm.GetComponent<Collider>());
        rightArm.GetComponent<Renderer>().material = bodyMat;

        // Left silver magnetic pole
        GameObject leftTip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftTip.transform.parent = visualGroup.transform;
        leftTip.transform.localPosition = new Vector3(-0.25f, 0.33f, 0f);
        leftTip.transform.localScale = new Vector3(0.20f, 0.16f, 0.24f);
        Destroy(leftTip.GetComponent<Collider>());
        leftTip.GetComponent<Renderer>().material = tipMat;

        // Right silver magnetic pole
        GameObject rightTip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightTip.transform.parent = visualGroup.transform;
        rightTip.transform.localPosition = new Vector3(0.25f, 0.33f, 0f);
        rightTip.transform.localScale = new Vector3(0.20f, 0.16f, 0.24f);
        Destroy(rightTip.GetComponent<Collider>());
        rightTip.GetComponent<Renderer>().material = tipMat;

        return root;
    }

    /// <summary>
    /// Creates a prominent, arcade-scale 3D Heraldic Shield symbol.
    /// </summary>
    GameObject CreateShieldSymbolPrefab()
    {
        GameObject root = new GameObject("Prefab_PowerUp_Shield");

        BoxCollider col = root.AddComponent<BoxCollider>();
        col.size = new Vector3(1.5f, 1.6f, 0.8f);
        col.isTrigger = true;

        PowerUpItem item = root.AddComponent<PowerUpItem>();
        item.powerUpType = PowerUpType.Shield;

        Material shieldMat = SylvanMaterialHelper.CreateMaterial(new Color(0.18f, 0.95f, 0.45f), new Color(0.12f, 0.80f, 0.32f), 0.75f);
        Material bossMat = SylvanMaterialHelper.CreateMaterial(new Color(1.0f, 0.84f, 0.22f), new Color(0.95f, 0.75f, 0.15f), 0.9f);

        GameObject visualGroup = new GameObject("Visual");
        visualGroup.transform.parent = root.transform;
        visualGroup.transform.localPosition = Vector3.zero;
        visualGroup.transform.localScale = Vector3.one * 1.85f; // Prominent arcade size!

        // Upper protective plate
        GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plate.transform.parent = visualGroup.transform;
        plate.transform.localPosition = new Vector3(0f, 0.14f, 0f);
        plate.transform.localScale = new Vector3(0.72f, 0.50f, 0.15f);
        Destroy(plate.GetComponent<Collider>());
        plate.GetComponent<Renderer>().material = shieldMat;

        // Lower tapered point
        GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tip.transform.parent = visualGroup.transform;
        tip.transform.localPosition = new Vector3(0f, -0.12f, 0f);
        tip.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        tip.transform.localScale = new Vector3(0.48f, 0.48f, 0.15f);
        Destroy(tip.GetComponent<Collider>());
        tip.GetComponent<Renderer>().material = shieldMat;

        // Golden center emblem boss
        GameObject boss = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        boss.transform.parent = visualGroup.transform;
        boss.transform.localPosition = new Vector3(0f, 0.08f, 0f);
        boss.transform.localScale = new Vector3(0.28f, 0.28f, 0.24f);
        Destroy(boss.GetComponent<Collider>());
        boss.GetComponent<Renderer>().material = bossMat;

        return root;
    }

    /// <summary>
    /// Creates a prominent, arcade-scale 3D Speed Arrow (>>) symbol.
    /// </summary>
    GameObject CreateDashSymbolPrefab()
    {
        GameObject root = new GameObject("Prefab_PowerUp_Dash");

        BoxCollider col = root.AddComponent<BoxCollider>();
        col.size = new Vector3(1.5f, 1.5f, 0.8f);
        col.isTrigger = true;

        PowerUpItem item = root.AddComponent<PowerUpItem>();
        item.powerUpType = PowerUpType.Dash;

        Material cyanMat = SylvanMaterialHelper.CreateMaterial(new Color(0.0f, 0.98f, 0.90f), new Color(0.0f, 0.92f, 0.84f), 0.85f);

        GameObject visualGroup = new GameObject("Visual");
        visualGroup.transform.parent = root.transform;
        visualGroup.transform.localPosition = Vector3.zero;
        visualGroup.transform.localScale = Vector3.one * 1.85f; // Prominent arcade size!

        // Front Chevron Arrow
        GameObject c1Left = GameObject.CreatePrimitive(PrimitiveType.Cube);
        c1Left.transform.parent = visualGroup.transform;
        c1Left.transform.localPosition = new Vector3(-0.14f, 0.16f, 0f);
        c1Left.transform.localRotation = Quaternion.Euler(0f, 0f, -34f);
        c1Left.transform.localScale = new Vector3(0.12f, 0.38f, 0.15f);
        Destroy(c1Left.GetComponent<Collider>());
        c1Left.GetComponent<Renderer>().material = cyanMat;

        GameObject c1Right = GameObject.CreatePrimitive(PrimitiveType.Cube);
        c1Right.transform.parent = visualGroup.transform;
        c1Right.transform.localPosition = new Vector3(0.14f, 0.16f, 0f);
        c1Right.transform.localRotation = Quaternion.Euler(0f, 0f, 34f);
        c1Right.transform.localScale = new Vector3(0.12f, 0.38f, 0.15f);
        Destroy(c1Right.GetComponent<Collider>());
        c1Right.GetComponent<Renderer>().material = cyanMat;

        // Rear Chevron Arrow
        GameObject c2Left = GameObject.CreatePrimitive(PrimitiveType.Cube);
        c2Left.transform.parent = visualGroup.transform;
        c2Left.transform.localPosition = new Vector3(-0.14f, -0.18f, 0f);
        c2Left.transform.localRotation = Quaternion.Euler(0f, 0f, -34f);
        c2Left.transform.localScale = new Vector3(0.12f, 0.38f, 0.15f);
        Destroy(c2Left.GetComponent<Collider>());
        c2Left.GetComponent<Renderer>().material = cyanMat;

        GameObject c2Right = GameObject.CreatePrimitive(PrimitiveType.Cube);
        c2Right.transform.parent = visualGroup.transform;
        c2Right.transform.localPosition = new Vector3(0.14f, -0.18f, 0f);
        c2Right.transform.localRotation = Quaternion.Euler(0f, 0f, 34f);
        c2Right.transform.localScale = new Vector3(0.12f, 0.38f, 0.15f);
        Destroy(c2Right.GetComponent<Collider>());
        c2Right.GetComponent<Renderer>().material = cyanMat;

        // Glowing center core spark
        GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        core.transform.parent = visualGroup.transform;
        core.transform.localPosition = new Vector3(0f, 0f, 0f);
        core.transform.localScale = new Vector3(0.20f, 0.20f, 0.20f);
        Destroy(core.GetComponent<Collider>());
        core.GetComponent<Renderer>().material = cyanMat;

        return root;
    }
}
