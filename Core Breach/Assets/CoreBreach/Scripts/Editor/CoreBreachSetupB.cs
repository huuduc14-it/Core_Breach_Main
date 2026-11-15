using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

/// <summary>
/// Công cụ Editor cho Thành viên B: dựng nhanh HUD/Pause/Main Menu bằng Canvas,
/// tạo RoomData, biến map đã vẽ thành Room prefab. Menu: Core Breach > Member B.
/// Nếu công cụ gây lỗi biên dịch, có thể xóa nguyên thư mục Scripts/Editor mà không ảnh hưởng game.
/// </summary>
public static class CoreBreachSetupB
{
    private const string Root = "Assets/CoreBreach";
    private const string ScenesDir = Root + "/Scenes";
    private const string RoomPrefabDir = Root + "/Prefabs/Rooms";
    private const string RoomDataDir = Root + "/ScriptableObjects";
    private const string MenuSceneName = "CoreBreach_MainMenu";
    private const string GameSceneName = "CoreBreach_Main";

    // ------------------------------------------------------------------
    // 1. HUD + Pause Canvas
    // ------------------------------------------------------------------
    [MenuItem("Core Breach/Member B/1. Create HUD + Pause Canvas in this scene")]
    public static void CreateHudCanvas()
    {
        if (GameObject.Find("HUD_Canvas") != null)
        {
            EditorUtility.DisplayDialog("HUD", "Scene này đã có HUD_Canvas. Hãy xóa nó trước nếu muốn tạo lại.", "OK");
            return;
        }

        Canvas canvas = CreateCanvas("HUD_Canvas");
        EnsureEventSystem();
        Transform hud = canvas.transform;

        // Góc trên trái: máu, giáp, nhiệt
        Slider hp = MakeBar(hud, "HealthBar", new Vector2(30f, -30f), new Vector2(360f, 26f), new Color(0.9f, 0.2f, 0.25f));
        Text hpText = MakeText(hud, "HealthText", "HP 100/100", 18, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(405f, -30f), new Vector2(260f, 26f), Color.white);

        Slider armor = MakeBar(hud, "ArmorBar", new Vector2(30f, -64f), new Vector2(360f, 18f), new Color(0.3f, 0.6f, 1f));
        Text armorText = MakeText(hud, "ArmorText", "ARMOR 0/0", 16, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(405f, -64f), new Vector2(260f, 18f), Color.white);

        Slider heat = MakeBar(hud, "HeatBar", new Vector2(30f, -92f), new Vector2(360f, 18f), new Color(0.2f, 0.8f, 1f));
        Text heatWarn = MakeText(hud, "HeatWarning", "OVERHEAT!", 20, TextAnchor.MiddleLeft, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(405f, -92f), new Vector2(260f, 22f), new Color(1f, 0.25f, 0.1f));
        heatWarn.gameObject.SetActive(false);

        // Góc trên phải: Scrap, Depth, Score
        Text scrap = MakeText(hud, "ScrapText", "SCRAP: 0", 26, TextAnchor.UpperRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -30f), new Vector2(360f, 32f), new Color(1f, 0.85f, 0.3f));
        Text depth = MakeText(hud, "DepthText", "DEPTH: 1", 26, TextAnchor.UpperRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -64f), new Vector2(360f, 32f), Color.white);
        Text score = MakeText(hud, "ScoreText", "SCORE: 0", 26, TextAnchor.UpperRight, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -98f), new Vector2(360f, 32f), Color.white);

        // Dùng lại PlayerHUD đã có trong scene (scene Main đã có sẵn), nếu chưa có thì thêm vào Canvas
        PlayerHUD playerHud = Object.FindFirstObjectByType<PlayerHUD>();
        if (playerHud == null) playerHud = canvas.gameObject.AddComponent<PlayerHUD>();
        SerializedObject hudSo = new SerializedObject(playerHud);
        SetRef(hudSo, "healthSlider", hp);
        SetRef(hudSo, "armorSlider", armor);
        SetRef(hudSo, "healthText", hpText);
        SetRef(hudSo, "armorText", armorText);
        SetRef(hudSo, "heatSlider", heat);
        SetRef(hudSo, "heatFillImage", heat.fillRect.GetComponent<Image>());
        SetRef(hudSo, "heatWarningText", heatWarn);
        SetRef(hudSo, "scrapText", scrap);
        SetRef(hudSo, "depthText", depth);
        SetRef(hudSo, "scoreText", score);
        hudSo.ApplyModifiedProperties();

        // Pause panel
        GameObject panel = new GameObject("PausePanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(hud, false);
        RectTransform prt = panel.GetComponent<RectTransform>();
        prt.anchorMin = Vector2.zero;
        prt.anchorMax = Vector2.one;
        prt.offsetMin = Vector2.zero;
        prt.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

        Vector2 mid = new Vector2(0.5f, 0.5f);
        MakeText(panel.transform, "PauseTitle", "PAUSED", 56, TextAnchor.MiddleCenter, mid, mid, new Vector2(0f, 150f), new Vector2(600f, 80f), Color.white);
        Button resume = MakeButton(panel.transform, "RESUME", new Vector2(0f, 40f));
        Button restart = MakeButton(panel.transform, "RESTART RUN", new Vector2(0f, -30f));
        Button menu = MakeButton(panel.transform, "MAIN MENU", new Vector2(0f, -100f));

        PauseMenu pause = Object.FindFirstObjectByType<PauseMenu>();
        if (pause == null) pause = canvas.gameObject.AddComponent<PauseMenu>();
        SerializedObject pauseSo = new SerializedObject(pause);
        pauseSo.FindProperty("pauseMenuPanel").objectReferenceValue = panel;
        pauseSo.ApplyModifiedProperties();

        UnityEventTools.AddPersistentListener(resume.onClick, pause.ResumeGame);
        UnityEventTools.AddPersistentListener(restart.onClick, pause.RestartRun);
        UnityEventTools.AddPersistentListener(menu.onClick, pause.ReturnToMainMenu);

        panel.SetActive(false);

        FinishScene(canvas.gameObject);
        Debug.Log("[CoreBreachSetupB] Đã tạo HUD_Canvas (HUD + PausePanel) và nối vào PlayerHUD / PauseMenu.");
    }

    // ------------------------------------------------------------------
    // 2. Main Menu scene
    // ------------------------------------------------------------------
    [MenuItem("Core Breach/Member B/2. Create Main Menu scene")]
    public static void CreateMainMenuScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camGo.tag = "MainCamera";
        Camera cam = camGo.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.07f, 0.08f, 0.13f, 1f);
        cam.orthographic = true;

        Canvas canvas = CreateCanvas("MainMenu_Canvas");
        EnsureEventSystem();
        Transform t = canvas.transform;
        Vector2 mid = new Vector2(0.5f, 0.5f);

        MakeText(t, "Title", "CORE BREACH", 110, TextAnchor.MiddleCenter, mid, mid, new Vector2(0f, 250f), new Vector2(1400f, 140f), new Color(0.4f, 0.85f, 1f));
        MakeText(t, "Subtitle", "Top-down Twin-stick Shooter Roguelite", 30, TextAnchor.MiddleCenter, mid, mid, new Vector2(0f, 160f), new Vector2(1000f, 40f), new Color(0.8f, 0.8f, 0.85f));
        Text high = MakeText(t, "HighScore", "HIGH SCORE: 0", 30, TextAnchor.MiddleCenter, mid, mid, new Vector2(0f, 70f), new Vector2(700f, 40f), new Color(1f, 0.85f, 0.3f));
        Text scrap = MakeText(t, "TotalScrap", "TOTAL SCRAP: 0", 26, TextAnchor.MiddleCenter, mid, mid, new Vector2(0f, 30f), new Vector2(700f, 36f), Color.white);
        Button play = MakeButton(t, "PLAY", new Vector2(0f, -60f));
        Button quit = MakeButton(t, "QUIT", new Vector2(0f, -130f));

        MainMenu mm = canvas.gameObject.AddComponent<MainMenu>();
        SerializedObject so = new SerializedObject(mm);
        so.FindProperty("highScoreText").objectReferenceValue = high;
        so.FindProperty("totalScrapText").objectReferenceValue = scrap;
        so.FindProperty("gameSceneName").stringValue = GameSceneName;
        so.ApplyModifiedProperties();

        UnityEventTools.AddPersistentListener(play.onClick, mm.PlayGame);
        UnityEventTools.AddPersistentListener(quit.onClick, mm.QuitGame);

        string path = ScenesDir + "/" + MenuSceneName + ".unity";
        EditorSceneManager.SaveScene(scene, path);

        AddScenesToBuildSettings();
        Debug.Log("[CoreBreachSetupB] Đã tạo " + path + " và thêm vào Build Settings.");
    }

    // ------------------------------------------------------------------
    // 3. RoomData từ các Room prefab + nối vào RoomManager
    // ------------------------------------------------------------------
    [MenuItem("Core Breach/Member B/3. Create RoomData assets and wire RoomManager")]
    public static void CreateRoomData()
    {
        if (!AssetDatabase.IsValidFolder(RoomPrefabDir))
        {
            EditorUtility.DisplayDialog("RoomData", "Không thấy thư mục " + RoomPrefabDir, "OK");
            return;
        }

        EnsureFolder(RoomDataDir);

        List<RoomData> datas = new List<RoomData>();
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { RoomPrefabDir });

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null || prefab.GetComponent<RoomController>() == null) continue;

            string name = Path.GetFileNameWithoutExtension(path);
            string assetPath = RoomDataDir + "/RoomData_" + name + ".asset";

            RoomData rd = AssetDatabase.LoadAssetAtPath<RoomData>(assetPath);
            if (rd == null)
            {
                rd = ScriptableObject.CreateInstance<RoomData>();
                rd.roomName = name;
                rd.roomPrefab = prefab;
                rd.roomType = name.ToLower().Contains("boss") ? RoomType.Boss : RoomType.Combat;
                rd.weight = 10;
                AssetDatabase.CreateAsset(rd, assetPath);
            }

            datas.Add(rd);
        }

        AssetDatabase.SaveAssets();

        RoomManager rm = Object.FindFirstObjectByType<RoomManager>();
        if (rm == null)
        {
            EditorUtility.DisplayDialog("RoomData",
                "Đã tạo " + datas.Count + " RoomData trong " + RoomDataDir + ".\nScene hiện tại chưa có RoomManager nên chưa nối tự động.", "OK");
            return;
        }

        List<RoomData> normal = datas.FindAll(d => d.roomType != RoomType.Boss);
        RoomData boss = datas.Find(d => d.roomType == RoomType.Boss);

        Undo.RecordObject(rm, "Wire RoomManager");
        rm.normalRooms = normal.ToArray();
        rm.bossRoom = boss;
        EditorUtility.SetDirty(rm);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        Debug.Log("[CoreBreachSetupB] RoomManager: " + normal.Count + " phòng thường, Boss = " + (boss != null ? boss.roomName : "(chưa có)"));
    }

    // ------------------------------------------------------------------
    // 4. Dựng Room prefab từ object đang chọn trong Hierarchy
    // ------------------------------------------------------------------
    [MenuItem("Core Breach/Member B/4. Build Room prefab from selected object")]
    public static void BuildRoomPrefab()
    {
        GameObject root = Selection.activeGameObject;
        if (root == null || EditorUtility.IsPersistent(root))
        {
            EditorUtility.DisplayDialog("Room prefab",
                "Hãy chọn object gốc của phòng TRONG HIERARCHY (ví dụ Room1_Root, chứa các Tilemap) rồi chạy lại.\nNên đổi tên thành RoomCombat_02 / RoomCombat_03 trước.", "OK");
            return;
        }

        // 1. Collider cho các Tilemap tường/vật thể
        int colliderCount = 0;
        foreach (Tilemap tm in root.GetComponentsInChildren<Tilemap>(true))
        {
            string n = tm.name.ToLower();
            if (n.Contains("wall") || n.Contains("object"))
            {
                AddWallCollider(tm.gameObject);
                colliderCount++;
            }
        }

        // 2. Kích thước phòng từ các TilemapRenderer
        TilemapRenderer[] renderers = root.GetComponentsInChildren<TilemapRenderer>(true);
        if (renderers.Length == 0)
        {
            EditorUtility.DisplayDialog("Room prefab", "Không tìm thấy Tilemap nào trong object đã chọn.", "OK");
            return;
        }

        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);

        Vector3 c = b.center;
        float margin = 3f;

        // 3. Spawn point / Exit (chỉ tạo nếu chưa có). VỊ TRÍ LÀ GỢI Ý: hãy kéo lại cho đúng sàn trong Scene view.
        Transform playerSpawn = GetOrCreateChild(root.transform, "PlayerSpawn", new Vector3(c.x, b.min.y + margin, 0f));

        Transform enemyRoot = root.transform.Find("EnemySpawns");
        if (enemyRoot == null)
        {
            enemyRoot = GetOrCreateChild(root.transform, "EnemySpawns", new Vector3(c.x, c.y, 0f));
            Vector3[] offsets =
            {
                new Vector3(-b.size.x * 0.25f,  b.size.y * 0.15f, 0f),
                new Vector3( b.size.x * 0.25f,  b.size.y * 0.15f, 0f),
                new Vector3(-b.size.x * 0.25f, -b.size.y * 0.05f, 0f),
                new Vector3( b.size.x * 0.25f, -b.size.y * 0.05f, 0f)
            };
            for (int i = 0; i < offsets.Length; i++)
            {
                GetOrCreateChild(enemyRoot, "Spawn_0" + (i + 1), c + offsets[i]);
            }
        }

        Transform exit = GetOrCreateChild(root.transform, "Exit", new Vector3(c.x, b.max.y - margin, 0f));
        if (exit.GetComponent<CircleCollider2D>() == null)
        {
            CircleCollider2D col = exit.gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 1f;
        }
        if (exit.GetComponent<RoomExit>() == null) exit.gameObject.AddComponent<RoomExit>();

        // 4. RoomController
        RoomController rc = root.GetComponent<RoomController>();
        if (rc == null) rc = root.AddComponent<RoomController>();

        SerializedObject so = new SerializedObject(rc);
        so.FindProperty("roomName").stringValue = root.name;
        so.FindProperty("playerSpawnPoint").objectReferenceValue = playerSpawn;
        so.FindProperty("exit").objectReferenceValue = exit.gameObject;
        so.FindProperty("autoStartOnPlay").boolValue = false;

        SerializedProperty spawns = so.FindProperty("enemySpawnPoints");
        if (spawns.arraySize == 0)
        {
            spawns.arraySize = enemyRoot.childCount;
            for (int i = 0; i < enemyRoot.childCount; i++)
                spawns.GetArrayElementAtIndex(i).objectReferenceValue = enemyRoot.GetChild(i);
        }

        SerializedProperty prefabs = so.FindProperty("enemyPrefabs");
        if (prefabs.arraySize == 0)
        {
            GameObject drone = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/SentryDrone.prefab");
            GameObject turret = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Prefabs/TurretBot.prefab");
            List<GameObject> list = new List<GameObject>();
            if (drone != null) list.Add(drone);
            if (turret != null) list.Add(turret);

            prefabs.arraySize = list.Count;
            for (int i = 0; i < list.Count; i++) prefabs.GetArrayElementAtIndex(i).objectReferenceValue = list[i];
        }
        so.ApplyModifiedProperties();

        // 5. Lưu prefab
        EnsureFolder(RoomPrefabDir);
        string path = RoomPrefabDir + "/" + root.name + ".prefab";
        if (File.Exists(path) && !EditorUtility.DisplayDialog("Room prefab", path + " đã tồn tại. Ghi đè?", "Ghi đè", "Hủy"))
            return;

        PrefabUtility.SaveAsPrefabAsset(root, path);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Room prefab",
            "Đã lưu " + path + "\n- Tilemap được thêm collider: " + colliderCount +
            "\n- Spawn point và Exit đặt ở vị trí GỢI Ý: mở prefab và kéo cho đúng sàn.\n" +
            "- Xóa object phòng khỏi scene (RoomManager sẽ tự tạo phòng lúc chơi).\n" +
            "- Sau đó chạy menu 3 để tạo RoomData.", "OK");
    }

    // ------------------------------------------------------------------
    // 5. Build Settings
    // ------------------------------------------------------------------
    [MenuItem("Core Breach/Member B/5. Add Menu + Game scenes to Build Settings")]
    public static void AddScenesToBuildSettings()
    {
        string menuPath = ScenesDir + "/" + MenuSceneName + ".unity";
        string gamePath = ScenesDir + "/" + GameSceneName + ".unity";

        List<EditorBuildSettingsScene> list = new List<EditorBuildSettingsScene>();
        if (File.Exists(menuPath)) list.Add(new EditorBuildSettingsScene(menuPath, true));
        if (File.Exists(gamePath)) list.Add(new EditorBuildSettingsScene(gamePath, true));

        foreach (EditorBuildSettingsScene s in EditorBuildSettings.scenes)
        {
            if (s.path != menuPath && s.path != gamePath) list.Add(s);
        }

        EditorBuildSettings.scenes = list.ToArray();
        Debug.Log("[CoreBreachSetupB] Build Settings: " + list.Count + " scene (Menu đứng đầu).");
    }

    // ==================================================================
    // Helpers
    // ==================================================================
    private static Canvas CreateCanvas(string name)
    {
        GameObject go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        return canvas;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;

        GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
    }

    private static Font GetFont()
    {
        Font f = null;
        try { f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
        if (f == null)
        {
            try { f = Resources.GetBuiltinResource<Font>("Arial.ttf"); } catch { }
        }
        return f;
    }

    private static void Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static Text MakeText(Transform parent, string name, string content, int fontSize, TextAnchor alignment,
                                 Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);

        // anchorMin == anchorMax trong mọi lời gọi: dùng chung 1 điểm neo
        Place(go.GetComponent<RectTransform>(), anchorMin, pos, size);

        Text t = go.GetComponent<Text>();
        t.font = GetFont();
        t.text = content;
        t.fontSize = fontSize;
        t.alignment = alignment;
        t.color = color;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    private static Slider MakeBar(Transform parent, string name, Vector2 pos, Vector2 size, Color fillColor)
    {
        GameObject root = new GameObject(name, typeof(RectTransform), typeof(Slider));
        root.transform.SetParent(parent, false);
        Place(root.GetComponent<RectTransform>(), new Vector2(0f, 1f), pos, size);

        GameObject bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(root.transform, false);
        Stretch(bg.GetComponent<RectTransform>());
        bg.GetComponent<Image>().color = new Color(0.08f, 0.08f, 0.1f, 0.85f);

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(root.transform, false);
        Stretch(fillArea.GetComponent<RectTransform>());

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        Stretch(fill.GetComponent<RectTransform>());
        fill.GetComponent<Image>().color = fillColor;

        Slider s = root.GetComponent<Slider>();
        s.fillRect = fill.GetComponent<RectTransform>();
        s.direction = Slider.Direction.LeftToRight;
        s.minValue = 0f;
        s.maxValue = 100f;
        s.value = 100f;
        s.interactable = false;
        s.transition = Selectable.Transition.None;
        return s;
    }

    private static Button MakeButton(Transform parent, string label, Vector2 pos)
    {
        Vector2 mid = new Vector2(0.5f, 0.5f);

        GameObject go = new GameObject(label + "_Button", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        Place(go.GetComponent<RectTransform>(), mid, pos, new Vector2(320f, 54f));
        go.GetComponent<Image>().color = new Color(0.15f, 0.25f, 0.4f, 1f);

        MakeText(go.transform, "Label", label, 28, TextAnchor.MiddleCenter, mid, mid, Vector2.zero, new Vector2(320f, 54f), Color.white);
        return go.GetComponent<Button>();
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void SetRef(SerializedObject so, string field, Object value)
    {
        SerializedProperty p = so.FindProperty(field);
        if (p == null)
        {
            Debug.LogWarning("[CoreBreachSetupB] Không tìm thấy field '" + field + "' trong " + so.targetObject.GetType().Name);
            return;
        }
        p.objectReferenceValue = value;
    }

    private static void FinishScene(GameObject selected)
    {
        Selection.activeGameObject = selected;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        string leaf = Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, leaf);
    }

    private static Transform GetOrCreateChild(Transform parent, string name, Vector3 worldPos)
    {
        Transform t = parent.Find(name);
        if (t != null) return t;

        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = worldPos;
        return go.transform;
    }

    private static void AddWallCollider(GameObject go)
    {
        Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
        if (rb == null) rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        if (go.GetComponent<CompositeCollider2D>() == null) go.AddComponent<CompositeCollider2D>();

        TilemapCollider2D tc = go.GetComponent<TilemapCollider2D>();
        if (tc == null) tc = go.AddComponent<TilemapCollider2D>();
        tc.compositeOperation = Collider2D.CompositeOperation.Merge;
    }
}
