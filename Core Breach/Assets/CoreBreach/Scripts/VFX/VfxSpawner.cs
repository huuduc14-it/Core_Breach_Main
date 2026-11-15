using UnityEngine;

public enum VfxType
{
    BulletImpact,    // tóe tia lửa khi đạn va chạm tường/quái
    EnemyExplosion,  // nổ khi kẻ địch chết
    MetalShards,     // mảnh kim loại
    OverheatSmoke    // khói cảnh báo Overheat
}

/// <summary>
/// Sinh hiệu ứng hạt (Particle System) bằng code - KHÔNG cần tạo prefab hay thêm object vào scene.
/// Dùng: VfxSpawner.Spawn(VfxType.BulletImpact, hitPoint);
/// </summary>
public static class VfxSpawner
{
    private static Material sharedMaterial;

    public static void Spawn(VfxType type, Vector3 position)
    {
        position.z = 0f;

        Color c1, c2;
        int count;
        float speed, life, size, gravity, radius;

        switch (type)
        {
            case VfxType.EnemyExplosion:
                c1 = new Color(1f, 0.75f, 0.2f); c2 = new Color(1f, 0.3f, 0.1f);
                count = 24; speed = 3.5f; life = 0.5f; size = 0.14f; gravity = 0f; radius = 0.15f;
                break;

            case VfxType.MetalShards:
                c1 = new Color(0.8f, 0.85f, 0.95f); c2 = new Color(0.45f, 0.5f, 0.6f);
                count = 14; speed = 4.5f; life = 0.6f; size = 0.09f; gravity = 1.5f; radius = 0.1f;
                break;

            case VfxType.OverheatSmoke:
                c1 = new Color(0.75f, 0.75f, 0.75f, 0.7f); c2 = new Color(0.35f, 0.35f, 0.4f, 0.5f);
                count = 5; speed = 0.8f; life = 0.9f; size = 0.22f; gravity = -0.3f; radius = 0.1f;
                break;

            default: // BulletImpact
                c1 = new Color(1f, 0.95f, 0.5f); c2 = new Color(1f, 0.55f, 0.15f);
                count = 8; speed = 3f; life = 0.25f; size = 0.07f; gravity = 0f; radius = 0.03f;
                break;
        }

        GameObject go = new GameObject("VFX_" + type);
        go.transform.position = position;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 0.5f;
        main.startLifetime = life;
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.5f, speed);
        main.startSize = new ParticleSystem.MinMaxCurve(size * 0.6f, size);
        main.startColor = new ParticleSystem.MinMaxGradient(c1, c2);
        main.gravityModifier = gravity;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 64;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, (short)count) });

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = radius;

        ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient fade = new Gradient();
        fade.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        col.color = fade;

        ParticleSystem.SizeOverLifetimeModule sz = ps.sizeOverLifetime;
        sz.enabled = true;
        sz.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.2f));

        ParticleSystemRenderer r = go.GetComponent<ParticleSystemRenderer>();
        r.material = GetMaterial();
        r.sortingOrder = 50;

        ps.Play();
        Object.Destroy(go, life + 0.7f);
    }

    private static Material GetMaterial()
    {
        if (sharedMaterial != null) return sharedMaterial;

        // Sprites/Default luôn có trong build 2D; Particles/Standard Unlit làm phương án dự phòng
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");

        if (shader != null) sharedMaterial = new Material(shader);
        return sharedMaterial;
    }
}
