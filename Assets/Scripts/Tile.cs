using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color _baseColor, _offsetColor;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private GameObject _highlight;

    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private bool mouseOnTile = false;

    public void Init(bool isOffset, int newX, int newY)
    {
        _renderer.color = isOffset ? _offsetColor : _baseColor;
        x = newX;
        y = newY;
    }

    void OnMouseEnter()
    {
        _highlight.SetActive(true);
        mouseOnTile = true;
    }

    void OnMouseExit()
    {
        _highlight.SetActive(false);
        mouseOnTile = false;
    }

    void Update()
    {
        if((Input.GetButtonDown("Fire1") && mouseOnTile) && !GridManager.Instance.IsMouseOverUI())
        {
            GridManager.Instance.OnTileClick(x, y);
        }
    }
}
