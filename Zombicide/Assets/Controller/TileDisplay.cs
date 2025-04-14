using UnityEngine;

public class TileDisplay : MonoBehaviour
{
   
    private SpriteRenderer spriteRenderer;

    public void Setup(Sprite tileSprite, Vector3 position, float rotation)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = tileSprite;

        transform.position = position;
        transform.rotation = Quaternion.Euler(90, 0, rotation);

    }

}
