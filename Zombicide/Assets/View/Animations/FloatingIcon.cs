using UnityEngine;
using DG.Tweening;
public class FloatingIcon : MonoBehaviour
{
    private float floatHeight = 1f;
    private float duration = 0.8f;
    private float scaleStart = 0.2f;
    private float scaleEnd = 4f;

    private SpriteRenderer spriteRenderer;
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        transform.localScale = Vector3.one * scaleStart;

        Color color = spriteRenderer.color;
        color.a = 1f;
        spriteRenderer.color = color;

        transform.DOScale(scaleEnd, duration);

        transform.DOMoveY(transform.position.y + floatHeight, duration);

        spriteRenderer.DOFade(0f, duration * 0.6f)
            .SetEase(Ease.InQuad)
            .SetDelay(duration * 0.4f);
    }
}
