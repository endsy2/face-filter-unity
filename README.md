# 🎭 FaceFilter — AR Face Filter App

A mobile **Augmented Reality face filter** application built with Unity and AR Foundation.  
Point your camera at a face and apply real-time 3D props, particle effects, and face texture overlays — similar to Instagram or Snapchat filters.

> **Platform:** Android · iOS  
> **Engine:** Unity (URP)  
> **Assignment:** Year 4 Game Development

---

## 📱 Features

- **Real-time face tracking** via ARCore (Android) and ARKit (iOS)
- **3D prop overlays** — hats, ears, horns, teeth, moustaches, and more
- **Particle effects** — fire, rain, dizzy stars, magic eyes, steam
- **Face texture overlays** — UV-mapped makeup, geometric patterns, leaves, waves
- **Dynamic filter switching** at runtime via a clean UI
- **Modular filter system** — add a new filter by creating a single ScriptableObject asset

---

## 🛠️ Tech Stack

| Component | Package / Version |
|---|---|
| Engine | Unity (Universal Render Pipeline 17.3.0) |
| AR Framework | AR Foundation `6.3.3` |
| Android AR | ARCore XR Plugin `6.3.3` |
| iOS AR | ARKit XR Plugin `6.3.3` |
| Interaction | XR Interaction Toolkit `3.3.1` |
| Input | Unity Input System `1.18.0` |

---

## 📂 Project Structure

```
Assets/
├── FaceFilter/                     ← All custom project content
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── FaceFilterManager.cs       ← Singleton; owns filter list & active state
│   │   │   └── FaceFilterController.cs    ← On AR face prefab; spawns props & VFX
│   │   ├── Data/
│   │   │   ├── FaceFilterDefinition.cs    ← ScriptableObject; defines one filter
│   │   │   └── FaceAnchorPoint.cs         ← Enum for named face regions
│   │   └── UI/
│   │       └── FilterSelectionUI.cs       ← Prev/Next buttons + filter label
│   │
│   ├── Filters/                    ← One .asset file per filter
│   │   ├── Filter_None.asset
│   │   ├── Filter_Cat.asset
│   │   ├── Filter_Goblin.asset
│   │   ├── Filter_HiHat.asset
│   │   ├── Filter_Fire.asset
│   │   └── Filter_MakeUp_Waves.asset
│   │
│   ├── Prefabs/
│   │   ├── Core/
│   │   │   └── AR Default Face.prefab     ← Root face tracking prefab (ARFace + anchors)
│   │   ├── Props/                         ← Individual 3D prop prefabs
│   │   │   ├── Prop_Cap.prefab
│   │   │   ├── Prop_CatEars.prefab
│   │   │   ├── Prop_CatNose.prefab
│   │   │   ├── Prop_CrazyEyes.prefab
│   │   │   ├── Prop_GoblinEars.prefab
│   │   │   ├── Prop_GrinningTeeth.prefab
│   │   │   ├── Prop_HiHat.prefab
│   │   │   ├── Prop_Horns.prefab
│   │   │   └── Prop_Moustache.prefab
│   │   └── VFX/                           ← Particle effect prefabs
│   │       ├── VFX_Dizzy.prefab
│   │       ├── VFX_Fire.prefab
│   │       ├── VFX_MagicEyes.prefab
│   │       ├── VFX_Rain.prefab
│   │       └── VFX_Steam.prefab
│   │
│   ├── Materials/
│   │   ├── FaceOverlays/                  ← Materials for ARFace mesh overlays
│   │   └── Props/                         ← PBR materials for 3D props & particles
│   │
│   ├── Meshes/                            ← FBX source files for each prop
│   ├── Textures/
│   │   ├── FaceOverlays/                  ← UV-mapped face texture PNGs
│   │   ├── Particles/                     ← Flipbook textures for particle systems
│   │   └── Props/                         ← PBR texture maps for props
│   └── Animations/
│       └── (per-prop subfolders)          ← .anim clip + Animator Controller per prop
│
├── Scenes/
│   ├── Basic_Face_Filter.unity            ← ★ Primary scene
│   ├── ARScene.unity                      ← AR plane detection demo
│   └── SampleScene.unity                  ← Default Unity sample
│
├── MobileARTemplateAssets/                ← Unity Mobile AR Template (unmodified)
└── Samples/XR Interaction Toolkit/        ← XRI Starter & AR Starter Assets
```

