#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// Creates the complete Composite Collider practice scene and assets.
/// It generates the scene automatically the first time this project opens.
/// </summary>
[InitializeOnLoad]
public static class CompositeColliderPracticeSetup
{
    private const string ScenePath = "Assets/Scenes/CompositeColliderPractice.unity";
    private const string TilePath = "Assets/Tiles/GroundTile.asset";
    private const string PrefabPath = "Assets/Prefabs/Ginger.prefab";
    private const string GroundSpritePath = "Assets/Art/GroundTile.png";
    private const string GingerSpritePath = "Assets/Art/Ginger.png";

    static CompositeColliderPracticeSetup()
    {
        EditorApplication.delayCall += GenerateSceneIfMissing;
    }

    [MenuItem("Tools/Week 3/Build Composite Collider Practice")]
    private static void BuildFromMenu()
    {
        if (File.Exists(ScenePath) &&
            !EditorUtility.DisplayDialog(
                "Rebuild Practice Scene",
                "This replaces the generated practice scene and Ginger prefab. Continue?",
                "Rebuild",
                "Cancel"))
        {
            return;
        }

        BuildPracticeScene();
    }

    private static void GenerateSceneIfMissing()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || File.Exists(ScenePath))
            return;

        try
        {
            BuildPracticeScene();
            Debug.Log("Composite Collider Practice scene and Ginger prefab were generated.");
        }
        catch (Exception exception)
        {
            Debug.LogError("Could not generate Composite Collider Practice assets. Use Tools > Week 3 > Build Composite Collider Practice after the Console errors are fixed.\n" + exception);
        }
    }

    private static void BuildPracticeScene()
    {
        EnsureFolder("Assets/Art");
        EnsureFolder("Assets/Tiles");
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Scenes");
        EnsureFolder("Assets/Scripts");

        Sprite groundSprite = CreateSprite(GroundSpritePath, 16, 16, CreateGroundPixels());
        Sprite gingerSprite = CreateSprite(GingerSpritePath, 16, 24, CreateGingerPixels());
        Tile groundTile = CreateOrUpdateGroundTile(groundSprite);
        GameObject gingerPrefab = CreateOrUpdateGingerPrefab(gingerSprite);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateGroundTilemap(groundTile);
        CreatePlayer(gingerPrefab);
        CreateCamera();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
    }

    private static void CreateGroundTilemap(Tile groundTile)
    {
        GameObject gridObject = new GameObject("Grid", typeof(Grid));
        Grid grid = gridObject.GetComponent<Grid>();
        grid.cellSize = Vector3.one;

        GameObject platformObject = new GameObject("Platforms", typeof(Tilemap), typeof(TilemapRenderer));
        platformObject.transform.SetParent(gridObject.transform, false);

        Tilemap tilemap = platformObject.GetComponent<Tilemap>();
        tilemap.tileAnchor = new Vector3(0.5f, 0.5f, 0f);

        TilemapRenderer tilemapRenderer = platformObject.GetComponent<TilemapRenderer>();
        tilemapRenderer.sortingOrder = 0;

        // A wide, two-tile-thick floor. Coordinates keep the top surface at y = -2.
        for (int x = -12; x <= 12; x++)
        {
            tilemap.SetTile(new Vector3Int(x, -4, 0), groundTile);
            tilemap.SetTile(new Vector3Int(x, -3, 0), groundTile);
        }

        // A small raised ledge so the jump input can be tested too.
        for (int x = 4; x <= 8; x++)
        {
            tilemap.SetTile(new Vector3Int(x, -1, 0), groundTile);
        }

        TilemapCollider2D tilemapCollider = platformObject.AddComponent<TilemapCollider2D>();
        CompositeCollider2D compositeCollider = platformObject.AddComponent<CompositeCollider2D>();
        Rigidbody2D groundBody = platformObject.GetComponent<Rigidbody2D>();

        if (groundBody == null)
            groundBody = platformObject.AddComponent<Rigidbody2D>();

        groundBody.bodyType = RigidbodyType2D.Static;
        groundBody.simulated = true;

        // Unity 6 uses Composite Operation = Merge instead of the legacy
        // "Used By Composite" checkbox.
        tilemapCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
        tilemapCollider.extrusionFactor = 0.01f;
        compositeCollider.geometryType = CompositeCollider2D.GeometryType.Outlines;
        compositeCollider.generationType = CompositeCollider2D.GenerationType.Synchronous;

        tilemap.RefreshAllTiles();
    }

    private static GameObject CreateOrUpdateGingerPrefab(Sprite gingerSprite)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        GameObject source;

        if (prefab == null)
        {
            source = new GameObject("Ginger");
            source.AddComponent<SpriteRenderer>();
            source.AddComponent<Rigidbody2D>();
            source.AddComponent<CapsuleCollider2D>();
            source.AddComponent<GingerPlatformerController>();
        }
        else
        {
            // Build a clean source so the required physics settings are deterministic.
            source = new GameObject("Ginger");
            source.AddComponent<SpriteRenderer>();
            source.AddComponent<Rigidbody2D>();
            source.AddComponent<CapsuleCollider2D>();
            source.AddComponent<GingerPlatformerController>();
        }

        SpriteRenderer spriteRenderer = source.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = gingerSprite;
        spriteRenderer.sortingOrder = 10;
        spriteRenderer.color = Color.white;

        Rigidbody2D body = source.GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = 1f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        CapsuleCollider2D capsule = source.GetComponent<CapsuleCollider2D>();
        capsule.direction = CapsuleDirection2D.Vertical;
        capsule.size = new Vector2(0.72f, 1.2f);
        capsule.offset = new Vector2(0f, -0.06f);

        PrefabUtility.SaveAsPrefabAsset(source, PrefabPath);
        UnityEngine.Object.DestroyImmediate(source);
        return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
    }

    private static void CreatePlayer(GameObject gingerPrefab)
    {
        GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(gingerPrefab);
        player.name = "Ginger";
        player.transform.position = new Vector3(0.5f, 1.2f, 0f);
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0.5f, -0.5f, -10f);

        Camera camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(34, 43, 61, 255);
    }

    private static Tile CreateOrUpdateGroundTile(Sprite sprite)
    {
        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(TilePath);
        if (tile == null)
        {
            tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = "GroundTile";
            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.Sprite;
            AssetDatabase.CreateAsset(tile, TilePath);
        }
        else
        {
            tile.sprite = sprite;
            tile.colliderType = Tile.ColliderType.Sprite;
            EditorUtility.SetDirty(tile);
        }

        return tile;
    }

    private static Sprite CreateSprite(string assetPath, int width, int height, Color32[] pixels)
    {
        if (!File.Exists(assetPath))
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(Path.GetFullPath(assetPath), texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
        }

        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 16f;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite == null)
            throw new InvalidOperationException("Unity did not import sprite asset: " + assetPath);

        return sprite;
    }

    private static Color32[] CreateGroundPixels()
    {
        const int width = 16;
        const int height = 16;
        Color32[] pixels = new Color32[width * height];
        Color32 grass = new Color32(83, 157, 69, 255);
        Color32 grassLight = new Color32(123, 190, 81, 255);
        Color32 dirt = new Color32(139, 88, 54, 255);
        Color32 dirtDark = new Color32(105, 62, 42, 255);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color32 color;
                if (y >= 13)
                {
                    color = ((x + y) % 5 == 0 || (x == 2 && y == 14)) ? grassLight : grass;
                }
                else
                {
                    color = ((x * 3 + y * 5) % 11 == 0) ? dirtDark : dirt;
                }

                pixels[y * width + x] = color;
            }
        }

        return pixels;
    }

    private static Color32[] CreateGingerPixels()
    {
        const int width = 16;
        const int height = 24;
        Color32[] pixels = new Color32[width * height];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color32(0, 0, 0, 0);

        Color32 hair = new Color32(82, 47, 35, 255);
        Color32 skin = new Color32(246, 187, 130, 255);
        Color32 shirt = new Color32(233, 133, 54, 255);
        Color32 shirtLight = new Color32(255, 177, 73, 255);
        Color32 trousers = new Color32(61, 83, 130, 255);
        Color32 shoes = new Color32(48, 40, 42, 255);
        Color32 eye = new Color32(38, 32, 31, 255);

        FillRect(pixels, width, height, 3, 1, 7, 3, shoes);
        FillRect(pixels, width, height, 9, 1, 13, 3, shoes);
        FillRect(pixels, width, height, 4, 3, 7, 8, trousers);
        FillRect(pixels, width, height, 9, 3, 12, 8, trousers);
        FillRect(pixels, width, height, 3, 8, 13, 14, shirt);
        FillRect(pixels, width, height, 5, 9, 8, 12, shirtLight);
        FillRect(pixels, width, height, 1, 8, 3, 13, shirt);
        FillRect(pixels, width, height, 13, 8, 15, 13, shirt);
        FillRect(pixels, width, height, 2, 7, 4, 9, skin);
        FillRect(pixels, width, height, 12, 7, 14, 9, skin);
        FillRect(pixels, width, height, 4, 14, 12, 21, skin);
        FillRect(pixels, width, height, 3, 19, 13, 23, hair);
        FillRect(pixels, width, height, 3, 17, 5, 21, hair);
        FillRect(pixels, width, height, 11, 17, 13, 21, hair);
        FillRect(pixels, width, height, 6, 17, 7, 18, eye);
        FillRect(pixels, width, height, 10, 17, 11, 18, eye);
        FillRect(pixels, width, height, 7, 14, 9, 15, new Color32(196, 101, 71, 255));

        return pixels;
    }

    private static void FillRect(Color32[] pixels, int width, int height, int xMin, int yMin, int xMaxExclusive, int yMaxExclusive, Color32 color)
    {
        for (int y = Mathf.Max(0, yMin); y < Mathf.Min(height, yMaxExclusive); y++)
        {
            for (int x = Mathf.Max(0, xMin); x < Mathf.Min(width, xMaxExclusive); x++)
                pixels[y * width + x] = color;
        }
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        int separator = path.LastIndexOf('/');
        string parent = path.Substring(0, separator);
        string folder = path.Substring(separator + 1);
        if (!AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(parent, folder);
    }
}
#endif
