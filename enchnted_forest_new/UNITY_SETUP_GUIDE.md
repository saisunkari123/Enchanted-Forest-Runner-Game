# SYLVAN RUSH - COMPLETE UNITY PROJECT SETUP GUIDE

## 1. Why Did Dragging the Web Files Into Unity Cause Errors?
When you download or export files from **Google AI Studio**, the project is written in **Web Technologies**:
* **TypeScript (`.ts`, `.tsx`)**, **React**, **Three.js**, **Node/Vite**, and `package.json`.
* **Unity does NOT compile TypeScript or React!** Unity is a completely different game engine that exclusively compiles **C# (`.cs`)** scripts and runs on the Mono/.NET runtime.
* When you dragged `package.json`, `vite.config.ts`, and `.tsx` files into Unity's `Assets` folder, Unity tried to parse them as unknown code, which caused version mismatches, syntax errors, and broken project references.

---

## 2. The Solution: Complete Unity C# Scripts Included
We have written the complete, pure **C# Unity version** of **Sylvan Rush: 3D Enchanted Forest Endless Runner**!
All files are located in the folder:
`/unity_scripts/`

### List of Included Unity C# Scripts:
1. `PlayerController.cs`: 3-lane kinematic movement (-2.65, 0, 2.65), smooth damping, jump gravity, coyote time (0.095s), jump buffer (0.15s), and banking tilt.
2. `GameManager.cs`: Score, distance, combo multiplier, dynamic speed ramping (16.5m/s to 33.5m/s), game over, and restart.
3. `ObstacleSpawner.cs`: Procedural spawner with object pooling. Spawns low jumpable Woodland Fallen Logs (0.74m), Ancient Rune Cairns (1.56m), and Mana Orbs in safe lanes.
4. `PowerUpManager.cs`: Implements Spirit Magnet, Bark Shield, and Wind Dash.
5. `CameraFollowShake.cs`: Smooth follow camera with harmonic decay screen-shake on impact.
6. `ProceduralForestEnvironment.cs`: Endless scrolling bioluminescent forest path and glowing trees.
7. `ProceduralAudio.cs`: Native real-time procedural synthesizer that generates jump tones, coin chimes, shield shatter, and defeat sounds with **ZERO external audio files**!
8. `GameUIManager.cs`: Renders the Score, Distance, Combo, Orbs, Buff Badges, and Game Over Screen automatically via Unity's native `OnGUI` (requires no complex Canvas or TextMeshPro setup).
9. `SylvanRushBootstrap.cs`: **The 1-Click Auto-Setup Script!** Automatically builds the player, camera, lighting, and environment the moment you press Play!

---

## 3. Step-by-Step: How to Run in Unity (Takes under 2 Minutes)

### Step 1: Create a Clean Unity Project
1. Open **Unity Hub**.
2. Click **New Project**.
3. Select **3D (Core)** or **3D (Built-in Render Pipeline / URP)**.
   * *Compatible with Unity 2021.3, 2022.3, 2023, or Unity 6 (6000).*
4. Name your project: `SylvanRush_3D` and click **Create Project**.

### Step 2: Delete Any Web Files from Unity
* **Make sure your Unity `Assets` folder is completely clean!**
* Do **NOT** copy `package.json`, `vite.config.ts`, `node_modules`, `src/`, or `.tsx` files into Unity!

### Step 3: Copy the C# Scripts into Unity
1. In your new Unity project, open the **Project** window at the bottom.
2. In the `Assets` folder, create a new folder named `Scripts`.
3. Copy all 9 `.cs` files from `/unity_scripts/` into `Assets/Scripts/`:
   * `PlayerController.cs`
   * `GameManager.cs`
   * `ObstacleSpawner.cs`
   * `PowerUpManager.cs`
   * `CameraFollowShake.cs`
   * `ProceduralForestEnvironment.cs`
   * `ProceduralAudio.cs`
   * `GameUIManager.cs`
   * `SylvanRushBootstrap.cs`

### Step 4: Run the Game!
1. In Unity, create an empty GameObject:
   * Menu: **GameObject -> Create Empty**.
   * Rename it to `GameBootstrapper`.
2. Select `GameBootstrapper` in the Hierarchy, then in the Inspector click **Add Component** and add:
   * `SylvanRushBootstrap`
3. Click the **▶ Play** button at the top of Unity!
4. **That's it!** `SylvanRushBootstrap` will automatically:
   * Build the glowing teal spirit runner avatar.
   * Set up the deep forest night lighting and atmospheric fog.
   * Position the follow camera with screen-shake.
   * Generate the scrolling forest path and glowing sideline trees.
   * Start the obstacle spawner, mana orbs, and procedural audio.

---

## 4. Controls in Unity Play Mode
* **A / Left Arrow**: Shift Lane Left
* **D / Right Arrow**: Shift Lane Right
* **W / Up Arrow / Space**: Vault Jump
* **Mouse / Touch Swipe**: Swipe left/right/up for mobile preview

---

## 5. Building the Game for Submission
To submit your game to your mentor or upload to itch.io:
1. In Unity, go to **File -> Build Settings**.
2. Select **PC, Mac & Linux Standalone** (or **WebGL** if you want to run it in a web browser).
3. Click **Build** and choose an output folder.
