using System.Collections.Generic;
using UnityEngine;

public class ColorPiece : MonoBehaviour
{
    public enum ColorType
    {
        BLUE,
        GREEN,
        ORANGE,
        PURPLE,
        RED,
        YELLOW,
        ANY,
        COUNT
    };

    [System.Serializable]
    public struct ColorSprite
    {
        public ColorType color;
        public Sprite sprite;
    };
    public ColorSprite[] colorSprites;
    private Dictionary<ColorType, Sprite> colorSpriteDict;

    private ColorType color;
    public ColorType Color
    {
        get { return color; }
        set { SetColor(value); }
    }

    public int NumColors
    {
        get { return colorSprites.Length; }
    }
    private SpriteRenderer sprite;

    private void Awake()
    {
        colorSpriteDict = new Dictionary<ColorType, Sprite>();
        sprite = transform.Find("piece").GetComponent<SpriteRenderer>();
        for (int i = 0; i < colorSprites.Length; i++)
        {
            if (!colorSpriteDict.ContainsKey(colorSprites[i].color))
            {
                colorSpriteDict.Add(colorSprites[i].color, colorSprites[i].sprite);
            }
        }
    }

    void Start()
    {

    }


    void Update()
    {

    }

    public void SetColor(ColorType newColor)
    {
        color = newColor;
        if (colorSpriteDict.ContainsKey(newColor))
        {
            sprite.sprite = colorSpriteDict[newColor];
        }
    }

}
