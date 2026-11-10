using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObstacleType
{
    Log,      // Woodland Fallen Log (0.74m height, jumpable)
    Monolith  // Ancient Rune Stone Cairn (1.56m height, lane-dodge)
}

/// <summary>
/// Attached to every spawned obstacle prefab/mesh.
/// </summary>
public class Obstacle : MonoBehaviour
{
    public ObstacleType obstacleType;
    public float obstacleHeight = 0.74f;
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
        // Gentle rotation & floating
        transform.Rotate(Vector3.up * (Time.deltaTime * 180f));

        if (isMagnetized && PlayerController.Instance != null)
        {
            transform.position = Vector3.Lerp(transform.position, PlayerController.Instance.transform.position + Vector3.up * 1f, Time.deltaTime * 12f);
        }
    }
}

/// <summary>
/// Sylvan Rush - Procedural Obstacle & Reward Spawner
/// Recycles objects along the scrolling Z-axis with balanced distance-based pacing.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{
    public static ObstacleSpawner Instance { get; private set; }

    [Header("Wave Spacing Settings")]
    public float initialWaveSpacing = 18f;
    public float minWaveSpacing = 11.5f;

    [Header("Prefabs & Materials (Auto-Generated if Null)")]
    public GameObject logPrefab;
    public GameObject monolithPrefab;
    public GameObject orbPrefab;
    public GameObject powerUpPrefab;

    private float spawnZCursor = -24f;
    private List<GameObject> activeObstacles = new List<GameObject>();
    private List<GameObject> activeOrbs = new List<GameObject>();
    private List<GameObject> activePowerUps = new List<GameObject>();

    void Awake()
    {
        Instance = this;
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

        // Advance spawn cursor
        spawnZCursor += distDelta;
        while (spawnZCursor > -120f)
        {
            SpawnNextWave();
        }

        // Scroll active obstacles toward player (along +Z)
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            GameObject obs = activeObstacles[i];
            if (obs == null || !obs.activeSelf) continue;

            obs.transform.position += Vector3.forward * distDelta;

            if (obs.transform.position.z > 12f)
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

            // Magnet check
            if (PlayerController.Instance != null && PlayerController.Instance.hasMagnet)
            {
                if (orb.transform.position.z > -22f && orb.transform.position.z < 4f)
                {
                    ManaOrb mo = orb.GetComponent<ManaOrb>();
                    if (mo != null) mo.isMagnetized = true;
                }
            }

            if (orb.transform.position.z > 10f)
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
            pow.transform.Rotate(Vector3.up * (Time.deltaTime * 120f));

            if (pow.transform.position.z > 10f)
            {
                pow.SetActive(false);
            }
        }
    }

    public void ResetSpawner()
    {
        foreach (var o in activeObstacles) if (o != null) o.SetActive(false);
        foreach (var o in activeOrbs) if (o != null) o.SetActive(false);
        foreach (var p in activePowerUps) if (p != null) p.SetActive(false);

        spawnZCursor = -24f;
        for (int i = 0; i < 7; i++)
        {
            SpawnNextWave();
        }
    }

    void SpawnNextWave()
    {
        float dist = GameManager.Instance != null ? GameManager.Instance.distance : 0f;
        float difficulty = Mathf.Clamp01(dist / 2200f);

        // Wave spacing tightens dynamically
        float waveSpacing = Mathf.Lerp(initialWaveSpacing, minWaveSpacing, Mathf.Pow(difficulty, 0.85f));
        spawnZCursor -= waveSpacing;

        List<int> availableLanes = new List<int> { 0, 1, 2 };
        List<int> chosenLanes = new List<int>();

        // Obstacle count determination
        int obstacleCount = 1;
        if (dist < 320f)
        {
            obstacleCount = Random.value < 0.22f ? 2 : 1;
        }
        else if (dist < 900f)
        {
            obstacleCount = Random.value < 0.42f ? 2 : 1;
        }
        else
        {
            obstacleCount = Random.value < Mathf.Lerp(0.45f, 0.72f, (dist - 900f) / 1300f) ? 2 : 1;
        }

        for (int c = 0; c < obstacleCount; c++)
        {
            int idx = Random.Range(0, availableLanes.Count);
            chosenLanes.Add(availableLanes[idx]);
            availableLanes.RemoveAt(idx);
        }

        // Spawn obstacles
        foreach (int laneIdx in chosenLanes)
        {
            float monolithChance = dist < 320f ? 0.25f : Mathf.Lerp(0.32f, 0.52f, difficulty);
            ObstacleType type = Random.value < monolithChance ? ObstacleType.Monolith : ObstacleType.Log;

            GameObject obs = GetPooledObstacle(type);
            float posX = PlayerController.Instance != null ? PlayerController.Instance.GetTargetXForLane(laneIdx) : (laneIdx - 1) * 2.65f;
            obs.transform.position = new Vector3(posX, 0f, spawnZCursor);
            obs.SetActive(true);

            Obstacle comp = obs.GetComponent<Obstacle>();
            if (comp != null)
            {
                comp.obstacleType = type;
                comp.obstacleHeight = type == ObstacleType.Log ? 0.74f : 1.56f;
                comp.laneIndex = laneIdx;
                comp.isPassed = false;
            }
        }

        // Spawn Mana Orbs in the remaining open lane
        if (availableLanes.Count > 0)
        {
            int orbLane = availableLanes[0];
            float orbX = (orbLane - 1) * 2.65f;
            int orbCount = dist < 350f ? Random.Range(5, 7) : Random.Range(4, 6);

            for (int k = 0; k < orbCount; k++)
            {
                GameObject orb = GetPooledOrb();
                orb.transform.position = new Vector3(orbX, 1.05f, spawnZCursor + 4.0f - k * 2.0f);
                orb.SetActive(true);

                ManaOrb mo = orb.GetComponent<ManaOrb>();
                if (mo != null)
                {
                    mo.laneIndex = orbLane;
                    mo.isMagnetized = false;
                }
            }
        }

        // Power-Up Spawning (24% early, 18% late)
        float powerChance = dist < 350f ? 0.24f : 0.18f;
        if (Random.value < powerChance && availableLanes.Count > 0)
        {
            int powLane = availableLanes[Random.Range(0, availableLanes.Count)];
            float powX = (powLane - 1) * 2.65f;

            GameObject pow = GetPooledPowerUp();
            pow.transform.position = new Vector3(powX, 1.15f, spawnZCursor - waveSpacing * 0.45f);
            pow.SetActive(true);

            PowerUpItem pi = pow.GetComponent<PowerUpItem>();
            if (pi != null)
            {
                PowerUpType[] types = { PowerUpType.Magnet, PowerUpType.Shield, PowerUpType.Dash };
                pi.powerUpType = types[Random.Range(0, types.Length)];
                pi.ApplyColor();
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

        GameObject newObj = type == ObstacleType.Log ? Instantiate(logPrefab) : Instantiate(monolithPrefab);
        newObj.tag = "Obstacle";
        activeObstacles.Add(newObj);
        return newObj;
    }

    GameObject GetPooledOrb()
    {
        foreach (var orb in activeOrbs)
        {
            if (orb != null && !orb.activeSelf) return orb;
        }

        GameObject newOrb = Instantiate(orbPrefab);
        newOrb.tag = "Orb";
        activeOrbs.Add(newOrb);
        return newOrb;
    }

    GameObject GetPooledPowerUp()
    {
        foreach (var p in activePowerUps)
        {
            if (p != null && !p.activeSelf) return p;
        }

        GameObject newPow = Instantiate(powerUpPrefab);
        newPow.tag = "PowerUp";
        activePowerUps.Add(newPow);
        return newPow;
    }

    void EnsureDefaultPrefabs()
    {
        // 1. Procedural Log Prefab
        if (logPrefab == null)
        {
            logPrefab = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            logPrefab.name = "Prefab_Log";
            logPrefab.transform.localScale = new Vector3(0.72f, 1.15f, 0.72f);
            logPrefab.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            var rend = logPrefab.GetComponent<Renderer>();
            rend.material.color = new Color(0.25f, 0.16f, 0.08f); // Bark brown
            Obstacle obs = logPrefab.AddComponent<Obstacle>();
            obs.obstacleType = ObstacleType.Log;
            obs.obstacleHeight = 0.74f;
            logPrefab.GetComponent<Collider>().isTrigger = true;
            logPrefab.SetActive(false);
        }

        // 2. Procedural Rune Monolith Prefab
        if (monolithPrefab == null)
        {
            monolithPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
            monolithPrefab.name = "Prefab_Monolith";
            monolithPrefab.transform.localScale = new Vector3(1.15f, 1.56f, 0.85f);
            var rend = monolithPrefab.GetComponent<Renderer>();
            rend.material.color = new Color(0.12f, 0.35f, 0.32f); // Rune emerald stone
            Obstacle obs = monolithPrefab.AddComponent<Obstacle>();
            obs.obstacleType = ObstacleType.Monolith;
            obs.obstacleHeight = 1.56f;
            monolithPrefab.GetComponent<Collider>().isTrigger = true;
            monolithPrefab.SetActive(false);
        }

        // 3. Procedural Mana Orb Prefab
        if (orbPrefab == null)
        {
            orbPrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orbPrefab.name = "Prefab_ManaOrb";
            orbPrefab.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            var rend = orbPrefab.GetComponent<Renderer>();
            rend.material.color = new Color(0f, 0.96f, 0.83f); // Glowing cyan
            orbPrefab.AddComponent<ManaOrb>();
            orbPrefab.GetComponent<Collider>().isTrigger = true;
            orbPrefab.SetActive(false);
        }

        // 4. Procedural Power-Up Prefab
        if (powerUpPrefab == null)
        {
            powerUpPrefab = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            powerUpPrefab.name = "Prefab_PowerUp";
            powerUpPrefab.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
            powerUpPrefab.AddComponent<PowerUpItem>();
            powerUpPrefab.GetComponent<Collider>().isTrigger = true;
            powerUpPrefab.SetActive(false);
        }
    }
}