---

## 🏗️ Architecture Overview

The project uses a **data-driven, event-based** filter system:

```
FaceFilterManager  (singleton)
    │  holds List<FaceFilterDefinition>
    │  fires onFilterChanged event
    ▼
FaceFilterController  (on AR Default Face prefab)
    │  listens to onFilterChanged
    │  spawns / clears props + VFX at named face anchors
    │  swaps the face overlay material
    ▼
FaceFilterDefinition  (ScriptableObject asset)
    │  pure data — prop prefabs, VFX prefabs, face material
    │  one .asset file per filter, no code required
    ▼
FilterSelectionUI  (Canvas)
    │  Prev / Next buttons
    │  calls FaceFilterManager.NextFilter() / PreviousFilter()
```

### Face Anchor Points

The `AR Default Face` prefab contains a set of named empty GameObjects (`Anchors/`) that mark common face regions. Each anchor's local position is relative to the face root (nose bridge):

| Anchor | Approximate Use |
|---|---|
| `Anchor_FaceCenter` | Default fallback, nose bridge |
| `Anchor_Forehead` | Horns, hats, headbands |
| `Anchor_LeftEye` / `Anchor_RightEye` | Eye props, magic VFX |
| `Anchor_Nose` | Cat nose, clown nose |
| `Anchor_Mouth` | Teeth, moustache |
| `Anchor_LeftEar` / `Anchor_RightEar` | Ear props |
| `Anchor_Chin` | Beard props |

### Runtime Flow — Switching Filters

```
User taps "Next" button
  → FilterSelectionUI → FaceFilterManager.NextFilter()
  → onFilterChanged.Invoke(newFilter)
  → FaceFilterController.ApplyFilter(newFilter)   [on every active tracked face]
      → destroy old props & VFX
      → swap face overlay material on ARFace mesh
      → instantiate new props at their anchor points
      → instantiate new VFX at their anchor points
  → FilterSelectionUI updates name label & thumbnail
```

---

## 🚀 Getting Started

### Prerequisites

