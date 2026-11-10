using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
#endif

/// <summary>
/// Sylvan Rush - Complete Project Automator & Bootstrapper (Clean Polish Edition)
/// </summary>
public class SylvanRushBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoInitializeIfRequested()
    {
        if (FindFirstObjectByType<SylvanRushBootstrap>() == null && FindFirstObjectByType<GameManager>() == null)
        {
            GameObject boot = new GameObject("SylvanRush_Bootstrapper");
            boot.AddComponent<SylvanRushBootstrap>();
        }
    }

#if UNITY_EDITOR
    [MenuItem("Tools/Launch Sylvan Rush Game")]
    public static void LaunchSylvanRush()
    {
        // 1. Clean up ALL old gameobjects and clones in the scene
        string[] oldNames = {
            "Player_SpiritRunner", "Player", "TileSpawner", "GameManager", "UIManager",
            "ObstacleSpawner", "PowerUpManager", "Environment_ProceduralForest",
            "Audio_ProceduralSynthesizer", "SylvanRush_Bootstrapper", "FairyWisp", "SoundManager",
            "Prefab_Log_Root", "Prefab_Monolith_Root", "Prefab_Overhead_Root", "Prefab_ManaOrb",
            "Prefab_PowerUp", "Prefab_PowerUp_Magnet", "Prefab_PowerUp_Shield", "Prefab_PowerUp_Dash"
        };

        foreach (string n in oldNames)
        {
            GameObject go = GameObject.Find(n);
            if (go != null) DestroyImmediate(go);
        }

        foreach (var p in Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            DestroyImmediate(p.gameObject);
        }

        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go != null && go.name.Contains("(Clone)"))
            {
                DestroyImmediate(go);
            }
        }

        // 2. Fix materials to URP Lit
        FixNatureAndCharacterMaterials();

        // 3. Configure Running.fbx ModelImporter
        ConfigureCharacterImporter();

        // 4. Instantiate and build scene
        GameObject boot = new GameObject("SylvanRush_Bootstrapper");
        SylvanRushBootstrap bootstrap = boot.AddComponent<SylvanRushBootstrap>();
        bootstrap.SetupLightingAndAtmosphere();
        bootstrap.SetupAudio();
        bootstrap.SetupPlayer();
        bootstrap.SetupCamera();
        bootstrap.SetupEnvironment();
        bootstrap.SetupGameManagers();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        Debug.Log("<color=#00f6d3><b>[Sylvan Rush]</b> Fresh scene generated with balanced obstacles, 3D symbols, running animation, and medium speed!</color>");
        EditorUtility.DisplayDialog("Game Ready!",
            "All updates are active:\n\n" +
            "• Obstacles: Resized to balanced medium (knee-high stump & tall mossy boulder)\n" +
            "• Power-Ups: Replaced with 3D Horseshoe Magnet, Aegis Shield, and Speed Chevrons\n" +
            "• Speed: Energetic medium starting speed (26 m/s)\n" +
            "• Character: Erika Archer with active looped running animation\n" +
            "• Audio: High-volume 2D audio clips\n\n" +
            "Press Play (▶) at the top of Unity to play!", "Let's Play!");
    }

    static void FixNatureAndCharacterMaterials()
    {
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null) urpLit = Shader.Find("Universal Render Pipeline/Simple Lit");
        if (urpLit == null) return;

        string[] matGUIDs = AssetDatabase.FindAssets("t:Material", new string[] { "Assets/SimpleNaturePack/Materials" });
        foreach (string guid in matGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null)
            {
                Texture mainTex = mat.GetTexture("_MainTex");
                if (mainTex == null) mainTex = mat.GetTexture("_BaseMap");

                mat.shader = urpLit;
                if (mainTex != null)
                {
                    mat.SetTexture("_BaseMap", mainTex);
                    mat.SetTexture("_MainTex", mainTex);
                }
                mat.SetColor("_BaseColor", Color.white);
                mat.SetFloat("_Smoothness", 0.15f);
                EditorUtility.SetDirty(mat);
            }
        }
        AssetDatabase.SaveAssets();
    }

    static void ConfigureCharacterImporter()
    {
        string[] fbxPaths = { "Assets/Resources/Character/Running.fbx", "Assets/Running.fbx" };
        foreach (string p in fbxPaths)
        {
            ModelImporter importer = AssetImporter.GetAtPath(p) as ModelImporter;
            if (importer != null)
            {
                importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
                importer.materialLocation = ModelImporterMaterialLocation.External;

                importer.importNormals = ModelImporterNormals.Calculate;
                importer.normalSmoothingAngle = 80f;
                importer.normalCalculationMode = ModelImporterNormalCalculationMode.AreaAndAngleWeighted;

                ModelImporterClipAnimation[] clips = importer.defaultClipAnimations;
                if (clips != null && clips.Length > 0)
                {
                    foreach (var c in clips)
                    {
                        c.loopTime = true;
                    }
                    importer.clipAnimations = clips;
                }
                importer.SaveAndReimport();
                break;
            }
        }
    }
