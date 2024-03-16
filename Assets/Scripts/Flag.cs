using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour
{
    [SerializeField] private SpriteRenderer flagRenderer, poleRenderer;

    public void setLayer(int y)
    {
        int _multiplier = StartData.Instance.getLayerMultiplier();
        int _width = GridManager.Instance.getWidth();
        int _height = GridManager.Instance.getHeight();

        flagRenderer.sortingOrder = ((_height - y - 1) * _width + 1) * _multiplier;
        //poleRenderer.sortingOrder = ((_height - y - 1) * _width + 1) * _multiplier;
    }

}