- **Unity** — version compatible with the packages in `Packages/manifest.json`
  - Install via [Unity Hub](https://unity.com/download)
  - Include **Android Build Support** and/or **iOS Build Support** modules
- **Android:** USB debugging enabled on device; ARCore-capable device (Android 7.0+)
- **iOS:** Xcode installed; ARKit-capable device (A9 chip or later, iOS 13+)
- **VS Code** (optional) — install the [Unity VS Tools extension](https://marketplace.visualstudio.com/items?itemName=visualstudiotoolsforunity.vstuc) for debugging

### Opening the Project

1. Clone or download this repository
2. Open **Unity Hub → Add → Add project from disk** → select the repo root
3. Let Unity import assets and compile scripts (first open takes a few minutes)
4. Open `Assets/Scenes/Basic_Face_Filter.unity`

> ⚠️ **AR face tracking cannot run in the Unity Editor.** You must build and deploy to a physical device to test face tracking.

---

## 🔨 Building & Deploying

### Android

1. **File → Build Settings**
2. Select **Android**, click **Switch Platform**
3. **Player Settings → Other Settings:**
   - Minimum API Level: Android 7.0 (API 24)
   - Enable **Custom Main Manifest** if needed for ARCore permissions
4. Connect device via USB with USB debugging enabled
5. Click **Build and Run**

### iOS

1. **File → Build Settings**
2. Select **iOS**, click **Switch Platform**
3. **Player Settings → Other Settings:**
   - Camera Usage Description: `"Required for AR face tracking"`
   - Target minimum iOS Version: 13.0
4. Click **Build** → open the generated Xcode project
5. Set your Team in Xcode → **Product → Run**

---

## ✨ Adding a New Filter

No code required — create a **ScriptableObject asset**:

1. In the Project window, navigate to `Assets/FaceFilter/Filters/`
2. Right-click → **Create → FaceFilter → Filter Definition**
3. Name it `Filter_YourName`
4. Fill in the Inspector:

| Field | What to set |
|---|---|
| **Filter Name** | Display name shown in UI |
| **Thumbnail** | Small icon sprite for the UI carousel |
| **Face Overlay Material** | Material for the ARFace mesh (or `None` for no overlay) |
| **Props** | Add entries — assign prefab from `Prefabs/Props/`, choose anchor, tweak offsets |
| **VFX** | Add entries — assign prefab from `Prefabs/VFX/`, choose anchor |

5. Open the scene, select the `Managers` GameObject, and add your new asset to `FaceFilterManager → Available Filters`

### Adding a New Prop Mesh

1. Export an FBX from your 3D tool and drop it into `FaceFilter/Meshes/`
2. Create a prefab in `FaceFilter/Prefabs/Props/` — add MeshRenderer + Animator
3. Create an Animator Controller in `FaceFilter/Animations/YourProp/`
4. Reference the new prefab in a `FaceFilterDefinition` asset

### Adding a New Face Texture Overlay

1. Paint your texture using `Textures/FaceOverlays/canonical_face_texture.png` as UV reference
2. Save your PNG to `FaceFilter/Textures/FaceOverlays/`
3. Create a Material in `FaceFilter/Materials/FaceOverlays/` using the face overlay shader
4. Assign the material to a `FaceFilterDefinition`'s **Face Overlay Material** field

---

## 🎛️ Available Filters

| Filter | Props | VFX | Face Overlay |
|---|---|---|---|
| **None** | — | — | — |
| **Cat** | Cat Ears, Cat Nose | — | Optional makeup |
| **Goblin** | Goblin Ears, Horns | — | — |
| **HiHat** | Hi-Hat | — | — |
| **Fire** | — | Fire VFX | — |
| **Wave Makeup** | — | — | Wave texture |
| **Dizzy** | — | Dizzy stars | — |
| **Rain** | — | Rain VFX | — |
| *(more...)* | | | |

---

## 🔧 VS Code Debugger

Attach the VS Code debugger to a running Unity Editor session:

1. Open the project in VS Code
2. Press **F5** (or **Run → Start Debugging**)
3. Select **"Attach to Unity"** configuration (defined in `.vscode/launch.json`)
4. This requires the [Unity VS Tools](https://marketplace.visualstudio.com/items?itemName=visualstudiotoolsforunity.vstuc) extension

---

## 📋 Key Files Reference

| File | Purpose |
|---|---|
| `Assets/FaceFilter/Scripts/Core/FaceFilterManager.cs` | Singleton; manages filter list and active selection |
| `Assets/FaceFilter/Scripts/Core/FaceFilterController.cs` | Per-face component; applies props/VFX/overlay |
| `Assets/FaceFilter/Scripts/Data/FaceFilterDefinition.cs` | ScriptableObject definition for one filter |
| `Assets/FaceFilter/Scripts/Data/FaceAnchorPoint.cs` | Enum for named face regions |
| `Assets/FaceFilter/Scripts/UI/FilterSelectionUI.cs` | Prev/Next UI controller |
| `Assets/FaceFilter/Prefabs/Core/AR Default Face.prefab` | Root face tracking prefab (ARFace + anchors) |
| `Assets/Scenes/Basic_Face_Filter.unity` | Primary app scene |
| `Packages/manifest.json` | All Unity package dependencies and versions |

---

## ⚡ Performance Notes

- AR rendering is GPU-heavy — keep particle counts **below 50 particles per system**
- Face overlay materials should use **Unlit shaders** (no lighting calculations on the face mesh)
- Texture overlays should be compressed: **ASTC 6×6** for Android, **PVRTC** for iOS
- Filter switching uses Instantiate/Destroy (acceptable — it's user-triggered, not per-frame)

---

## 📁 Ignored by Git

```
Temp/           ← Unity build temporaries
Library/        ← Unity's local asset cache (auto-regenerated)
Logs/           ← Editor logs
UserSettings/   ← Per-developer Editor preferences
```

> After cloning, open the project in Unity Hub and wait for the Library to be rebuilt automatically.

---

*Built with Unity AR Foundation · Year 4 Game Development Assignment*
