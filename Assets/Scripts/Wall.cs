using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;


public class Wall : MonoBehaviour
{
    public const int layerMultiplier = 50;
    public static readonly string[] wallMaterialNames = { "Bricks", "Cement" };
    public static readonly string[] wallInsideMaterialNames = { "Bricks", "Cement" };
    public enum wallType { hole = 0, wall, window, frame, halfwall };
    public enum wallTexture { brick = 0, cement };
    public enum wallInsideTexture { cement = 0, cementLight };
    public bool hidden;
    private SpriteRenderer[] midFrameRenderers, frontFrameRenderers, backFrameRenderers;
    private UnityEngine.U2D.Animation.SpriteResolver[] mainInsideParts, mainInParts, mainOutParts, frontInsideParts, frontOutParts, frontInParts, backInsideParts, backOutParts, backInParts;
    [SerializeField] private GameObject mainParts, frontParts, backParts, mainInside, mainIn, mainOut, frontInside, frontIn, frontOut, backInside, backIn, backOut, HitboxObjectPrefab, currentHitboxObject;
    [SerializeField] private wallType type;
    [SerializeField] private GridTools.WallInfo wallInfo;
    [SerializeField] private int height;

    public void spawnHitbox()
    {
        if(wallInfo.isVertical)
        {
            currentHitboxObject = Instantiate(HitboxObjectPrefab, new Vector3(wallInfo.x - 0.5f, wallInfo.y + 3 * height), Quaternion.identity);
        }
        else
        {
            currentHitboxObject = Instantiate(HitboxObjectPrefab, new Vector3(wallInfo.x, wallInfo.y - 0.5f + 3 * height), Quaternion.Euler(0, 0, 90f));
        }
    }

    public void setInfo(GridTools.WallInfo info)
    {
        wallInfo = info;
        string inTextureName = wallMaterialNames[(int)wallInfo.wallFront], outTextureName = wallMaterialNames[((int)wallInfo.wallBack)], insideTextureName = wallInsideMaterialNames[((int)wallInfo.wallInside)];
        for (int i = 0; i < mainInParts.Length; i++)
        {
            mainInParts[i].SetCategoryAndLabel(mainInParts[i].GetCategory(), inTextureName);
        }
        for (int i = 0; i < frontInParts.Length; i++)
        {
            frontInParts[i].SetCategoryAndLabel(frontInParts[i].GetCategory(), inTextureName);
        }
        for (int i = 0; i < backInParts.Length; i++)
        {
            backInParts[i].SetCategoryAndLabel(backInParts[i].GetCategory(), inTextureName);
        }
        for (int i = 0; i < mainOutParts.Length; i++)
        {
            mainOutParts[i].SetCategoryAndLabel(mainOutParts[i].GetCategory(), outTextureName);
        }
        for (int i = 0; i < frontOutParts.Length; i++)
        {
            frontOutParts[i].SetCategoryAndLabel(frontOutParts[i].GetCategory(), outTextureName);
        }
        for (int i = 0; i < backOutParts.Length; i++)
        {
            backOutParts[i].SetCategoryAndLabel(backOutParts[i].GetCategory(), outTextureName);
        }
        for (int i = 0; i < mainInsideParts.Length; i++)
        {
            mainInsideParts[i].SetCategoryAndLabel(mainInsideParts[i].GetCategory(), insideTextureName);
        }
        for (int i = 0; i < frontInsideParts.Length; i++)
        {
            frontInsideParts[i].SetCategoryAndLabel(frontInsideParts[i].GetCategory(), insideTextureName);
        }
        for (int i = 0; i < backInsideParts.Length; i++)
        {
            backInsideParts[i].SetCategoryAndLabel(backInsideParts[i].GetCategory(), insideTextureName);
        }
    }

    public wallType getWallType()
    {
        return type;
    }

    public void hide()
    {
        if(!hidden)
        {
            //spriteRenderer.color -= new Color(0, 0, 0, 0.50f);
            for (int i = 0; i < frontFrameRenderers.Length; i++)
            {
                frontFrameRenderers[i].color -= new Color(0, 0, 0, 0.50f);
            }
            for (int i = 0; i < midFrameRenderers.Length; i++)
            {
                midFrameRenderers[i].color -= new Color(0, 0, 0, 0.50f);
            }
            for (int i = 0; i < backFrameRenderers.Length; i++)
            {
                backFrameRenderers[i].color -= new Color(0, 0, 0, 0.50f);
            }
            hidden = true;
        }
    }

