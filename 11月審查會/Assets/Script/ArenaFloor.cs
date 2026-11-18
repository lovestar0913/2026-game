using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ArenaFloor : MonoBehaviour
{
    public static ArenaFloor Instance { get; private set; }

    [Header("範圍")]
    [SerializeField, Label("地板寬度", 1f)] private float width = 40f;
    [SerializeField, Label("地板高度", 1f)] private float height = 24f;

    [Header("材質")]
    [SerializeField, Label("地板圖片")] private Texture2D floorTexture;
    [SerializeField, Label("地板 Material（優先使用）")] private Material floorMaterial;
    [SerializeField, Label("圖片重複單位（世界單位）", 0.1f)] private float tileSize = 4f;
    [SerializeField, Label("顏色")] private Color tint = Color.white;
    [SerializeField, Label("排序層級")] private int sortingOrder = -100;

    [Header("不規則邊緣")]
    [SerializeField, Label("邊緣凹凸最大深度", 0f)] private float edgeJitter = 1.5f;
    [SerializeField, Label("邊緣細緻度（越大越碎）", 0.01f)] private float noiseFrequency = 0.25f;
    [SerializeField, Label("邊緣取點間距", 0.1f)] private float segmentLength = 0.5f;
    [SerializeField, Label("隨機種子")] private int seed = 12345;
    [SerializeField, Label("每次開始都隨機形狀")] private bool randomizeSeed = false;

    // 地板的標準範圍（鏡頭用它來限制移動）
    public Rect Rect => new Rect(
        (Vector2)transform.position - new Vector2(width, height) * 0.5f,
        new Vector2(width, height));
    // 保證整塊都有地板的安全範圍（已扣掉不規則邊緣的最大深度）
    public Rect SafeRect
    {
        get
        {
            float inset = edgeJitter + 0.1f;
            Rect full = Rect;

            return new Rect(
                full.xMin + inset,
                full.yMin + inset,
                Mathf.Max(0f, full.width - inset * 2f),
                Mathf.Max(0f, full.height - inset * 2f));
        }
    }

    private void Awake()
    {
        Instance = this;
        Build();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Build()
    {
        float halfW = width * 0.5f;
        float halfH = height * 0.5f;
        float inset = edgeJitter + 0.1f;           // 內圈往內縮，保證不會和外圈交錯
        float perimeter = 2f * (width + height);

        int usedSeed = randomizeSeed ? Random.Range(0, 100000) : seed;
        float noiseOffset = 1000f + Mathf.Abs(usedSeed) * 0.137f;

        // 四個角落，逆時針
        Vector2[] corners =
        {
            new Vector2(-halfW, -halfH),
            new Vector2( halfW, -halfH),
            new Vector2( halfW,  halfH),
            new Vector2(-halfW,  halfH)
        };

        List<Vector2> outer = new List<Vector2>();   // 不規則的外緣
        List<Vector2> inner = new List<Vector2>();   // 規則的內圈
        float travelled = 0f;

        for (int e = 0; e < 4; e++)
        {
            Vector2 a = corners[e];
            Vector2 b = corners[(e + 1) % 4];
            Vector2 prevA = corners[(e + 3) % 4];

            Vector2 dir = (b - a).normalized;
            Vector2 prevDir = (a - prevA).normalized;

            // 逆時針時，邊方向向左轉 90 度就是內側
            Vector2 inward = new Vector2(-dir.y, dir.x);
            Vector2 prevInward = new Vector2(-prevDir.y, prevDir.x);

            float edgeLength = Vector2.Distance(a, b);
            int steps = Mathf.Max(1, Mathf.RoundToInt(edgeLength / segmentLength));
            float stepLength = edgeLength / steps;

            for (int s = 0; s < steps; s++)
            {
                Vector2 basePoint = a + dir * (stepLength * s);
                float depth = edgeJitter * SampleNoise(travelled, perimeter, noiseOffset);

                if (s == 0)
                {
                    // 角落：同時往兩個方向的內側位移
                    outer.Add(basePoint + (inward + prevInward) * depth);
                    inner.Add(basePoint + (inward + prevInward) * inset);
                }
                else
                {
                    outer.Add(basePoint + inward * depth);
                    inner.Add(basePoint + inward * inset);
                }

                travelled += stepLength;
            }
        }

        int count = outer.Count;
        int centerIndex = count * 2;

        Vector3[] vertices = new Vector3[count * 2 + 1];
        Vector2[] uvs = new Vector2[vertices.Length];

        for (int i = 0; i < count; i++)
        {
            vertices[i] = outer[i];
            vertices[count + i] = inner[i];
        }

        vertices[centerIndex] = Vector3.zero;

        // UV 依位置計算，圖片會平鋪
        for (int i = 0; i < vertices.Length; i++)
            uvs[i] = new Vector2(vertices[i].x / tileSize, vertices[i].y / tileSize);

        List<int> triangles = new List<int>();

        for (int i = 0; i < count; i++)
        {
            int next = (i + 1) % count;
            int o0 = i;
            int o1 = next;
            int i0 = count + i;
            int i1 = count + next;

            // 外緣到內圈的帶狀區域
            triangles.Add(o0); triangles.Add(i1); triangles.Add(o1);
            triangles.Add(o0); triangles.Add(i0); triangles.Add(i1);

            // 內圈填滿
            triangles.Add(centerIndex); triangles.Add(i1); triangles.Add(i0);
        }

        Mesh mesh = new Mesh();
        mesh.name = "ArenaFloorMesh";
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = mesh;

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = CreateMaterial();
        meshRenderer.sortingOrder = sortingOrder;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
    }

    private Material CreateMaterial()
    {
        if (floorMaterial != null)
            return floorMaterial;

        if (floorTexture != null)
            floorTexture.wrapMode = TextureWrapMode.Repeat;

        Material material = new Material(Shader.Find("Sprites/Default"));
        material.mainTexture = floorTexture;
        material.color = tint;
        return material;
    }

    // 沿著周長取雜訊（首尾相連，不會在接縫處出現斷層），回傳 0~1
    private float SampleNoise(float travelled, float perimeter, float offset)
    {
        float angle = travelled / perimeter * Mathf.PI * 2f;
        float radius = perimeter * noiseFrequency / (Mathf.PI * 2f);

        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);

        float large = Mathf.PerlinNoise(
            cos * radius + offset,
            sin * radius + offset);

        float small = Mathf.PerlinNoise(
            cos * radius * 4f + offset + 50f,
            sin * radius * 4f + offset + 50f);

        float mixed = large * 0.7f + small * 0.3f;

        return Mathf.InverseLerp(0.25f, 0.75f, mixed);
    }

    // 在 Scene 視窗顯示地板的標準範圍
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(width, height, 0f));
    }
}