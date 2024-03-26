using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSpriteChanger : MonoBehaviour
{
    [SerializeField] Sprite NormalSprite;
    [SerializeField] Sprite hoverSprite;

    SpriteRenderer buttonSpriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        buttonSpriteRenderer = this.GetComponent<SpriteRenderer>();
    }

    public void hover()
    {
        buttonSpriteRenderer.sprite = hoverSprite;
    }

    public void unhover()
    {
        buttonSpriteRenderer.sprite = NormalSprite;
    }
}
