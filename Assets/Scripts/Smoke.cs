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

    public void setCoordinates(int x, int y)
    {
        SmokeRenderer.sortingOrder = GridTools.OnGridObjectLayer(GridManager.Instance.getWidth(), GridManager.Instance.getHeight(), x, y) + GridTools.getLayerMultiplier() - 1;
    }

    public void deleteHighlight(object sender, EventArgs e)
    {
        GridManager.Instance.nextTurn -= deleteHighlight;
        Destroy(gameObject);
    }
}