    public void appear()
    {
        if(hidden)
        {
            //spriteRenderer.color += new Color(0, 0, 0, 0.50f);
            for (int i = 0; i < frontFrameRenderers.Length; i++)
            {
                frontFrameRenderers[i].color += new Color(0, 0, 0, 0.50f);
            }
            for (int i = 0; i < midFrameRenderers.Length; i++)
            {
                midFrameRenderers[i].color += new Color(0, 0, 0, 0.50f);
            }
            for (int i = 0; i < backFrameRenderers.Length; i++)
            {
                backFrameRenderers[i].color += new Color(0, 0, 0, 0.50f);
            }
            hidden = false;
        }
    }

    public void setLayer(int _width, int _height)
    {
        height = _height;
        if (wallInfo.isVertical)
        {
            for (int i = 0; i < frontFrameRenderers.Length; i++)
            {
                frontFrameRenderers[i].sortingOrder = ((_width + 1) * (_height - wallInfo.y)) + (2 * _width + 1) * layerMultiplier * (_height - 1 - wallInfo.y) + (2 * (_width - wallInfo.x) + 1) * layerMultiplier;
            }
            for (int i = 0; i < midFrameRenderers.Length; i++)
            {
                midFrameRenderers[i].sortingOrder = (_width + 1) * (_height - wallInfo.y) + (2 * _width + 1) * layerMultiplier * (_height - 1 - wallInfo.y) + (2 * (_width - wallInfo.x) - 1) * layerMultiplier;
            }
            for (int i = 0; i < backFrameRenderers.Length; i++)
            {
                backFrameRenderers[i].sortingOrder = (_width + 1) * (_height - wallInfo.y) + (2 * _width + 1) * layerMultiplier * (_height - 1 - wallInfo.y);
            }
        }
        else
        {
            for (int i = 0; i < midFrameRenderers.Length; i++)
            {
                midFrameRenderers[i].sortingOrder = (_width + 1) * (_height - wallInfo.y) + ((2 * _width + 1) * layerMultiplier) * (_height - wallInfo.y) + (_width - wallInfo.x);
            }
        }
    }

    public void Awake()
    {
        if (MapEditor.Instance)
        {
            MapEditor.Instance.resetPreview += deleteWall;
        }
        backFrameRenderers = backParts.GetComponentsInChildren<SpriteRenderer>();
        midFrameRenderers = mainParts.GetComponentsInChildren<SpriteRenderer>();
        frontFrameRenderers = frontParts.GetComponentsInChildren<SpriteRenderer>();
        mainInsideParts = mainInside.GetComponentsInChildren<UnityEngine.U2D.Animation.SpriteResolver>();
        mainInParts = mainIn.GetComponentsInChildren<UnityEngine.U2D.Animation.SpriteResolver>();
        mainOutParts = mainOut.GetComponentsInChildren<UnityEngine.U2D.Animation.SpriteResolver>();
        frontInsideParts = frontInside.GetComponentsInChildren<UnityEngine.U2D.Animation.SpriteResolver>();
        frontInParts = frontIn.GetComponentsInChildren<UnityEngine.U2D.Animation.SpriteResolver>();
        frontOutParts = frontOut.GetComponentsInChildren<UnityEngine.U2D.Animation.SpriteResolver>();
        backInsideParts = backInside.GetComponentsInChildren<UnityEngine.U2D.Animation.SpriteResolver>();
        backInParts = backIn.GetComponentsInChildren<UnityEngine.U2D.Animation.SpriteResolver>();
        backOutParts = backOut.GetComponentsInChildren<UnityEngine.U2D.Animation.SpriteResolver>();
    }

    public void destroyWall()
    {
        Destroy(gameObject);
    }

    public void deleteWall(object sender, EventArgs e)
    {
        MapEditor.Instance.resetPreview -= deleteWall;
        Destroy(gameObject);
    }
}
