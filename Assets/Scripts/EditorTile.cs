using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorTile : MonoBehaviour
{
    [SerializeField] private Color _baseColor, _offsetColor;
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private GameObject _highlight;
    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private bool mouseOnTile = false;
    private enum editingMode { placeTiles, placeWalls_V_U, placeWalls_V_D, placeWalls_H_L, placeWalls_H_R };
    editingMode currentEditingMode;

    private void Start()
    {
        MapEditor.Instance.clearMap += deleteTile;
    }

    public void deleteTile(object sender, EventArgs e)
    {
        MapEditor.Instance.clearMap -= deleteTile;
        Destroy(gameObject);
    }

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
        if ((Input.GetButtonDown("Fire1") && mouseOnTile) && !GridManager.IsMouseOverUI())
        {
            MapEditor.Instance.OnTileClick(x, y);
        }
    }
}
