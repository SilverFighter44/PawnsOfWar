using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
    [SerializeField] private Toggle outlinesToggle, crouchToogle;
    [SerializeField] private TextWriter UnitControlsHP, UnitControlsNumber, UnitControlsMoves, clockCounter;
    [SerializeField] private UI_ClassSymbol UnitControlsSymbol;
    [SerializeField] private bool crouchFunctionOn = false;
    [SerializeField] private List<Sprite> gadgetSprites;
    [SerializeField] private Image gadget1Preview, gadget2Preview;
    [SerializeField] private GameObject CanvasObject, infoPrefab, optionsMenu;
    [SerializeField] private Transform infoPosition;
    [SerializeField] private List<UnitListedInfo> unitListedInfos;

    private void Start()
    {
        GridManager.Instance.displayUnitInfoEvent += SetUnitInfoDisplay;
        GridManager.Instance.updateUnitsInfoList += UpdateUnitsIcons;
        PrepareUnitsIcons();
        optionsMenu.SetActive(false);
    }

    public void toOptions()
    {
        if (optionsMenu.activeSelf)
        {
            optionsMenu.SetActive(false);
        }
        else
        {
            optionsMenu.SetActive(true);
        }
    }

    public void backToMenu()
    {
        SceneManager.LoadScene(1);
    }

    public void PrepareUnitsIcons()
    {
        int teamSize = StartData.Instance.getData().BlueTeam.Length;
        for (int i = 0; i < teamSize; i++)
        {
            GameObject currentIcon = Instantiate(infoPrefab, infoPosition.position , Quaternion.identity); // 30 = info height
            //parent to canvas
            currentIcon.transform.SetParent(CanvasObject.transform, false);
            currentIcon.GetComponent<RectTransform>().position = infoPosition.position + new Vector3(0f, CanvasObject.transform.localScale.x * 60f * i, 0f);
            unitListedInfos.Add(currentIcon.GetComponent<UnitListedInfo>());
            unitListedInfos[i].unitClass.ChangeIcon(StartData.Instance.getData().BlueTeam[i].UnitRole, UI_ClassSymbol.IconCategory.Blue);
            unitListedInfos[i].unitNumber.ChangeNumber(StartData.Instance.getData().BlueTeam[i].UnitNumber, UI_ClassSymbol.IconCategory.Blue);
            unitListedInfos[i].unitMovesCount.updateMovesCount(GridManager.Instance.getBlueTeam()[i].GetComponent<Unit>().howManyMoves());
        }
    }

    public void UpdateUnitsIcons(object sender, EventArgs e)
    {
        for (int i = 0; i < unitListedInfos.Count; i++)
        {
            if(GridManager.Instance.whatSide())
            {
                unitListedInfos[i].unitClass.ChangeIcon(StartData.Instance.getData().BlueTeam[i].UnitRole, UI_ClassSymbol.IconCategory.Blue);
                unitListedInfos[i].unitNumber.ChangeNumber(StartData.Instance.getData().BlueTeam[i].UnitNumber, UI_ClassSymbol.IconCategory.Blue);
                if (GridManager.Instance.getBlueTeam()[i])
                {
                    unitListedInfos[i].unitMovesCount.updateMovesCount(GridManager.Instance.getBlueTeam()[i].GetComponent<Unit>().howManyMoves());
                }
                else
                {
                    unitListedInfos[i].unitMovesCount.updateMovesCount(-1);
                }
            }
            else
            {
                unitListedInfos[i].unitClass.ChangeIcon(StartData.Instance.getData().RedTeam[i].UnitRole, UI_ClassSymbol.IconCategory.Red);
                unitListedInfos[i].unitNumber.ChangeNumber(StartData.Instance.getData().RedTeam[i].UnitNumber, UI_ClassSymbol.IconCategory.Red);
                if (GridManager.Instance.getRedTeam()[i])
                {
                    unitListedInfos[i].unitMovesCount.updateMovesCount(GridManager.Instance.getRedTeam()[i].GetComponent<Unit>().howManyMoves());
                }
                else
                {
                    unitListedInfos[i].unitMovesCount.updateMovesCount(-1);
                }
            }
        }
    }

    public void ReloadButtton()
    {
        GridManager.Instance.selectedUnit.reload();
        GridManager.Instance.UpdateMovesCount();
    }

    public void CrouchButtton()
    {
        if (crouchFunctionOn)
        {
            GridManager.Instance.selectedUnit.crouch();
            GridManager.Instance.ResetHighlights();
        }
    }

    public void Gadget1Buttton()
    {
        GridManager.Instance.selectedUnit.useGadget1();
        GridManager.Instance.ResetHighlights();
    }

    public void Gadget2Buttton()
    {
        GridManager.Instance.selectedUnit.useGadget2();
        GridManager.Instance.ResetHighlights();
    }

    private void SetUnitInfoDisplay(object sender, EventArgs e)
    {
        crouchFunctionOn = false;
        Unit.UnitInfo _info = GridManager.Instance.GetSelectedUnit().GetInfoToDisplay();
        UnitControlsHP.showMessage(_info.hp.ToString());
        UnitControlsNumber.showMessage(_info.number.ToString());
        UnitControlsSymbol.ChangeIcon(_info.role, _info.team);
        UnitControlsMoves.showMessage(_info.movesCount.ToString());
        gadget1Preview.sprite = gadgetSprites[(int)_info.g1];
        gadget2Preview.sprite = gadgetSprites[(int)_info.g2];
        if (_info.crouched && !crouchToogle.isOn)
        {
            crouchToogle.isOn = true;
        }
        else if (!_info.crouched && crouchToogle.isOn)
        {
            crouchToogle.isOn = false;
        }
        crouchFunctionOn = true;
    }

    public void changeTeamOutlines()
    {
        GameObject[] blueTeam = GridManager.Instance.getBlueTeam(), redTeam = GridManager.Instance.getRedTeam();
        for(int i = 0; i < blueTeam.Length; i++)
        {
            if(blueTeam[i])
            {
                if (outlinesToggle.isOn)
                {
                    blueTeam[i].GetComponent<Unit>().showOutlines();
                }
                else
                { 
                    blueTeam[i].GetComponent<Unit>().hideOutlines(); 
                }
            }
        }
        for (int i = 0; i < redTeam.Length; i++)
        {
            if (redTeam[i])
            {
                if (outlinesToggle.isOn)
                {
                    redTeam[i].GetComponent<Unit>().showOutlines();
                }
                else
                {
                    redTeam[i].GetComponent<Unit>().hideOutlines();
                }
            }
        }
    }
}