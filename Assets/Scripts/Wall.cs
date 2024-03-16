using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public int wallType;
    public bool hidden;
    [SerializeField] private SpriteRenderer spriteRenderer, frontFrameSpriteRenderer;

    public void hide()
    {
        if(!hidden)
        {
            spriteRenderer.color -= new Color(0, 0, 0, 0.50f);
            hidden = true;
        }
    }

    public void appear()
    {
        if(hidden)
        {
            spriteRenderer.color += new Color(0, 0, 0, 0.50f);
            hidden = false;
        }
    }
 
    public void setLayer(int x, int y, bool isHorizontal)
    {
        int _multiplier = StartData.Instance.getLayerMultiplier();
        int _width = GridManager.Instance.getWidth();
        int _height = GridManager.Instance.getHeight();
        if(isHorizontal)
        {
            if(frontFrameSpriteRenderer)
            {
                frontFrameSpriteRenderer.sortingOrder = (((_height - x - 1) * _width + _width - y - 2) + 2 ) * _multiplier;
                spriteRenderer.sortingOrder = ((_height - x - 1) * _width + 1) * _multiplier;
            }
            else
            {
                spriteRenderer.sortingOrder = (((_height - x - 1) * _width + _width - y - 2) + 2 ) * _multiplier;
            }
        }
        else
        {
            spriteRenderer.sortingOrder = ((_height - x - 1) * _width + 1) * _multiplier;
        }
    }
}
