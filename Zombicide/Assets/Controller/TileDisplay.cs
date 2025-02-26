using UnityEngine;

public class TileDisplay : MonoBehaviour
{
   
    private SpriteRenderer spriteRenderer;

    public void Setup(Sprite tileSprite, Vector3 position, float rotation)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = tileSprite;

        // Beállítjuk a helyzetet és forgatást
        transform.position = position;
        transform.rotation = Quaternion.Euler(90, 0, rotation);
        //transform.localScale = new Vector3(-0.2f, 0, 0);
    }

    private void OnMouseDown()
    {
        Debug.Log("Tile kattintva: " + gameObject.name);
    }

}
