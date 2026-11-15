using UnityEngine;

/// <summary>
/// Hệ thống Menu Chọn Nhân Vật (Character Selection Screen) trước khi chơi:
/// - Lựa chọn 3 lớp nhân vật: Unit-7 Recon, Unit-0 Striker, Unit-9 Titan
/// - Cấu hình 3 bộ súng tầm xa và 3 vũ khí cận chiến đặc trưng riêng biệt
/// - Phím tắt [1], [2], [3] hoặc Click chuột để chọn, [SPACE]/[ENTER]/[W/A/S/D] để xuất trận
/// </summary>
public class CharacterSelectSystem : MonoBehaviour
{
    public static CharacterSelectSystem Instance { get; private set; }

    [Header("State")]
    [SerializeField] private bool isSelectionOpen = true;
    [SerializeField] private CharacterClass selectedClass = CharacterClass.Unit7_Recon;

    [Header("Character Sprites")]
    [SerializeField] private Sprite unit7Sprite;
    [SerializeField] private Sprite unit0Sprite;
    [SerializeField] private Sprite unit9Sprite;

    [Header("Weapon Sprites")]
    [SerializeField] private Sprite plasmaSprite;
    [SerializeField] private Sprite smgSprite;
    [SerializeField] private Sprite scatterSprite;
    [SerializeField] private Sprite daggerSprite;
    [SerializeField] private Sprite katanaSprite;
    [SerializeField] private Sprite hammerSprite;

    private static Texture2D whitePixel;

