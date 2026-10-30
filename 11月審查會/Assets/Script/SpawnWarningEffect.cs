
using UnityEngine;
using System.Collections;

public class SpawnWarningEffect : MonoBehaviour
{
    [Header("Blink Settings")]
    [SerializeField] private float blinkInterval = 0.8f;
    [SerializeField] private float finalBlinkInterval = 0.15f;
    [SerializeField] private float finalBlinkDuration = 0.6f;

    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Play(float duration)
    {
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        blinkCoroutine = StartCoroutine(BlinkRoutine(duration));
    }

    private IEnumerator BlinkRoutine(float duration)
    {
        if (spriteRenderer == null)
            yield break;

        SetVisible(true);

        float normalDuration =
            Mathf.Max(0f, duration - finalBlinkDuration);

        float timer = 0f;

        // 正常等待階段：每 0.8 秒閃爍
        while (timer < normalDuration)
        {
            yield return new WaitForSeconds(blinkInterval);

            timer += blinkInterval;

            SetVisible(!spriteRenderer.enabled);
        }

        // 最後階段：快速閃爍兩下
        for (int i = 0; i < 2; i++)
        {
            SetVisible(false);
            yield return new WaitForSeconds(finalBlinkInterval);

            SetVisible(true);
            yield return new WaitForSeconds(finalBlinkInterval);
        }

        SetVisible(true);
        blinkCoroutine = null;
    }

    private void SetVisible(bool visible)
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = visible;
    }

    private void OnDestroy()
    {
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);
    }
}