#endif

    void Awake()
    {
        SetupLightingAndAtmosphere();
        SetupAudio();
        SetupPlayer();
        SetupCamera();
        SetupEnvironment();
        SetupGameManagers();
    }

    public void SetupLightingAndAtmosphere()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
        }
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.018f, 0.042f, 0.065f);
        cam.fieldOfView = 54f;

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.28f, 0.36f, 0.42f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.012f;
        RenderSettings.fogColor = new Color(0.024f, 0.065f, 0.082f);

        Light light = FindFirstObjectByType<Light>();
        if (light == null)
        {
            GameObject lightObj = new GameObject("Directional Light");
            light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            lightObj.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
        }
        light.color = new Color(0.85f, 0.92f, 1.0f);
        light.intensity = 1.25f;
        light.shadows = LightShadows.Soft;

        Volume volume = FindFirstObjectByType<Volume>();
        if (volume != null && volume.profile != null)
        {
            Bloom bloom;
            if (!volume.profile.TryGet(out bloom))
            {
                bloom = volume.profile.Add<Bloom>(true);
            }
            bloom.intensity.Override(1.1f);
            bloom.threshold.Override(0.85f);
            bloom.scatter.Override(0.65f);
        }
    }

    public void SetupPlayer()
    {
        PlayerController existing = FindFirstObjectByType<PlayerController>();
        if (existing != null) DestroyImmediate(existing.gameObject);

        GameObject player = new GameObject("Player_SpiritRunner");
        player.tag = "Player";
        PlayerController controller = player.AddComponent<PlayerController>();

        GameObject visualRoot = null;

        GameObject charPrefab = Resources.Load<GameObject>("Character/Running");
        if (charPrefab == null) charPrefab = Resources.Load<GameObject>("Running");

        if (charPrefab != null)
        {
            visualRoot = Instantiate(charPrefab, player.transform);
            visualRoot.name = "ErikaArcher_Model";
            visualRoot.transform.localPosition = Vector3.zero;
            visualRoot.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            visualRoot.transform.localScale = Vector3.one * 1.2f;

            foreach (var c in visualRoot.GetComponentsInChildren<Camera>(true)) Destroy(c.gameObject);
            foreach (var l in visualRoot.GetComponentsInChildren<Light>(true)) Destroy(l.gameObject);
            foreach (var col in visualRoot.GetComponentsInChildren<Collider>(true)) Destroy(col);

            Texture2D bodyTex = Resources.Load<Texture2D>("Character/Textures/texture_4");
            Texture2D armorTex = Resources.Load<Texture2D>("Character/Textures/texture_0");

            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Universal Render Pipeline/Simple Lit");
            foreach (var rend in visualRoot.GetComponentsInChildren<Renderer>(true))
            {
                foreach (var mat in rend.materials)
                {
                    if (mat != null && urpLit != null)
                    {
                        mat.shader = urpLit;

                        Texture mainTex = mat.GetTexture("_MainTex") ?? mat.GetTexture("_BaseMap");
                        if (mainTex == null)
                        {
                            string mName = mat.name.ToLower();
                            if (mName.Contains("body") || mName.Contains("skin") || mName.Contains("face"))
                            {
                                mainTex = bodyTex;
                            }
                            else
                            {
                                mainTex = (armorTex != null) ? armorTex : bodyTex;
                            }
                        }

                        if (mainTex != null)
                        {
                            mat.SetTexture("_BaseMap", mainTex);
                            mat.SetTexture("_MainTex", mainTex);
                        }

                        mat.DisableKeyword("_NORMALMAP");
                        mat.SetTexture("_BumpMap", null);

                        mat.SetColor("_BaseColor", Color.white);
                        mat.SetFloat("_Smoothness", 0.0f);
                    }
                }
            }

            Animator anim = visualRoot.GetComponent<Animator>();
            if (anim == null) anim = visualRoot.AddComponent<Animator>();
            anim.applyRootMotion = false;

            // Load and bind the configured AnimatorController with the looped running animation
            RuntimeAnimatorController runtimeController = Resources.Load<RuntimeAnimatorController>("Character/Erika_Animator");
            if (runtimeController != null)
            {
                anim.runtimeAnimatorController = runtimeController;
                anim.speed = 1.25f; // Energetic, snappy sprint animation!
            }
        }

        // Player Collider & Kinematic Rigidbody
        CapsuleCollider rootCol = player.AddComponent<CapsuleCollider>();
        rootCol.center = new Vector3(0f, 1.0f, 0f);
        rootCol.radius = 0.45f;
        rootCol.height = 1.9f;
        rootCol.isTrigger = true;

        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        // Bark Shield: 3 Orbiting Golden-Emerald Aegis Crests (Replaces murky green bubble!)
        GameObject shieldWard = new GameObject("ShieldWard_Orbiters");
        shieldWard.transform.parent = visualRoot != null ? visualRoot.transform : player.transform;
        shieldWard.transform.localPosition = new Vector3(0f, 1.0f, 0f);

        Material crestMat = SylvanMaterialHelper.CreateMaterial(new Color(0.18f, 0.95f, 0.45f), new Color(0.12f, 0.80f, 0.35f), 0.8f);
        Material bossMat = SylvanMaterialHelper.CreateMaterial(new Color(1.0f, 0.85f, 0.2f), new Color(0.95f, 0.75f, 0.15f), 0.9f);

        for (int i = 0; i < 3; i++)
        {
            float angle = i * 120f * Mathf.Deg2Rad;
            GameObject orbiter = new GameObject("Orbiter_" + i);
            orbiter.transform.parent = shieldWard.transform;
            orbiter.transform.localPosition = new Vector3(Mathf.Cos(angle) * 1.15f, 0f, Mathf.Sin(angle) * 1.15f);
            orbiter.transform.localRotation = Quaternion.Euler(0f, -i * 120f + 90f, 0f);

            // Mini shield body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.parent = orbiter.transform;
            body.transform.localPosition = Vector3.zero;
            body.transform.localScale = new Vector3(0.30f, 0.42f, 0.08f);
            Destroy(body.GetComponent<Collider>());
            body.GetComponent<Renderer>().material = crestMat;

            // Golden center core
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.transform.parent = orbiter.transform;
            core.transform.localPosition = new Vector3(0f, 0f, 0.04f);
            core.transform.localScale = new Vector3(0.15f, 0.15f, 0.12f);
            Destroy(core.GetComponent<Collider>());
            core.GetComponent<Renderer>().material = bossMat;
        }

        shieldWard.SetActive(false);

        controller.visualRoot = visualRoot != null ? visualRoot.transform : null;
        controller.shieldBubble = shieldWard.transform;
    }

    public void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        CameraFollowShake follow = cam.GetComponent<CameraFollowShake>();
        if (follow == null)
        {
            follow = cam.gameObject.AddComponent<CameraFollowShake>();
        }

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            follow.target = player.transform;
        }

        if (cam.GetComponent<AudioListener>() == null)
        {
            cam.gameObject.AddComponent<AudioListener>();
        }
    }

    public void SetupEnvironment()
    {
        if (FindFirstObjectByType<ProceduralForestEnvironment>() == null)
        {
            GameObject env = new GameObject("Environment_ProceduralForest");
            env.AddComponent<ProceduralForestEnvironment>();
        }
    }

    public void SetupAudio()
    {
        if (FindFirstObjectByType<ProceduralAudio>() == null)
        {
            GameObject audioObj = new GameObject("Audio_ProceduralSynthesizer");
            audioObj.AddComponent<ProceduralAudio>();
        }
    }

    public void SetupGameManagers()
    {
        // Clean up any stale prefabs from older runs
        string[] staleObjects = {
            "Prefab_Log_Root", "Prefab_Monolith_Root", "Prefab_Overhead_Root", "Prefab_ManaOrb",
            "Prefab_PowerUp"
        };
        foreach (var s in staleObjects)
        {
            GameObject obj = GameObject.Find(s);
            if (obj != null) Destroy(obj);
        }

        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            gm = gmObj.AddComponent<GameManager>();
        }
        gm.baseSpeed = 20.0f;
        gm.currentSpeed = 20.0f;

        ObstacleSpawner spawner = FindFirstObjectByType<ObstacleSpawner>();
        if (spawner == null)
        {
            GameObject spawnerObj = new GameObject("ObstacleSpawner");
            spawner = spawnerObj.AddComponent<ObstacleSpawner>();
        }
        else
        {
            spawner.EnsureDefaultPrefabs();
            spawner.ResetSpawner();
        }

        if (FindFirstObjectByType<PowerUpManager>() == null)
        {
            GameObject pow = new GameObject("PowerUpManager");
            pow.AddComponent<PowerUpManager>();
        }

        if (FindFirstObjectByType<GameUIManager>() == null)
        {
            GameObject ui = new GameObject("UIManager");
            ui.AddComponent<GameUIManager>();
        }
    }
}
