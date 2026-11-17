using UnityEngine;

// 把整個 Prefab 拍成一張透明背景的圖片
public static class PrefabSnapshot
{
    private const int TextureSize = 512;
    private const float Padding = 1.1f;   // 四周留一點空白
    private static readonly Vector3 StudioPosition = new Vector3(5000f, 5000f, 0f);

    public static Sprite Render(GameObject prefab)
    {
        if (prefab == null)
            return null;

        // 放在停用的父物件底下生成，避免腳本的 Awake 被執行
        GameObject studio = new GameObject("PrefabSnapshotStudio");
        studio.SetActive(false);
        studio.transform.position = StudioPosition;

        GameObject instance = Object.Instantiate(prefab, studio.transform);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;

        StripGameplayComponents(instance);

        // 啟用後才能取得正確的範圍
        studio.SetActive(true);

        Animator animator = instance.GetComponentInChildren<Animator>();
        if (animator != null)
            animator.Update(0f);   // 套用動畫第一格

        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
        Bounds bounds = default;
        bool hasBounds = false;

        foreach (Renderer renderer in renderers)
        {
            if (!renderer.enabled)
                continue;

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        if (!hasBounds)
        {
            Object.Destroy(studio);
            return null;
        }

        // 臨時攝影機：只拍這個 Prefab（它在很遠的地方，別的物件拍不到）
        GameObject cameraObject = new GameObject("PrefabSnapshotCamera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.enabled = false;
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0f, 0f, 0f, 0f);   // 透明背景
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 100f;

        Vector3 center = bounds.center;
        cameraObject.transform.position = new Vector3(center.x, center.y, center.z - 10f);
        camera.orthographicSize =
            Mathf.Max(0.01f, Mathf.Max(bounds.extents.x, bounds.extents.y) * Padding);

        RenderTexture renderTexture = RenderTexture.GetTemporary(
            TextureSize, TextureSize, 24, RenderTextureFormat.ARGB32);

        camera.targetTexture = renderTexture;
        camera.Render();

        // 把畫面讀回成一般圖片
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;

        Texture2D texture = new Texture2D(
            TextureSize, TextureSize, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, TextureSize, TextureSize), 0, 0);
        texture.Apply();

        RenderTexture.active = previous;
        camera.targetTexture = null;
        RenderTexture.ReleaseTemporary(renderTexture);

        Object.Destroy(cameraObject);
        Object.Destroy(studio);

        return Sprite.Create(
            texture,
            new Rect(0, 0, TextureSize, TextureSize),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }

    // 拿掉遊戲邏輯，只留外觀
    private static void StripGameplayComponents(GameObject instance)
    {
        foreach (MonoBehaviour behaviour in instance.GetComponentsInChildren<MonoBehaviour>(true))
            Object.DestroyImmediate(behaviour);

        foreach (AudioSource audio in instance.GetComponentsInChildren<AudioSource>(true))
            Object.DestroyImmediate(audio);

        foreach (Collider2D collider in instance.GetComponentsInChildren<Collider2D>(true))
            Object.DestroyImmediate(collider);

        foreach (Rigidbody2D body in instance.GetComponentsInChildren<Rigidbody2D>(true))
            Object.DestroyImmediate(body);
    }
}