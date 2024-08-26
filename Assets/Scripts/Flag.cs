using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flag : MonoBehaviour
{
    [SerializeField] private SpriteRenderer flagRenderer, poleRenderer;
    [SerializeField] private GameObject flagObject;
    [SerializeField] private int flagLevel = 4;
    [SerializeField] private bool side;

    public void setLayer(int x, int y, int _width, int _height)
    {
        //int _multiplier = GridTools.getLayerMultiplier();
        //int _width = GridManager.Instance.getWidth();
        //int _height = GridManager.Instance.getHeight();

        //flagRenderer.sortingOrder = ((_height - y - 1) * _width + 1) * _multiplier;
        //poleRenderer.sortingOrder = ((_height - y - 1) * _width + 1) * _multiplier;
        x++;
        y++;
        flagRenderer.sortingOrder = (_width + 1) * (_height - y) + ((2 * _width + 1) * GridTools.getLayerMultiplier()) * (_height - y) + (_width - x);
        poleRenderer.sortingOrder = (_width + 1) * (_height - y) + ((2 * _width + 1) * GridTools.getLayerMultiplier()) * (_height - y) + (_width - x);
    }

    public int getLevel()
    {
        return flagLevel;
    }

    public void flagDown()
    {
        flagLevel--;
        if(flagLevel > 0)
        {
            flagObject.transform.position -= new Vector3(0, 0.2f, 0);
        }
        else
        {
            Destroy(flagObject);
            GameManager.Instance.winGame(!side);
        }
    }
}
