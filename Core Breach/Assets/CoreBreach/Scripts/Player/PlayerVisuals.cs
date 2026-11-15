using System.Collections;
using UnityEngine;

/// <summary>
/// Quản lý toàn bộ hình ảnh và hoạt ảnh nhân vật chuẩn phong cách Soul Knight:
/// - Hoạt ảnh bước chân cơ học rõ rệt (Leg_Left & Leg_Right sải bước so le và nhấc chân khi chạy)
/// - Tay và vũ khí hiển thị rõ ràng trên tay (HeldWeaponVisual & ArmHand)
/// - Hoạt ảnh vung tay và vung vũ khí cận chiến:
///   + Katana: Rút gươm vung chém quét cung 150°
///   + Búa Gravity: Giơ búa lên cao overhead rồi đập mạnh nện xuống sàn
///   + Song dao: Đâm chém liên hoàn 2 nhịp nhanh
/// - Hướng quay thuần túy bám theo điều khiển của người chơi (chuột/WASD), không tự động giật theo quái
/// - Bóng đổ tròn mềm mại (Drop Shadow) dưới chân
/// </summary>
public class PlayerVisuals : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform visualTransform;

    private PlayerMovement playerMovement;
    private PlayerAim playerAim;
    private Camera mainCamera;

    private Transform leftLeg;
    private Transform rightLeg;
    private SpriteRenderer leftLegSr;
    private SpriteRenderer rightLegSr;

    private Transform weaponPivot;
    private Transform weaponVisualHolder;
    private SpriteRenderer weaponVisualSr;
    private SpriteRenderer handSr;

    private Vector3 initialLocalPos;
    private float walkCycle = 0f;
    private float ghostTrailTimer = 0f;
    private float recoilOffset = 0f;
    private bool isSwingingMelee = false;
    private Color trailColor = new Color(0f, 0.9f, 1f, 0.65f);

    private static Sprite cachedShadowSprite;
    private static Sprite defaultBootSprite;
    private static Sprite defaultHandSprite;

    private Sprite pendingPlayerSprite;
    private Sprite pendingWeaponSprite;

    public void SetTrailColor(Color col)
    {
        trailColor = col;
    }

    public void SetSprite(Sprite spr)
    {
        pendingPlayerSprite = spr;
        if (spriteRenderer != null && spr != null)
        {
            spriteRenderer.sprite = spr;
            spriteRenderer.enabled = true;
        }
    }

    private Coroutine weaponSwitchRoutine;

    public void SetWeaponSprite(Sprite spr)
    {
        pendingWeaponSprite = spr;
        if (weaponVisualSr != null)
        {
            if (spr == null) spr = CreateFallbackWeaponSprite("plasma");
            weaponVisualSr.sprite = spr;
            weaponVisualSr.enabled = true;
            PlayWeaponSwitchAnim();
        }
    }

    public void PlayWeaponSwitchAnim()
    {
        if (weaponVisualHolder == null) return;
        if (weaponSwitchRoutine != null) StopCoroutine(weaponSwitchRoutine);
        weaponSwitchRoutine = StartCoroutine(WeaponSwitchAnimRoutine());
    }

    private IEnumerator WeaponSwitchAnimRoutine()
    {
        Vector3 origPos = new Vector3(0.22f, 0f, 0f);
        // Hiệu ứng giật nảy và phóng to khi rút vũ khí mới (Juicy Weapon Swap Feedback)
        weaponVisualHolder.localScale = new Vector3(1.4f, 1.4f, 1f);
        weaponVisualHolder.localPosition = origPos + new Vector3(-0.08f, -0.06f, 0f);

        float elapsed = 0f;
        float duration = 0.12f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            weaponVisualHolder.localScale = Vector3.Lerp(new Vector3(1.4f, 1.4f, 1f), Vector3.one, t);
            weaponVisualHolder.localPosition = Vector3.Lerp(origPos + new Vector3(-0.08f, -0.06f, 0f), origPos, t);
            yield return null;
        }

        weaponVisualHolder.localScale = Vector3.one;
        weaponVisualHolder.localPosition = origPos;
    }

    public void SetClassVisuals(CharacterClass cClass)
    {
        string bootName = cClass == CharacterClass.Unit0_Striker ? "boot_unit0" :
                         (cClass == CharacterClass.Unit9_Titan ? "boot_unit9" : "boot_unit7");
        Sprite bootSpr = Resources.Load<Sprite>("Characters/" + bootName);
        if (bootSpr == null) bootSpr = EnsureDefaultBoot();

        if (leftLegSr != null) leftLegSr.sprite = bootSpr;
        if (rightLegSr != null) rightLegSr.sprite = bootSpr;

        string handName = cClass == CharacterClass.Unit0_Striker ? "hand_unit0" :
                         (cClass == CharacterClass.Unit9_Titan ? "hand_unit9" : "hand_unit7");
        Sprite handSpr = Resources.Load<Sprite>("Characters/" + handName);
        if (handSpr == null) handSpr = EnsureDefaultHand();
        if (handSr != null) handSr.sprite = handSpr;
    }

    private void Awake()
    {
        mainCamera = Camera.main;

        // 1. Tách biệt hoàn toàn VisualModel khỏi Physics Root
        Transform child = transform.Find("VisualModel");
        if (child == null)
        {
            GameObject visualObj = new GameObject("VisualModel");
            visualObj.transform.SetParent(transform, false);
            visualObj.transform.localPosition = Vector3.zero;
            visualObj.transform.localRotation = Quaternion.identity;
            visualObj.transform.localScale = Vector3.one;

            SpriteRenderer rootSr = GetComponent<SpriteRenderer>();
            if (rootSr != null)
            {
                SpriteRenderer childSr = visualObj.AddComponent<SpriteRenderer>();
                childSr.sprite = rootSr.sprite;
                childSr.color = rootSr.color;
                childSr.sortingOrder = 10;
                childSr.material = rootSr.material;
                rootSr.enabled = false;
                spriteRenderer = childSr;
            }
            visualTransform = visualObj.transform;
        }
        else
        {
            visualTransform = child;
            if (spriteRenderer == null) spriteRenderer = visualTransform.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) spriteRenderer.sortingOrder = 10;
        }

        playerMovement = GetComponent<PlayerMovement>();
        playerAim = GetComponent<PlayerAim>();
        initialLocalPos = Vector3.zero;

        SetupSoftShadow();
        SetupAnimatedLegs();
        SetupWeaponAndArm();

        if (pendingPlayerSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = pendingPlayerSprite;
            spriteRenderer.enabled = true;
        }
        if (pendingWeaponSprite != null && weaponVisualSr != null)
        {
            weaponVisualSr.sprite = pendingWeaponSprite;
            weaponVisualSr.enabled = true;
        }
    }

    private void SetupSoftShadow()
    {
        EnsureShadowSprite();
        Transform sChild = transform.Find("DropShadow");
        if (sChild == null)
        {
            GameObject sObj = new GameObject("DropShadow");
            sObj.transform.SetParent(transform, false);
            sObj.transform.localPosition = new Vector3(0f, -0.32f, 0f);
            sObj.transform.localScale = new Vector3(0.75f, 0.35f, 1f);

            SpriteRenderer sSr = sObj.AddComponent<SpriteRenderer>();
            sSr.sprite = cachedShadowSprite;
            sSr.sortingOrder = 2; // Sát mặt đất
            sSr.color = new Color(0f, 0f, 0f, 0.28f);
        }
    }

    private void SetupAnimatedLegs()
    {
        Sprite bootSpr = EnsureDefaultBoot();

        // Chân trái (Leg_Left)
        Transform lChild = transform.Find("Leg_Left");
        if (lChild == null)
        {
            GameObject lObj = new GameObject("Leg_Left");
            lObj.transform.SetParent(transform, false);
            lObj.transform.localPosition = new Vector3(-0.12f, -0.30f, 0f);
            leftLegSr = lObj.AddComponent<SpriteRenderer>();
            leftLegSr.sprite = bootSpr;
            leftLegSr.sortingOrder = 8; // Nằm sau thân
            leftLeg = lObj.transform;
        }
        else
        {
            leftLeg = lChild;
            leftLegSr = lChild.GetComponent<SpriteRenderer>();
        }

        // Chân phải (Leg_Right)
        Transform rChild = transform.Find("Leg_Right");
        if (rChild == null)
        {
            GameObject rObj = new GameObject("Leg_Right");
            rObj.transform.SetParent(transform, false);
            rObj.transform.localPosition = new Vector3(0.12f, -0.30f, 0f);
            rightLegSr = rObj.AddComponent<SpriteRenderer>();
            rightLegSr.sprite = bootSpr;
            rightLegSr.sortingOrder = 9; // Nằm trước chân trái, sau thân
            rightLeg = rObj.transform;
        }
        else
        {
            rightLeg = rChild;
            rightLegSr = rChild.GetComponent<SpriteRenderer>();
        }
    }

    private void SetupWeaponAndArm()
    {
        Transform pivot = transform.Find("WeaponPivot");
        if (pivot == null)
        {
            GameObject newPivot = new GameObject("WeaponPivot");
            newPivot.transform.SetParent(transform, false);
            newPivot.transform.localPosition = new Vector3(0.14f, -0.06f, 0f);
            pivot = newPivot.transform;
        }
        weaponPivot = pivot;

        Transform wVisual = pivot.Find("HeldWeaponVisual");
        if (wVisual == null)
        {
            GameObject wObj = new GameObject("HeldWeaponVisual");
            wObj.transform.SetParent(pivot, false);
            wObj.transform.localPosition = new Vector3(0.22f, 0f, 0f);

            weaponVisualSr = wObj.AddComponent<SpriteRenderer>();
            weaponVisualSr.sortingOrder = 14;
            weaponVisualHolder = wObj.transform;

            // Bàn tay cầm vũ khí (Hand)
            GameObject hObj = new GameObject("Hand");
            hObj.transform.SetParent(wObj.transform, false);
            hObj.transform.localPosition = Vector3.zero;
            handSr = hObj.AddComponent<SpriteRenderer>();
            handSr.sprite = EnsureDefaultHand();
            handSr.sortingOrder = 15; // Nổi lên trên vũ khí
        }
        else
        {
            weaponVisualHolder = wVisual;
            weaponVisualSr = wVisual.GetComponent<SpriteRenderer>();
            Transform hChild = wVisual.Find("Hand");
            if (hChild != null)
            {
                handSr = hChild.GetComponent<SpriteRenderer>();
                hChild.localPosition = Vector3.zero;
            }
        }

        // Đảm bảo luôn có sprite vũ khí hiển thị trên tay
        if (weaponVisualSr != null && weaponVisualSr.sprite == null)
        {
            WeaponController wc = GetComponent<WeaponController>();
            if (wc != null && wc.CurrentWeapon != null && wc.CurrentWeapon.WeaponSprite != null)
            {
                weaponVisualSr.sprite = wc.CurrentWeapon.WeaponSprite;
            }
            else
            {
                weaponVisualSr.sprite = CreateFallbackWeaponSprite("plasma");
            }
            weaponVisualSr.enabled = true;
        }
    }

    private void Update()
    {
        if (playerMovement == null) return;

        bool isMoving = playerMovement.MoveInput.sqrMagnitude > 0.05f;
        bool isDashing = playerMovement.IsDashing;

        // 1. CẬP NHẬT HƯỚNG QUAY (THUẦN TÚY THEO ĐIỀU KHIỂN CỦA NGƯỜI CHƠI)
        UpdatePlayerControlledFacing();

        // 2. DASH AFTERIMAGE GHOST TRAIL
        if (isDashing)
        {
            ghostTrailTimer -= Time.deltaTime;
            if (ghostTrailTimer <= 0f)
            {
                SpawnGhostTrail();
                ghostTrailTimer = 0.04f;
            }
            return;
        }

        // 3. HOẠT ẢNH BƯỚC ĐI CƠ HỌC (Hai chân sải bước rõ rệt)
        if (isMoving)
        {
            walkCycle += Time.deltaTime * 14f;

            // Chân trái và chân phải sải bước so le và nhấc bước
            float strideL = Mathf.Sin(walkCycle) * 0.16f;
            float strideR = Mathf.Sin(walkCycle + Mathf.PI) * 0.16f;
            float liftL = Mathf.Max(0f, Mathf.Cos(walkCycle)) * 0.08f;
            float liftR = Mathf.Max(0f, Mathf.Cos(walkCycle + Mathf.PI)) * 0.08f;

            if (leftLeg != null)
            {
                leftLeg.localPosition = new Vector3(-0.12f + strideL, -0.30f + liftL, 0f);
            }
            if (rightLeg != null)
            {
                rightLeg.localPosition = new Vector3(0.12f + strideR, -0.30f + liftR, 0f);
            }

            // Thân người nhún nhảy theo nhịp chạy
            float bounceY = Mathf.Abs(Mathf.Sin(walkCycle)) * 0.06f;
            visualTransform.localPosition = initialLocalPos + new Vector3(0f, bounceY, 0f);

            // Bụi lướt chân
            if (Mathf.Abs(Mathf.Sin(walkCycle)) > 0.95f)
            {
                Vector3 stepPos = transform.position + new Vector3(strideL > 0 ? -0.12f : 0.12f, -0.32f, 0f);
                CombatFeedback.SpawnSparks(stepPos, -playerMovement.MoveInput, new Color(0.35f, 0.45f, 0.55f, 0.4f), 1);
            }
        }
        else
        {
            walkCycle = 0f;
            // Đứng yên: Hai chân đứng vững phẳng phiu
            if (leftLeg != null) leftLeg.localPosition = new Vector3(-0.12f, -0.30f, 0f);
            if (rightLeg != null) rightLeg.localPosition = new Vector3(0.12f, -0.30f, 0f);

            // Nhịp thở nhẹ nhàng
            float hoverY = Mathf.Sin(Time.time * 3f) * 0.02f;
            visualTransform.localPosition = initialLocalPos + new Vector3(0f, hoverY, 0f);
        }

        // 4. HỒI PHỤC GIẬT NÒNG VŨ KHÍ
        if (!isSwingingMelee && recoilOffset > 0.001f && weaponVisualHolder != null)
        {
            recoilOffset = Mathf.Lerp(recoilOffset, 0f, Time.deltaTime * 20f);
            weaponVisualHolder.localPosition = new Vector3(0.22f - recoilOffset, 0f, 0f);
        }
    }

    private void UpdatePlayerControlledFacing()
    {
        // Lấy hướng ngắm hoàn toàn từ điều khiển của người chơi
        Vector2 aimDir = playerAim != null ? playerAim.GetAimDirection() : Vector2.right;
        bool lookLeft = aimDir.x < 0;

        // Lật mặt nhân vật sang trái/phải theo điều khiển của người chơi
        float baseScaleX = lookLeft ? -1f : 1f;
        visualTransform.localScale = new Vector3(baseScaleX, 1f, 1f);

        // Lật 2 chân
        if (leftLegSr != null) leftLegSr.flipX = lookLeft;
        if (rightLegSr != null) rightLegSr.flipX = lookLeft;

        // Lật vũ khí và bàn tay
        if (weaponVisualSr != null) weaponVisualSr.flipY = lookLeft;
        if (handSr != null) handSr.flipY = lookLeft;

        // Vị trí WeaponPivot gắn bên hông người theo hướng quay
        if (weaponPivot != null)
        {
            weaponPivot.localPosition = new Vector3(lookLeft ? -0.14f : 0.14f, -0.06f, 0f);
        }
    }

    /// <summary>
    /// Kích hoạt hoạt ảnh vung tay và vung vũ khí cận chiến (Búa nện đất, Kiếm chém quét, Dao đâm liên hoàn)
    /// </summary>
    public void TriggerMeleeSwing(MeleeType type, Vector2 dir)
    {
        if (weaponVisualHolder != null)
        {
            StartCoroutine(MeleeSwingRoutine(type, dir));
        }
    }

    private IEnumerator MeleeSwingRoutine(MeleeType type, Vector2 dir)
    {
        isSwingingMelee = true;
        Vector3 origPos = new Vector3(0.22f, 0f, 0f);

        if (type == MeleeType.GravityHammer)
        {
            // BÚA TRỌNG LỰC: Giơ búa lên cao overhead (+65 độ), rồi đập mạnh nện xuống sàn (-40 độ)
            float windupDuration = 0.08f;
            float slamDuration = 0.10f;
            float elapsed = 0f;

            // 1. Giơ búa lên cao
            while (elapsed < windupDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / windupDuration;
                weaponVisualHolder.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, 65f, t));
                weaponVisualHolder.localPosition = Vector3.Lerp(origPos, origPos + new Vector3(-0.08f, 0.20f, 0f), t);
                yield return null;
            }

            // 2. NỆN MẠNH XUỐNG SÀN
            elapsed = 0f;
            while (elapsed < slamDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / slamDuration;
                weaponVisualHolder.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(65f, -45f, t));
                weaponVisualHolder.localPosition = Vector3.Lerp(origPos + new Vector3(-0.08f, 0.20f, 0f), origPos + new Vector3(0.12f, -0.22f, 0f), t);
                yield return null;
            }

            yield return new WaitForSeconds(0.06f);
        }
        else if (type == MeleeType.CyberKatana)
        {
            // CYBER KATANA: Rút gươm lùi ra sau (-50 độ), rồi vung chém quét mạnh mẽ tới trước (+70 độ)
            float drawDuration = 0.05f;
            float slashDuration = 0.10f;
            float elapsed = 0f;

            while (elapsed < drawDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / drawDuration;
                weaponVisualHolder.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, -50f, t));
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < slashDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / slashDuration;
                weaponVisualHolder.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(-50f, 70f, t));
                yield return null;
            }
        }
        else // CyberDaggers
        {
            // SONG ĐOẢN ĐAO: Đâm chém liên hoàn 2 nhịp nhanh
            for (int i = 0; i < 2; i++)
            {
                weaponVisualHolder.localPosition = origPos + new Vector3(0.20f, (i == 0 ? 0.06f : -0.06f), 0f);
                yield return new WaitForSeconds(0.04f);
                weaponVisualHolder.localPosition = origPos;
                yield return new WaitForSeconds(0.03f);
            }
        }

        // Trở về vị trí cầm vũ khí ban đầu
        weaponVisualHolder.localPosition = origPos;
        weaponVisualHolder.localRotation = Quaternion.identity;
        isSwingingMelee = false;
    }

    /// <summary>
    /// Giật súng lùi về sau khi bắn (Weapon Recoil)
    /// </summary>
    public void TriggerRecoil(Vector2 shootDir)
    {
        recoilOffset = 0.12f;
    }

    private void SpawnGhostTrail()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null) return;

        GameObject ghost = new GameObject("DashGhost");
        ghost.transform.position = transform.position;
        ghost.transform.rotation = visualTransform.rotation;
        ghost.transform.localScale = visualTransform.localScale;

        SpriteRenderer ghostSr = ghost.AddComponent<SpriteRenderer>();
        ghostSr.sprite = spriteRenderer.sprite;
        ghostSr.sortingOrder = 9;
        ghostSr.color = trailColor;

        StartCoroutine(FadeAndDestroyGhost(ghost, ghostSr));
    }

    private IEnumerator FadeAndDestroyGhost(GameObject ghost, SpriteRenderer sr)
    {
        float duration = 0.20f;
        float elapsed = 0f;
        Color startCol = sr.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (sr != null)
            {
                sr.color = new Color(startCol.r, startCol.g, startCol.b, Mathf.Lerp(startCol.a, 0f, t));
            }
            yield return null;
        }

        Destroy(ghost);
    }

    // =========================================================
    // FALLBACK PROCEDURAL SPRITE GENERATORS (CHỐNG LỖI MẤT HÌNH)
    // =========================================================

    private static Sprite EnsureDefaultBoot()
    {
        if (defaultBootSprite != null) return defaultBootSprite;
        defaultBootSprite = Resources.Load<Sprite>("Characters/boot_unit7");
        if (defaultBootSprite != null) return defaultBootSprite;

        int w = 16, h = 16;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (y >= 6 && y <= 13 && x >= 4 && x <= 11)
                    tex.SetPixel(x, y, new Color(0.3f, 0.4f, 0.5f, 1f));
                else if (y >= 12 && y <= 14 && x >= 3 && x <= 12)
                    tex.SetPixel(x, y, new Color(0.15f, 0.2f, 0.25f, 1f));
                else
                    tex.SetPixel(x, y, new Color(0, 0, 0, 0));
            }
        }
        tex.Apply();
        defaultBootSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f);
        return defaultBootSprite;
    }

    private static Sprite EnsureDefaultHand()
    {
        if (defaultHandSprite != null) return defaultHandSprite;
        defaultHandSprite = Resources.Load<Sprite>("Characters/hand_unit7");
        if (defaultHandSprite != null) return defaultHandSprite;

        int w = 10, h = 10;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float d = Mathf.Sqrt((x - 5) * (x - 5) + (y - 5) * (y - 5));
                if (d <= 4f) tex.SetPixel(x, y, new Color(0.4f, 0.5f, 0.6f, 1f));
                else tex.SetPixel(x, y, new Color(0, 0, 0, 0));
            }
        }
        tex.Apply();
        defaultHandSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f);
        return defaultHandSprite;
    }

    private static Sprite EnsureShadowSprite()
    {
        if (cachedShadowSprite != null) return cachedShadowSprite;

        int w = 32, h = 18;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2(w * 0.5f, h * 0.5f);
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = (x - center.x) / (w * 0.48f);
                float dy = (y - center.y) / (h * 0.48f);
                float dist = dx * dx + dy * dy;
                if (dist <= 1f)
                {
                    float alpha = Mathf.SmoothStep(1f, 0f, Mathf.Sqrt(dist));
                    tex.SetPixel(x, y, new Color(0f, 0f, 0f, alpha * 0.40f));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        cachedShadowSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f);
        return cachedShadowSprite;
    }

    public static Sprite CreateFallbackWeaponSprite(string type)
    {
        string t = type.ToLower();

        // 1. GRAVITY THUNDER HAMMER (Búa Trọng Lực khổng lồ - Unit-9)
        if (t.Contains("hammer"))
        {
            int w = 36, h = 24;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            ClearTexture(tex, w, h);

            // Cán búa bọc kim loại và da
            for (int x = 2; x <= 22; x++)
            {
                Color c = (x % 3 != 0) ? new Color(0.55f, 0.40f, 0.16f, 1f) : new Color(0.24f, 0.20f, 0.16f, 1f);
                tex.SetPixel(x, 11, c);
                tex.SetPixel(x, 12, c);
            }
            // Đốc chuôi búa mạ vàng
            for (int y = 10; y <= 13; y++) tex.SetPixel(2, y, new Color(0.85f, 0.70f, 0.15f, 1f));
            // Cổ búa thép
            for (int x = 20; x <= 23; x++)
                for (int y = 9; y <= 14; y++)
                    tex.SetPixel(x, y, new Color(0.35f, 0.35f, 0.40f, 1f));

            // Đầu búa khổng lồ (Khối thép đen beveled)
            for (int x = 23; x <= 34; x++)
                for (int y = 5; y <= 18; y++)
                    tex.SetPixel(x, y, new Color(0.20f, 0.20f, 0.25f, 1f));

            // Mặt nện búa mạ vàng và cạnh thép vát sáng
            for (int x = 24; x <= 33; x++) { tex.SetPixel(x, 5, new Color(0.5f, 0.5f, 0.6f, 1f)); tex.SetPixel(x, 18, new Color(0.12f, 0.12f, 0.15f, 1f)); }
            for (int y = 6; y <= 17; y++) { tex.SetPixel(23, y, new Color(0.5f, 0.5f, 0.6f, 1f)); tex.SetPixel(34, y, new Color(1f, 0.85f, 0.2f, 1f)); }

            // Lõi sấm sét phát quang rực rỡ bên trong đầu búa
            for (int x = 26; x <= 30; x++)
                for (int y = 8; y <= 15; y++)
                    tex.SetPixel(x, y, new Color(1f, 0.82f, 0.12f, 1f));

            // Tia chớp trắng giữa lõi
            for (int y = 9; y <= 11; y++) tex.SetPixel(28, y, Color.white);
            for (int y = 12; y <= 14; y++) tex.SetPixel(29, y, Color.white);

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.15f, 0.5f), 32f);
        }

        // 2. CYBER MURAMASA KATANA (Thanh đại kiếm Katana cong phong cách Nhật Bản - Unit-0)
        if (t.Contains("katana") || t.Contains("blade"))
        {
            int w = 40, h = 16;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            ClearTexture(tex, w, h);

            // Tsuka (Chuôi kiếm bọc vải đan caro đen-đỏ)
            for (int x = 2; x <= 9; x++)
            {
                Color wrapCol = (x % 2 == 0) ? new Color(0.1f, 0.08f, 0.1f, 1f) : new Color(0.85f, 0.15f, 0.25f, 1f);
                for (int y = 6; y <= 8; y++) tex.SetPixel(x, y, wrapCol);
            }
            tex.SetPixel(1, 7, new Color(0.9f, 0.75f, 0.2f, 1f)); // Kashira đốc kiếm mạ vàng

            // Tsuba (Vành chắn tay kim loại vàng)
            for (int y = 4; y <= 10; y++)
            {
                tex.SetPixel(10, y, new Color(0.9f, 0.75f, 0.2f, 1f));
                tex.SetPixel(11, y, new Color(0.7f, 0.55f, 0.15f, 1f));
            }

            // Lưỡi kiếm Katana cong vút lên trên thanh thoát, lưỡi cắt Plasma ở dưới
            for (int x = 12; x <= 37; x++)
            {
                int curve = (x < 24) ? 0 : ((x < 32) ? 1 : 2);
                int base_y = 7 + curve;

                tex.SetPixel(x, base_y + 1, new Color(0.28f, 0.26f, 0.32f, 1f)); // Mune (Sống kiếm thép xám ở trên)
                tex.SetPixel(x, base_y, new Color(0.95f, 0.95f, 1f, 1f));         // Thân lưỡi kiếm thép sáng bóng
                tex.SetPixel(x, base_y - 1, new Color(1f, 0.15f, 0.30f, 1f));     // Lưỡi cắt Plasma Muramasa rực đỏ ở dưới hướng chém
            }
            // Mũi nhọn vát cong vút lên (Kissaki)
            tex.SetPixel(38, 9, new Color(1f, 0.75f, 0.85f, 1f));
            tex.SetPixel(39, 10, Color.white);

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.15f, 0.44f), 32f);
        }

        // 3. NANO ENERGY DAGGERS (Cặp đoản đao song sát - Unit-7)
        if (t.Contains("dagger"))
        {
            int w = 30, h = 16;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            ClearTexture(tex, w, h);

            // Chuôi dao bọc cao su
            for (int x = 4; x <= 10; x++)
                for (int y = 7; y <= 9; y++)
                    tex.SetPixel(x, y, new Color(0.18f, 0.20f, 0.25f, 1f));
            tex.SetPixel(3, 8, new Color(0.4f, 0.5f, 0.6f, 1f));

            // Vành chắn dao
            for (int y = 5; y <= 11; y++)
                tex.SetPixel(11, y, new Color(0.45f, 0.55f, 0.65f, 1f));

            // Lưỡi dao tam giác vát sắc lẹm
            for (int x = 12; x <= 26; x++)
            {
                int half_w = Mathf.Max(1, (int)((26 - x) * 0.35f + 1));
                for (int dy = -half_w; dy <= half_w; dy++)
                {
                    int y = 8 + dy;
                    if (y >= 0 && y < h)
                    {
                        Color c = (dy == 0) ? Color.white : new Color(0f, 0.88f, 1f, 1f); // Lưỡi cắt Cyan rực rỡ
                        tex.SetPixel(x, y, c);
                    }
                }
            }
            tex.SetPixel(27, 8, Color.white);

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.20f, 0.5f), 32f);
        }

        // 4. HEAVY SCATTER CANNON (Đại bác phân mảnh 4 nòng hầm hố - Unit-9)
        if (t.Contains("scatter") || t.Contains("cannon") || t.Contains("shotgun"))
        {
            int w = 34, h = 18;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            ClearTexture(tex, w, h);

            // Báng súng hạng nặng
            for (int x = 3; x <= 7; x++)
                for (int y = 8; y <= 12; y++)
                    tex.SetPixel(x, y, new Color(0.22f, 0.18f, 0.15f, 1f));
            // Thân súng cơ bắp vàng đen
            for (int x = 8; x <= 19; x++)
                for (int y = 6; y <= 11; y++)
                    tex.SetPixel(x, y, new Color(0.30f, 0.28f, 0.26f, 1f));

            // Sọc cảnh báo Hazard Stripes vàng
            for (int x = 11; x <= 17; x += 3)
                for (int y = 7; y <= 10; y++)
                    tex.SetPixel(x, y, new Color(1f, 0.75f, 0.1f, 1f));

            // Tay cầm
            for (int x = 9; x <= 11; x++)
                for (int y = 12; y <= 15; y++)
                    tex.SetPixel(x, y, new Color(0.15f, 0.12f, 0.10f, 1f));

            // Cụm 4 nòng đại bác loe to
            for (int x = 20; x <= 29; x++)
                for (int y = 5; y <= 12; y++)
                    tex.SetPixel(x, y, new Color(0.42f, 0.40f, 0.38f, 1f));

            // Đầu 4 nòng súng rực lửa cam
            for (int x = 30; x <= 32; x++)
            {
                tex.SetPixel(x, 5, new Color(1f, 0.65f, 0f, 1f));
                tex.SetPixel(x, 7, new Color(1f, 0.65f, 0f, 1f));
                tex.SetPixel(x, 10, new Color(1f, 0.65f, 0f, 1f));
                tex.SetPixel(x, 12, new Color(1f, 0.65f, 0f, 1f));
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.24f, 0.5f), 32f);
        }

        // 5. VIPER LASER SMG (Tiểu liên laser Viper thon dài - Unit-0)
        if (t.Contains("smg") || t.Contains("machine") || t.Contains("viper"))
        {
            int w = 34, h = 16;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            ClearTexture(tex, w, h);

            // Báng gấp
            for (int y = 6; y <= 9; y++) { tex.SetPixel(4, y, new Color(0.18f, 0.15f, 0.20f, 1f)); tex.SetPixel(5, y, new Color(0.25f, 0.20f, 0.28f, 1f)); }
            // Thân SMG
            for (int x = 6; x <= 23; x++)
                for (int y = 5; y <= 8; y++)
                    tex.SetPixel(x, y, new Color(0.24f, 0.22f, 0.26f, 1f));
            // Tay cầm
            for (int y = 9; y <= 13; y++)
                for (int x = 9; x <= 11; x++)
                    tex.SetPixel(x, y, new Color(0.15f, 0.12f, 0.18f, 1f));
            // Hộp tiếp đạn cong đỏ
            for (int y = 9; y <= 14; y++)
                for (int x = 16; x <= 18; x++)
                    tex.SetPixel(x, y, new Color(0.85f, 0.12f, 0.25f, 1f));
            // Ống ngắm Holographic đỏ
            for (int x = 11; x <= 16; x++)
            {
                tex.SetPixel(x, 3, new Color(1f, 0.15f, 0.30f, 1f));
                tex.SetPixel(x, 4, new Color(0.25f, 0.22f, 0.28f, 1f));
            }
            // Nòng giảm thanh & Laser Diode
            for (int x = 24; x <= 30; x++)
            {
                tex.SetPixel(x, 5, new Color(1f, 0.10f, 0.25f, 1f)); // Tia laser
                tex.SetPixel(x, 6, new Color(0.38f, 0.35f, 0.40f, 1f));
                tex.SetPixel(x, 7, new Color(0.38f, 0.35f, 0.40f, 1f));
            }
            tex.SetPixel(31, 6, new Color(1f, 0.4f, 0.5f, 1f));
            tex.SetPixel(31, 7, new Color(1f, 0.4f, 0.5f, 1f));

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.24f, 0.5f), 32f);
        }

        // 6. DUAL PLASMA CARBINE (Súng trường Plasma nòng đôi công nghệ cao - Unit-7)
        {
            int w = 32, h = 16;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            ClearTexture(tex, w, h);

            // Báng & Tay cầm
            for (int y = 8; y <= 13; y++)
                for (int x = 6; x <= 9; x++)
                    tex.SetPixel(x, y, new Color(0.18f, 0.22f, 0.28f, 1f));

            // Thân súng công nghệ cao
            for (int y = 5; y <= 8; y++)
                for (int x = 8; x <= 19; x++)
                    tex.SetPixel(x, y, new Color(0.26f, 0.32f, 0.40f, 1f));

            // Pin năng lượng Plasma xanh Cyan
            for (int y = 9; y <= 11; y++)
                for (int x = 12; x <= 15; x++)
                    tex.SetPixel(x, y, new Color(0f, 0.88f, 1f, 1f));

            // Hai nòng bắn năng lượng song song
            for (int x = 20; x <= 27; x++)
            {
                tex.SetPixel(x, 5, new Color(0.45f, 0.55f, 0.65f, 1f));
                tex.SetPixel(x, 6, new Color(0f, 1f, 1f, 1f)); // Nòng trên
                tex.SetPixel(x, 7, new Color(0.45f, 0.55f, 0.65f, 1f));
                tex.SetPixel(x, 8, new Color(0f, 1f, 1f, 1f)); // Nòng dưới
            }
            // Đầu họng súng phát sáng
            for (int y = 5; y <= 8; y++)
            {
                tex.SetPixel(28, y, new Color(0f, 1f, 1f, 1f));
                tex.SetPixel(29, y, Color.white);
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.24f, 0.5f), 32f);
        }
    }

    private static void ClearTexture(Texture2D tex, int w, int h)
    {
        Color clear = new Color(0, 0, 0, 0);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, clear);
    }
}
