using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlight : MonoBehaviour
{
    [SerializeField] private int highlightX, highlightY, range = 0;

    private void Start()
    {
        GridManager.Instance.destroyHighlights += deleteHighlight;
    }

    public void setRange( int range)
    {
        this.range = range;
    }

    public int getRange()
    {
        return range;
    }

    public void setCoordinates( int _x, int _y)
    {
        highlightX = _x;
        highlightY = _y;
        GridManager.Instance.HideWall(highlightX, highlightY);
    }

    public void deleteHighlight(object sender, EventArgs e)
    {
        GridManager.Instance.destroyHighlights -= deleteHighlight;
        GridManager.Instance.UnHideWall(highlightX, highlightY);
        Destroy(gameObject);
    }
}
