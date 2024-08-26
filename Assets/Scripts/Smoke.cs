using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smoke : MonoBehaviour
{
    [SerializeField] private SpriteRenderer SmokeRenderer;

    private void Start()
    {
        GridManager.Instance.nextTurn += deleteHighlight;
    }

    public void setCoordinates(int _y, int _x)
    {
        SmokeRenderer.sortingOrder = GridTools.getLayerMultiplier() - 1 + GridTools.getLayerMultiplier() * (((GridManager.Instance.getHeight() - _x - 1) * GridManager.Instance.getWidth() + GridManager.Instance.getWidth() - _y - 2) + 2);
    }

    public void deleteHighlight(object sender, EventArgs e)
    {
        GridManager.Instance.nextTurn -= deleteHighlight;
        Destroy(gameObject);
    }
}
