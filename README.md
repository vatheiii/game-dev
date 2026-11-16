# Kitty Surfer 

A colorful, cartoon-style 3D endless runner for Windows PC. You play a cat who is lost in a forest and has to fend for itself to survive in the wilderness. Run through the endless forest, avoid obstacles, collect food and power-ups, and travel as far as you can to set a high score.

Built with **Unity 6.3 LTS (6000.3.8f1)** and **C#**.

- **Gameplay video:** [Game play recording](https://drive.google.com/file/d/1sC2UHsbvB1QfB5_loq1LleETfbu_1E18/view?usp=sharing)
- **Playable build (Windows):** [Download Kitty Surfer for Windows (64-bit)](https://github.com/vatheiii/game-dev/releases/download/kitty-surfer-v0.1.0/KittySurfer-Windows-v0.1.0.zip)
- **Playable build (macOS):** [Download Kitty Surfer for Mac (Apple silicon and Intel)](https://github.com/vatheiii/game-dev/releases/download/kitty-surfer-v0.1.0/KittySurfer-macOS-v0.1.0.zip)

---

## Objective

Survive in the forest and run as far as you can. Your score goes up the further you travel, so avoid obstacles and bombs, collect fish and shields, and keep your lives. You start with **3 lives**; the run ends when you lose all of them, then you can replay to beat your best score, which is saved on your PC.

## Rules

- **Score:** 1 point for every 10 units of forward distance. Jumping and changing lanes do not earn points.
- **Lives:** You start with 3. Hitting a log, boulder, moving log or bomb costs 1 life.
- **Recovery time:** After you take a hit, you are briefly protected (about 1 second) so one collision cannot drain all your lives.
- **Game Over:** At 0 lives the run ends and the score freezes. Press **Replay** to try again.
- **Best score:** Saved locally and shown on the HUD. It only updates when a finished run beats it.
- **Difficulty:** The cat speeds up the further it runs, and track layouts get tougher at around 250 and 600 units. Layouts are randomized, so each run is different.

---

## How to Play (Quick Tutorial)

1. **Start:** Click **Play** on the start screen. Your cat starts running forward on its own.
2. **Move:** Press `A` / `Left Arrow` to go left and `D` / `Right Arrow` to go right. There are 3 lanes.
3. **Jump:** Press `Space` to jump over low obstacles like logs. You cannot jump while already in the air.
4. **Dodge:** Boulders are too tall to jump, so switch lanes. Moving logs slide across lanes, so time your move.
5. **Collect:** Run through fish for double score for 5 seconds. Run through gems for a shield that blocks your next hit.
6. **Avoid bombs:** Touching a bomb costs 1 life, unless your shield blocks it.
7. **Survive:** You have 3 lives. After a hit you are protected for about 1 second. When you reach 0 lives the run ends.
8. **Replay:** Click **Replay** to try again and beat your best score.

**Tip:** Your score only goes up with forward distance, so staying alive matters more than chasing every item.

---

### Items

| Item | Effect |
|------|--------|
| Fish | Score x2 for 5 seconds. Picking up another fish restarts the timer (it does not stack). The HUD shows the active multiplier. |
| Shield (gem) | Blocks the next hit within 5 seconds. The shield is used up when it blocks. The HUD shows the remaining time. |
| Bomb | Hazard. Touching it costs 1 life unless the shield blocks it. |

### Obstacles

| Obstacle | Behavior |
|----------|----------|
| Log | Low. Jump over it or switch lanes. |
| Boulder | Tall. Switch lanes (it cannot be jumped). |
| Moving log | Slides across lanes, so time your move or jump. |

## Controls

| Action | Keys |
|--------|------|
| Move left | `A` or `Left Arrow` |
| Move right | `D` or `Right Arrow` |
| Jump | `Space` |
| Play / Replay / Quit | Mouse click on the on-screen buttons |

The cat runs forward automatically across 3 lanes.

## Unity Version and Packages

- Unity **6.3 LTS (6000.3.8f1)**
- Universal Render Pipeline (URP) 17.3.0
- Input System 1.18.0
- Target platform: Windows PC

## Setup and Play Instructions

### Option A: Play the build

1. Download the ZIP for your platform from the top of this README.
2. Extract the entire ZIP.
3. On Windows, run `KittySurfer.exe`. On macOS, open `KittySurfer.app` in Finder.
4. Click **Play**.

### Option B: Run from source in Unity

1. Install **Unity Hub** and Unity **6000.3.8f1** (add the *Windows Build Support* module if prompted).
2. Clone or download this repository:
   ```
   git clone [ADD REPO URL HERE]
   ```
3. In Unity Hub, click **Add > Add project from disk** and select the **`kitty surfer`** folder (the Unity project is inside the repository folder, not the repository root).
4. Open the project and wait for Unity to import assets.
5. Open the scene `Assets/Scenes/KittySurfer.unity`.
6. Press the **Play** button in the Unity Editor and click **Play** on the start screen.

### Build for Windows

1. Go to **File > Build Profiles** and select **Windows**.
2. Make sure `Scenes/KittySurfer` is enabled in the scene list.
3. Click **Build** and choose an output folder.

## Project Structure

```
kitty surfer/Assets/
  Scenes/KittySurfer.unity     Main game scene
  Scripts/
    PlayerController.cs        Lane movement, jump, speed ramp-up, camera follow
    GameManager.cs             Score, best score, lives, power-ups, game over, replay, quit
    TrackSpawner.cs            Endless track: recycles sections and applies layouts
    TrackLayouts.cs            Obstacle layouts by difficulty
    ObstacleMover.cs           Moving log behavior
    UIManager.cs               HUD, start screen, game over screen
  Prefabs/                     Logs, shield, start/game over displays, forest scenery
```

## Team Members and Contributions

### Rath Socheata: Scoring, Power-ups and Damage Rules

- **Distance scoring** (`GameManager.cs`): stores the cat's starting Z position and awards 1 point per 10 units of forward distance. Jumps and lane changes earn nothing because only forward distance counts. The score freezes at Game Over and resets on Replay.
- **Best Score:** saved with `PlayerPrefs` only when a finished run beats the old record, and shown on the HUD below the current score.
- **Fish multiplier:** created the `Fish` tag and added the fish as a trigger on the track section. A fish gives score x2 for 5 seconds, a second fish restarts the timer without stacking, and the HUD shows the active multiplier. `TrackSpawner` re-activates eaten fish when a section is recycled.
- **Shield:** created the `Shield` tag and added the gem as a trigger on the track section. The shield protects the player, and the HUD shows its remaining time.
- **Bomb and shared damage rule:** created the `Bomb` tag and added the bomb prefab as a trigger. Moved all life-loss rules into one method, `TakeHit()`: the shield blocks hits, a short invincibility period follows an unblocked hit, and zero lives ends the run. Both `OnCollisionEnter` (obstacles) and `OnTriggerEnter` (bomb) call `TakeHit()`, and `TrackSpawner` re-activates bombs on recycle.
- **Quit button:** added `QuitGame()`, which stops Play Mode in the Editor and calls `Application.Quit()` in a build, and connected it to the `quit_button` On Click list.

### Taing Sothyvan: Core Gameplay, Endless Track and Difficulty

- **Player controls** (`PlayerController.cs`): three-lane movement with `A`/`D` or the arrow keys, with the cat sliding smoothly to the target lane, plus jumping with `Space`. Jumping uses a ground check so the cat cannot jump in mid-air, and the camera follows the cat down the track.
- **Collision:** set up the Rigidbody and colliders so obstacles are detected through `OnCollisionEnter`, and the cat can pass an obstacle after contact.
- **Endless track** (`TrackSpawner.cs`): the level is built from a small set of track sections that are recycled as the player moves forward, so the run continues in one scene with no loading.
- **Randomized layouts** (`TrackLayouts.cs`): obstacle layouts are chosen at random and varied between runs. The same layout will not repeat more than twice in a row, and the safe lane changes from one run to the next.
- **Difficulty progression:** the cat's speed rises gradually with distance, and harder layouts unlock at about 250 and 600 units.
- **Moving obstacle** (`ObstacleMover.cs`): a log that slides across lanes using a kinematic Rigidbody moved in `FixedUpdate`.
- **Hit feedback:** the cat flashes red when hit and a shield bubble fades out during the recovery period.
- **Layout and background:** improved the scene layout and background, and merged the forest scenery branch (pull request #1) into `main`.

### Touch Daravatey: Road, Rock, Fish and Log Setup, Forest Environment

- **Road (track):** set up the road and ground prefab that each track section is built from, with the road texture and ground materials, so sections line up and can be recycled into an endless trail.
- **Rock (boulder):** set up the boulder obstacle prefab, with left and right lane versions, so it can be placed in any lane by the track layouts.
- **Fish:** set up the fish pickup prefab, with left, middle and right lane versions, using the fish model so the pickup can appear in any lane.
- **Log:** set up the log obstacle in the track section so it works with the track layouts and collisions.
- **Relay Screen:** created the relay screen display with a Play button. The game waits on this screen until the player is ready, and the HUD appears once the run begins.
- **Forest scenery:** designed the forest background for the running trail (the `design/forest-background` pull request), using the Supercyan forest pack for the colorful cartoon-forest look.
- **Project setup:** helped prepare the Unity project in the repository, including the `.gitignore` and the project rename to Kitty Surfer.

### Nhep Thavyrath: Obstacle and Shield Prefabs, Start Screen and Score UI

- **Log prefab:** built the log obstacle with its bark and end-grain materials, with left and right lane variants.
- **Shield prefab:** built the shield pickup, with variants for the left, middle and right lanes.
- **Start screen:** created the start screen display with a Play button. The game waits on this screen until the player is ready, and the HUD appears once the run begins.
- **Quit screen:** created the quit screen display. The game waits on this screen until the player is ready, and the HUD appears once the run begins.
- **Score text:** added the score text to the HUD, which `UIManager` updates as the cat runs.

## Assets and References

### Assets

- **Cartoon Dog, Cat** by PolyOne: cat model, materials, and run/idle animations.
- **Supercyan Free Forest Sample** by Supercyan: forest scenery, trees, rocks and shaders (the bomb, rock and fish prefab variants were made from this pack).
- **[Low Poly Fish](https://assetstore.unity.com/packages/3d/characters/animals/fish/low-poly-fish-339618)** by Floreswa: fish pickup model.
- **Simple Gems and Items Ultimate Animated Customizable Pack** by Bentlman1981 (BTM): gem used for the shield power-up.
- **TextMesh Pro** (Unity package), with the Liberation Sans font (SIL Open Font License).
- **Unity Universal Render Pipeline** template.