    public bool IsSelectionOpen => isSelectionOpen;
    public CharacterClass SelectedClass => selectedClass;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        LoadSpritesIfNull();
    }

    private void Start()
    {
        // Khi game vừa mở: Nếu GameManager đang ở MainMenu, ẩn CharacterSelect để MainMenu hiển thị
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.MainMenu)
        {
            isSelectionOpen = false;
        }
        else if (isSelectionOpen)
        {
            Time.timeScale = 1f; // Vẫn để 1f để UI và hiệu ứng hoạt ảnh chạy mượt
        }
    }

    private void Update()
    {
        if (!isSelectionOpen) return;

        // Phím ESC: Quay lại Menu chính
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMainMenu();
            return;
        }

        // Phím tắt chọn nhanh 1, 2, 3
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            SelectCharacter(CharacterClass.Unit7_Recon);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            SelectCharacter(CharacterClass.Unit0_Striker);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            SelectCharacter(CharacterClass.Unit9_Titan);
        }

        // Phím xác nhận: Phím số chọn 2 lần, hoặc Space / Enter / E / W A S D
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || 
            Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E) ||
            Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || 
            Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
        {
            ConfirmSelectionAndStart();
        }
    }

    public void OpenSelectionMenu()
    {
        isSelectionOpen = true;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameState.CharacterSelect);
        }
    }

    public void ReturnToMainMenu()
    {
        isSelectionOpen = false;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
    }

    public void SelectCharacter(CharacterClass cClass)
    {
        selectedClass = cClass;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayShoot();
        }
    }

    public void ConfirmSelectionAndStart()
    {
        isSelectionOpen = false;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameState.Playing);
        }

        // Áp dụng lớp nhân vật đã chọn cho Player
        ApplyCharacter(selectedClass);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayRoomClear();
        }

        CombatFeedback.ShowNotice(Vector3.zero, "SYSTEM INITIALIZED: " + selectedClass, Color.cyan);
        Debug.Log($"[CharacterSelect] Đã chọn: {selectedClass} - Bắt đầu xâm nhập hầm Vault!");

        // Khởi động phòng và spawn quái ngay sau khi người chơi chọn xong nhân vật
        RoomController room = FindFirstObjectByType<RoomController>();
        if (room != null && !room.IsRoomStarted())
        {
            room.StartRoom();
        }
    }

    public void ApplyCharacterClass(CharacterClass cClass)
    {
        selectedClass = cClass;
        ApplyCharacter(cClass);
    }

    private void ApplyCharacter(CharacterClass cClass)
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            PlayerMovement pm = FindFirstObjectByType<PlayerMovement>();
            if (pm != null) playerObj = pm.gameObject;
        }

        if (playerObj == null)
        {
            Debug.LogWarning("[CharacterSelect] Không tìm thấy Player trong Scene!");
            return;
        }

        CharacterProfile profile = CharacterDatabase.GetProfile(cClass);

        // 1. Cập nhật Máu & Giáp
        PlayerHealth health = playerObj.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.SetMaxStats(profile.maxHealth, profile.maxArmor);
        }

        // 2. Cập nhật Tốc độ di chuyển
        PlayerMovement movement = playerObj.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.SetMoveSpeed(profile.moveSpeed);
        }

        // 3. Cập nhật Sprite và Visuals
        LoadSpritesIfNull();
        PlayerVisuals visuals = playerObj.GetComponent<PlayerVisuals>();
        if (visuals != null)
        {
            if (cClass == CharacterClass.Unit7_Recon)
            {
                visuals.SetTrailColor(new Color(0f, 0.9f, 1f, 0.65f));
                if (unit7Sprite != null) visuals.SetSprite(unit7Sprite);
            }
            else if (cClass == CharacterClass.Unit0_Striker)
            {
                visuals.SetTrailColor(new Color(1f, 0.2f, 0.45f, 0.75f));
                if (unit0Sprite != null) visuals.SetSprite(unit0Sprite);
            }
            else if (cClass == CharacterClass.Unit9_Titan)
            {
                visuals.SetTrailColor(new Color(1f, 0.75f, 0.1f, 0.75f));
                if (unit9Sprite != null) visuals.SetSprite(unit9Sprite);
            }
            visuals.SetClassVisuals(cClass);
        }

        // 4. Áp dụng 3 Bộ Súng & 3 Vũ Khí Cận Chiến Riêng Biệt
        WeaponController wc = playerObj.GetComponent<WeaponController>();
        if (wc != null)
        {
            MeleeWeapon melee = wc.Slot2Weapon as MeleeWeapon;
            if (melee == null) melee = playerObj.GetComponentInChildren<MeleeWeapon>();

            if (cClass == CharacterClass.Unit0_Striker)
            {
                // Unit-0: Viper Laser SMG + Cyber Muramasa Katana (Mặc định rút kiếm Slot 2)
                if (wc.Slot1Weapon != null)
                {
                    wc.Slot1Weapon.Configure("Viper Laser SMG", 13f, 8.0f, 7f, 1, 4f, smgSprite);
                    wc.Slot1Weapon.WeaponSprite = smgSprite;
                }
                if (melee != null)
                {
                    melee.ConfigureMelee("Cyber Muramasa Katana", MeleeType.CyberKatana, 42f, 2.6f, 150f, 0.30f, new Color(1f, 0.2f, 0.45f));
                    melee.WeaponSprite = katanaSprite;
                }
                wc.EquipWeapon(1); // Rút kiếm Katana
            }
            else if (cClass == CharacterClass.Unit9_Titan)
            {
                // Unit-9: Heavy Scatter Cannon 4 tia + Gravity Thunder Hammer 360 độ
                if (wc.Slot1Weapon != null)
                {
                    wc.Slot1Weapon.Configure("Heavy Scatter Cannon", 15f, 2.2f, 18f, 4, 28f, scatterSprite);
                    wc.Slot1Weapon.WeaponSprite = scatterSprite;
                }
                if (melee != null)
                {
                    melee.ConfigureMelee("Gravity Thunder Hammer", MeleeType.GravityHammer, 55f, 3.2f, 360f, 0.55f, new Color(1f, 0.75f, 0.1f));
                    melee.WeaponSprite = hammerSprite;
                }
                wc.EquipWeapon(0); // Rút pháo hạng nặng
            }
            else
            {
                // Unit-7: Dual Plasma Carbine + Nano Energy Daggers
                if (wc.Slot1Weapon != null)
                {
                    wc.Slot1Weapon.Configure("Dual Plasma Carbine", 14f, 5.0f, 8f, 2, 8f, plasmaSprite);
                    wc.Slot1Weapon.WeaponSprite = plasmaSprite;
                }
                if (melee != null)
                {
                    melee.ConfigureMelee("Nano Energy Daggers", MeleeType.CyberDaggers, 24f, 2.0f, 120f, 0.18f, new Color(0f, 0.95f, 1f));
                    melee.WeaponSprite = daggerSprite;
                }
                wc.EquipWeapon(0); // Rút súng Plasma
            }
        }
    }

    private void LoadSpritesIfNull()
    {
        if (unit7Sprite == null) unit7Sprite = Resources.Load<Sprite>("Characters/player_unit7_scout");
        if (unit0Sprite == null) unit0Sprite = Resources.Load<Sprite>("Characters/player_unit0_striker");
        if (unit9Sprite == null) unit9Sprite = Resources.Load<Sprite>("Characters/player_unit9_titan");

        if (plasmaSprite == null) plasmaSprite = Resources.Load<Sprite>("Weapons/weapon_plasma");
        if (plasmaSprite == null) plasmaSprite = PlayerVisuals.CreateFallbackWeaponSprite("plasma");

        if (smgSprite == null) smgSprite = Resources.Load<Sprite>("Weapons/weapon_machine");
        if (smgSprite == null) smgSprite = PlayerVisuals.CreateFallbackWeaponSprite("smg");

        if (scatterSprite == null) scatterSprite = Resources.Load<Sprite>("Weapons/weapon_scatter");
        if (scatterSprite == null) scatterSprite = PlayerVisuals.CreateFallbackWeaponSprite("scatter");

        if (daggerSprite == null) daggerSprite = Resources.Load<Sprite>("Weapons/weapon_dagger");
        if (daggerSprite == null) daggerSprite = PlayerVisuals.CreateFallbackWeaponSprite("dagger");

        if (katanaSprite == null) katanaSprite = Resources.Load<Sprite>("Weapons/weapon_katana");
        if (katanaSprite == null) katanaSprite = PlayerVisuals.CreateFallbackWeaponSprite("katana");

        if (hammerSprite == null) hammerSprite = Resources.Load<Sprite>("Weapons/weapon_hammer");
        if (hammerSprite == null) hammerSprite = PlayerVisuals.CreateFallbackWeaponSprite("hammer");
    }

    private static void EnsureWhitePixel()
    {
        if (whitePixel == null)
        {
            whitePixel = new Texture2D(1, 1);
            whitePixel.SetPixel(0, 0, Color.white);
            whitePixel.Apply();
        }
    }

    private void OnGUI()
    {
        if (!isSelectionOpen) return;

        EnsureWhitePixel();
        float sw = Screen.width;
        float sh = Screen.height;

        // Phủ mờ màn hình
        GUI.color = new Color(0.04f, 0.06f, 0.1f, 0.94f);
        GUI.DrawTexture(new Rect(0, 0, sw, sh), whitePixel);

        // Khung trung tâm
        float panelW = Mathf.Min(840f, sw - 40f);
        float panelH = Mathf.Min(530f, sh - 40f);
        float startX = (sw - panelW) / 2f;
        float startY = (sh - panelH) / 2f;

        // Viền phát sáng Cyan
        GUI.color = new Color(0f, 0.85f, 1f, 0.8f);
        GUI.DrawTexture(new Rect(startX - 2, startY - 2, panelW + 4, panelH + 4), whitePixel);

        // Nền Panel
        GUI.color = new Color(0.07f, 0.09f, 0.14f, 0.98f);
        GUI.DrawTexture(new Rect(startX, startY, panelW, panelH), whitePixel);

        // Header Title
        GUI.color = new Color(0f, 0.95f, 1f, 1f);
        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 20,
            fontStyle = FontStyle.Bold
        };
        GUI.Label(new Rect(startX, startY + 16, panelW, 30), "◈ CHỌN ĐƠN VỊ CHIẾN ĐẤU - CORE BREACH ◈", titleStyle);

        GUI.color = new Color(0.7f, 0.8f, 0.95f, 0.9f);
        GUIStyle subStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 12
        };
        GUI.Label(new Rect(startX, startY + 44, panelW, 20), "Mỗi đơn vị sở hữu 1 Súng Tầm Xa và 1 Vũ Khí Cận Chiến độc quyền riêng biệt. Bấm [R] để đổi trong trận.", subStyle);

        // 3 Thẻ Nhân Vật
        CharacterProfile[] profiles = CharacterDatabase.GetAllProfiles();
        float cardW = (panelW - 60f) / 3f;
        float cardH = 360f;
        float cardY = startY + 70f;

        for (int i = 0; i < profiles.Length; i++)
        {
            CharacterProfile p = profiles[i];
            float cardX = startX + 20f + i * (cardW + 10f);
            bool isSelected = p.characterClass == selectedClass;

            // Viền thẻ
            GUI.color = isSelected ? p.themeColor : new Color(0.2f, 0.25f, 0.35f, 0.8f);
            GUI.DrawTexture(new Rect(cardX - 1, cardY - 1, cardW + 2, cardH + 2), whitePixel);

            // Nền thẻ
            GUI.color = isSelected ? new Color(0.12f, 0.15f, 0.22f, 1f) : new Color(0.08f, 0.1f, 0.15f, 0.95f);
            GUI.DrawTexture(new Rect(cardX, cardY, cardW, cardH), whitePixel);

            // Tên & Phím tắt
            GUI.color = isSelected ? p.themeColor : Color.white;
            GUIStyle cardTitle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };
            GUI.Label(new Rect(cardX + 6, cardY + 10, cardW - 12, 22), $"[{i + 1}] {p.codeName}", cardTitle);

            // Vai trò
            GUI.color = isSelected ? Color.white : new Color(0.7f, 0.75f, 0.85f);
            GUIStyle roleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 11 };
            GUI.Label(new Rect(cardX + 6, cardY + 32, cardW - 12, 18), p.roleTitle, roleStyle);

            // Chỉ số Bars
            float statY = cardY + 58f;
            DrawStatLine(cardX + 14, statY, cardW - 28, "MÁU (HP)", $"{p.maxHealth}", p.maxHealth / 150f, new Color(0f, 0.9f, 0.45f));
            DrawStatLine(cardX + 14, statY + 28, cardW - 28, "GIÁP", $"{p.maxArmor}", p.maxArmor / 50f, new Color(0f, 0.75f, 1f));
            DrawStatLine(cardX + 14, statY + 56, cardW - 28, "TỐC ĐỘ", $"{p.moveSpeed}", p.moveSpeed / 6.0f, new Color(1f, 0.75f, 0.1f));

            // Súng Tầm Xa
            float wepY = statY + 90f;
            GUI.color = new Color(0.6f, 0.8f, 1f);
            GUIStyle subLabel = new GUIStyle(GUI.skin.label) { fontSize = 10, fontStyle = FontStyle.Bold };
            GUI.Label(new Rect(cardX + 14, wepY, cardW - 28, 16), "🔫 SÚNG CHÍNH:", subLabel);
            GUI.color = isSelected ? p.themeColor : Color.white;
            GUIStyle wepStyle = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 11 };
            GUI.Label(new Rect(cardX + 14, wepY + 15, cardW - 28, 18), p.gunName, wepStyle);

            // Cận Chiến
            GUI.color = new Color(1f, 0.75f, 0.3f);
            GUI.Label(new Rect(cardX + 14, wepY + 34, cardW - 28, 16), "⚔️ CẬN CHIẾN:", subLabel);
            GUI.color = isSelected ? p.themeColor : Color.white;
            GUI.Label(new Rect(cardX + 14, wepY + 49, cardW - 28, 18), p.meleeName, wepStyle);

            // Mô tả
            GUI.color = new Color(0.75f, 0.8f, 0.9f);
            GUIStyle descStyle = new GUIStyle(GUI.skin.label) { fontSize = 10, wordWrap = true };
            GUI.Label(new Rect(cardX + 14, wepY + 70, cardW - 28, 55), p.description, descStyle);

            // Nút Click Chọn Thẻ
            if (GUI.Button(new Rect(cardX + 14, cardY + cardH - 34, cardW - 28, 26), isSelected ? "✓ ĐANG CHỌN" : $"CHỌN [{i + 1}]"))
            {
                SelectCharacter(p.characterClass);
            }
        }

        // Footer Buttons: VỀ MENU CHÍNH & XÁC NHẬN XUẤT TRẬN
        float totalBtnsW = 540f;
        float startBtnX = startX + (panelW - totalBtnsW) / 2f;
        float btnY = cardY + cardH + 12f;
        float btnH = 44f;

        // Nút 1: Về Menu chính
        GUI.color = new Color(1f, 0.75f, 0.2f);
        if (GUI.Button(new Rect(startBtnX, btnY, 190f, btnH), "◀ VỀ MENU CHÍNH [ESC]"))
        {
            ReturnToMainMenu();
        }

        // Nút 2: Xác nhận xuất trận
        GUI.color = new Color(0f, 0.95f, 0.45f);
        if (GUI.Button(new Rect(startBtnX + 205f, btnY, 335f, btnH), "▶ XÁC NHẬN VÀ XUẤT TRẬN [SPACE / ENTER]"))
        {
            ConfirmSelectionAndStart();
        }
        GUI.color = Color.white;
    }

    private void DrawStatLine(float x, float y, float w, string label, string val, float fill01, Color barCol)
    {
        GUI.color = new Color(0.75f, 0.8f, 0.9f);
        GUI.Label(new Rect(x, y, 75, 18), label);
        GUI.color = Color.white;
        GUI.Label(new Rect(x + w - 45, y, 45, 18), val);

        float barH = 5f;
        float barY = y + 17f;
        // Background
        GUI.color = new Color(0.15f, 0.18f, 0.25f, 1f);
        GUI.DrawTexture(new Rect(x, barY, w, barH), whitePixel);
        // Fill
        GUI.color = barCol;
        GUI.DrawTexture(new Rect(x, barY, w * Mathf.Clamp01(fill01), barH), whitePixel);
        GUI.color = Color.white;
    }
}
