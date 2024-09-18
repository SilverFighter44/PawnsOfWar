using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Buttons : MonoBehaviour
{
    [SerializeField] private Toggle outlinesToggle;

    public static void ReloadButtton()
    {
        GridManager.Instance.selectedUnit.reload();
        GridManager.Instance.UpdateMovesCount();
    }

    public static void CrouchButtton()
    {
        GridManager.Instance.selectedUnit.crouch();
        GridManager.Instance.ResetHighlights();
    }

    public static void Gadget1Buttton()
    {
        GridManager.Instance.selectedUnit.useGadget1();
        GridManager.Instance.ResetHighlights();
    }

    public static void Gadget2Buttton()
    {
        GridManager.Instance.selectedUnit.useGadget2();
        GridManager.Instance.ResetHighlights();
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