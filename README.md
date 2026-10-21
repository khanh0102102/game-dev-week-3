# Game Dev Week 3 — Composite Collider 2D Practice

This is a Unity 2D practice project for **Unity 6.6 (6000.6.6f1)**.

## What is included

When Unity imports the project for the first time, the Editor setup script creates a playable practice scene and the required assets:

- `Assets/Scenes/CompositeColliderPractice.unity`
- `Assets/Prefabs/Ginger.prefab`
- `Assets/Art/GroundTile.png` and `Assets/Art/Ginger.png`
- A `Grid/Platforms` Tilemap with `TilemapCollider2D`, `CompositeCollider2D`, and a static `Rigidbody2D`
- A Ginger player with a vertical `CapsuleCollider2D`, dynamic `Rigidbody2D`, and frozen rotation
- Basic keyboard movement and jumping so the player can be tested against the floor

The scene generator is idempotent: it only auto-creates the practice scene if that scene does not exist. It also provides **Tools > Week 3 > Build Composite Collider Practice** to rebuild the generated scene intentionally.

## Open and test

1. Install/open Unity Editor **6000.6.6f1** through Unity Hub.
2. In Unity Hub, select **Add > Add project from disk** and select the root of this repository.
3. Wait for Package Manager to resolve the packages and for the Editor setup to generate the scene.
4. Open `Assets/Scenes/CompositeColliderPractice.unity`.
5. Press **Play**. Ginger should fall onto the Platforms Tilemap and remain standing on it.
6. Use **A/D** or **Left/Right Arrow** to move and **Space**, **W**, or **Up Arrow** to jump.
7. To inspect the merged collider outline, select `Grid/Platforms` and temporarily disable its **Tilemap Renderer** component. The collider outline is visible in the Scene view when **Gizmos** is enabled. Re-enable Tilemap Renderer after inspection.

## Physics setup checklist

### Practice 1 — Composite Collider

- [x] GameObject named `Platforms` with `Tilemap` and `TilemapRenderer`
- [x] `TilemapCollider2D`
- [x] `CompositeCollider2D` on the same GameObject
- [x] `Rigidbody2D` set to **Static**
- [x] Tilemap collider configured to merge its geometry into the composite collider using `Collider2D.compositeOperation = Merge`

Unity 6 uses the **Composite Operation** setting rather than the old **Used By Composite** checkbox. Setting it to **Merge** is the equivalent modern configuration.

### Practice 2 — Player physics

- [x] Prefab named `Ginger`
- [x] `CapsuleCollider2D`, vertically oriented and sized to fit the character
- [x] `Rigidbody2D` set to **Dynamic**
- [x] `Rigidbody2D` rotation frozen

## Git

Unity-generated directories such as `Library/`, `Temp/`, and `Logs/` are excluded. After Unity generates the scene, prefab, textures and their `.meta` files, commit these generated project assets to Git so the remote repository contains the ready-to-open scene as well.
