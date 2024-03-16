using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChoiceMenuManager : MonoBehaviour
{
    public static ChoiceMenuManager Instance;

    [SerializeField] private GameObject UnitPrefab;
    [SerializeField] private GameObject B1pos, B2pos, B3pos, B4pos, R1pos, R2pos, R3pos, R4pos, I_pos, II_pos, III_pos, IV_pos;
    [SerializeField] private UnityEngine.U2D.Animation.SpriteResolver roleChoicePreview, gadget1ChoicePreview, gadget2ChoicePreview, gameModePreview, sideBPreview, sideRPrewiev;
    [SerializeField] private bool activeTeam;
    [SerializeField] private Unit.number activeNumber;
    [SerializeField] private StartData.GameSettingsData dataEdit;
    [SerializeField] private Unit[] unitsEdited = new Unit[4];
    public event EventHandler resetPreview;
    [SerializeField] private Unit.role _roleIcon;
    [SerializeField] private Unit.gadget _gadget1, _gadget2;
    [SerializeField] private Unit.gadget[] InfantrymanGadgets1 = { Unit.gadget.grenade, Unit.gadget.smoke }; 
    [SerializeField] private Unit.gadget[] InfantrymanGadgets2 = { Unit.gadget.grenade, Unit.gadget.smoke };
    [SerializeField] private Unit.gadget[] RiflemanGadgets1 = { Unit.gadget.grenade, Unit.gadget.smoke };
    [SerializeField] private Unit.gadget[] RiflemanGadgets2 = { Unit.gadget.grenade, Unit.gadget.smoke };
    [SerializeField] private StartData.gameMode _gameMode;


    public void SetEditTeam( bool n )
    {
        activeTeam = n;
    }

    public void SetEditUnit( int n )
    {
        activeNumber = ( Unit.number )n;
        dataEdit = StartData.Instance.getData();
        int temp;
        if (activeTeam)
        {
            temp = (int)dataEdit.BlueTeam[n].UnitRole;
        }
        else
        {
            temp = (int)dataEdit.RedTeam[n].UnitRole;
        }
        _roleIcon = (Unit.role)temp;
        _gadget1 = dataEdit.BlueTeam[n].gadget1;
        _gadget2 = dataEdit.BlueTeam[n].gadget2;
        roleChoicePreview.SetCategoryAndLabel(roleChoicePreview.GetCategory(), _roleIcon.ToString());
        gadget1ChoicePreview.SetCategoryAndLabel(gadget1ChoicePreview.GetCategory(), _gadget1.ToString());
        gadget2ChoicePreview.SetCategoryAndLabel(gadget2ChoicePreview.GetCategory(), _gadget2.ToString());
    }

    public void changeGameMode()
    {
        _gameMode++;
        if (_gameMode == StartData.gameMode.end)
        {
            _gameMode = 0;
        }
        switch (_gameMode)
        {
            case StartData.gameMode.defence:
                gameModePreview.SetCategoryAndLabel(gameModePreview.GetCategory(), "Defend_Symetric");
                sideBPreview.SetCategoryAndLabel(sideBPreview.GetCategory(), "DefenceB");
                sideRPrewiev.SetCategoryAndLabel(sideRPrewiev.GetCategory(), "DefenceR");
                break;
            case StartData.gameMode.defenceR:
                gameModePreview.SetCategoryAndLabel(gameModePreview.GetCategory(), "Defend_Red");
                sideBPreview.SetCategoryAndLabel(sideBPreview.GetCategory(), "AttackB");
                sideRPrewiev.SetCategoryAndLabel(sideRPrewiev.GetCategory(), "DefenceR");
                break;
            case StartData.gameMode.defenceB:
                gameModePreview.SetCategoryAndLabel(gameModePreview.GetCategory(), "Defend_Blue");
                sideBPreview.SetCategoryAndLabel(sideBPreview.GetCategory(), "DefenceB");
                sideRPrewiev.SetCategoryAndLabel(sideRPrewiev.GetCategory(), "AttackR");
                break;
            case StartData.gameMode.attack:
                gameModePreview.SetCategoryAndLabel(gameModePreview.GetCategory(), "DeathMatch");
                sideBPreview.SetCategoryAndLabel(sideBPreview.GetCategory(), "AttackB");
                sideRPrewiev.SetCategoryAndLabel(sideRPrewiev.GetCategory(), "AttackR");
                break;
            default:
                break;
        }
        dataEdit.GameMode = _gameMode;
        StartData.Instance.UpdateData(dataEdit);
       // StartData.Instance.UpdateData(dataEdit);
    }

        public void changeRole()
    {
        _roleIcon++;

        if(_roleIcon == Unit.role.end )
        {
            _roleIcon = 0;
        }
        int temp = (int)_roleIcon;
        roleChoicePreview.SetCategoryAndLabel(roleChoicePreview.GetCategory(), _roleIcon.ToString());
        switch(activeNumber)
        {
            case Unit.number.I:
                I_pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel(I_pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().GetCategory(), _roleIcon.ToString());
                break;
            case Unit.number.II:
                II_pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel(II_pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().GetCategory(), _roleIcon.ToString());
                break;
            case Unit.number.III:
                III_pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel(III_pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().GetCategory(), _roleIcon.ToString());
                break;
            case Unit.number.IV:
                IV_pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel(IV_pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().GetCategory(), _roleIcon.ToString());
                break;
            default:
                break;
        }

        if(activeTeam)
        {
            dataEdit.BlueTeam[(int)activeNumber].UnitRole = (Unit.role)temp;
            switch (_roleIcon)
            {
                case Unit.role.Infantryman:
                    dataEdit.BlueTeam[(int)activeNumber].gadget1 = InfantrymanGadgets1[0];
                    dataEdit.BlueTeam[(int)activeNumber].gadget2 = InfantrymanGadgets2[0];
                    _gadget1 = InfantrymanGadgets1[0];
                    _gadget2 = InfantrymanGadgets2[0];
                    break;
                case Unit.role.Rifleman:
                    dataEdit.BlueTeam[(int)activeNumber].gadget1 = RiflemanGadgets1[0];
                    dataEdit.BlueTeam[(int)activeNumber].gadget2 = RiflemanGadgets2[0];
                    _gadget1 = RiflemanGadgets1[0];
                    _gadget2 = RiflemanGadgets2[0];
                    break;
                default:
                    break;
            }
            unitsEdited[(int)activeNumber].SetData(dataEdit.BlueTeam[(int)activeNumber]);
        }
        else
        {
            dataEdit.RedTeam[(int)activeNumber].UnitRole = (Unit.role)temp;
            switch (_roleIcon)
            {
                case Unit.role.Infantryman:
                    dataEdit.RedTeam[(int)activeNumber].gadget1 = InfantrymanGadgets1[0];
                    dataEdit.RedTeam[(int)activeNumber].gadget2 = InfantrymanGadgets2[0];
                    _gadget1 = InfantrymanGadgets1[0];
                    _gadget2 = InfantrymanGadgets2[0];
                    break;
                case Unit.role.Rifleman:
                    dataEdit.RedTeam[(int)activeNumber].gadget1 = RiflemanGadgets1[0];
                    dataEdit.RedTeam[(int)activeNumber].gadget2 = RiflemanGadgets2[0];
                    _gadget1 = RiflemanGadgets1[0];
                    _gadget2 = RiflemanGadgets2[0];
                    break;
                default:
                    break;
            }
            unitsEdited[(int)activeNumber].SetData(dataEdit.RedTeam[(int)activeNumber]);
        }
        gadget1ChoicePreview.SetCategoryAndLabel(gadget1ChoicePreview.GetCategory(), _gadget1.ToString());
        gadget2ChoicePreview.SetCategoryAndLabel(gadget2ChoicePreview.GetCategory(), _gadget2.ToString());
        StartData.Instance.UpdateData(getUnitsSettings());
    }

    int findGadgetIndex(Unit.gadget[] gadgetsGroup, Unit.gadget _gadget)
    {
        for(int i = 0; i < gadgetsGroup.Length; i++)
        {
            if(gadgetsGroup[i] == _gadget)
            {
                return i;
            }
        }
        return 0;
    }

    public void changeGadget1()
    {
        int temp;
        switch(_roleIcon)
        {
            case Unit.role.Infantryman:
                temp = findGadgetIndex(InfantrymanGadgets1, _gadget1);
                temp++;
                if(temp >= InfantrymanGadgets1.Length)
                {
                    temp = 0;
                }
                if(activeTeam)
                {
                    dataEdit.BlueTeam[(int)activeNumber].gadget1 = InfantrymanGadgets1[temp];
                }
                else
                {
                    dataEdit.RedTeam[(int)activeNumber].gadget1 = InfantrymanGadgets1[temp];
                }
                _gadget1 = InfantrymanGadgets1[temp];
                break;
            case Unit.role.Rifleman:
                temp = findGadgetIndex(RiflemanGadgets1, _gadget1);
                temp++;
                if (temp >= RiflemanGadgets1.Length)
                {
                    temp = 0;
                }
                if (activeTeam)
                {
                    dataEdit.BlueTeam[(int)activeNumber].gadget1 = RiflemanGadgets1[temp];
                }
                else
                {
                    dataEdit.RedTeam[(int)activeNumber].gadget1 = RiflemanGadgets1[temp];
                }
                _gadget1 = RiflemanGadgets1[temp];
                break;
            default:
                break;
        }
        if(activeTeam)
        {
            unitsEdited[(int)activeNumber].SetData(dataEdit.BlueTeam[(int)activeNumber]);
        }
        else
        {
            unitsEdited[(int)activeNumber].SetData(dataEdit.RedTeam[(int)activeNumber]);
        }
        gadget1ChoicePreview.SetCategoryAndLabel(gadget1ChoicePreview.GetCategory(), _gadget1.ToString());
        StartData.Instance.UpdateData(getUnitsSettings());
    }
    public void changeGadget2()
    {
        int temp;
        switch (_roleIcon)
        {
            case Unit.role.Infantryman:
                temp = findGadgetIndex(InfantrymanGadgets2, _gadget2);
                temp++;
                if (temp >= InfantrymanGadgets2.Length)
                {
                    temp = 0;
                }
                if (activeTeam)
                {
                    dataEdit.BlueTeam[(int)activeNumber].gadget2 = InfantrymanGadgets2[temp];

                }
                else
                {
                    dataEdit.RedTeam[(int)activeNumber].gadget2 = InfantrymanGadgets2[temp];
                }
                _gadget2 = InfantrymanGadgets2[temp];
                break;
            case Unit.role.Rifleman:
                temp = findGadgetIndex(RiflemanGadgets2, _gadget2);
                temp++;
                if (temp >= RiflemanGadgets2.Length)
                {
                    temp = 0;
                }
                if (activeTeam)
                {
                    dataEdit.BlueTeam[(int)activeNumber].gadget2 = RiflemanGadgets2[temp];

                }
                else
                {
                    dataEdit.RedTeam[(int)activeNumber].gadget2 = RiflemanGadgets2[temp];
                }
                _gadget2 = RiflemanGadgets2[temp];
                break;
            default:
                break;
        }
        if (activeTeam)
        {
            unitsEdited[(int)activeNumber].SetData(dataEdit.BlueTeam[(int)activeNumber]);
        }
        else
        {
            unitsEdited[(int)activeNumber].SetData(dataEdit.RedTeam[(int)activeNumber]);
        }
        gadget2ChoicePreview.SetCategoryAndLabel(gadget2ChoicePreview.GetCategory(), _gadget2.ToString());
        StartData.Instance.UpdateData(getUnitsSettings());
    }

    public StartData.GameSettingsData getUnitsSettings()
    {
        return dataEdit;
    }

    void Start()
    {
        ShowUnits();
        dataEdit = StartData.Instance.getData();
    }

    void Awake()
    {
        Instance = this;
    }

    public void ShowUnits()
    {
        GameObject spawnedUnit;
        Unit.UnitData tempUData;
        StartData.GameSettingsData data = StartData.Instance.getData();
        //Set BlueTeam
        spawnedUnit = Instantiate(UnitPrefab, B1pos.GetComponent<Transform>().position - new Vector3(1f, 1f, 0f), Quaternion.identity);
        tempUData = data.BlueTeam[0];
        spawnedUnit.GetComponent<Unit>().SetData(tempUData);
        B1pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel(B1pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().GetCategory(), tempUData.UnitRole.ToString() + "Blue");// new
        spawnedUnit = Instantiate(UnitPrefab, B2pos.GetComponent<Transform>().position - new Vector3(1f, 1f, 0f), Quaternion.identity);
        tempUData = data.BlueTeam[1];
        spawnedUnit.GetComponent<Unit>().SetData(tempUData);
        B2pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel(B2pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().GetCategory(), tempUData.UnitRole.ToString() + "Blue");// new
        spawnedUnit = Instantiate(UnitPrefab, B3pos.GetComponent<Transform>().position - new Vector3(1f, 1f, 0f), Quaternion.identity);
        tempUData = data.BlueTeam[2];
        spawnedUnit.GetComponent<Unit>().SetData(tempUData);
        B3pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel(B3pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().GetCategory(), tempUData.UnitRole.ToString() + "Blue");// new
        spawnedUnit = Instantiate(UnitPrefab, B4pos.GetComponent<Transform>().position - new Vector3(1f, 1f, 0f), Quaternion.identity);
        tempUData = data.BlueTeam[3];
        spawnedUnit.GetComponent<Unit>().SetData(tempUData);
        B4pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel(B4pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().GetCategory(), tempUData.UnitRole.ToString() + "Blue");// new
        //Set RedTeam
        spawnedUnit = Instantiate(UnitPrefab, R1pos.GetComponent<Transform>().position - new Vector3(-1f, 1f, 0f), Quaternion.identity);
        tempUData = data.RedTeam[0];
        spawnedUnit.GetComponent<Unit>().SetData(tempUData);
        R1pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel(R1pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().GetCategory(), tempUData.UnitRole.ToString() + "Red");// new
        spawnedUnit = Instantiate(UnitPrefab, R2pos.GetComponent<Transform>().position - new Vector3(-1f, 1f, 0f), Quaternion.identity);
        tempUData = data.RedTeam[1];
        spawnedUnit.GetComponent<Unit>().SetData(tempUData);
        R2pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel(R2pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().GetCategory(), tempUData.UnitRole.ToString() + "Red");// new
        spawnedUnit = Instantiate(UnitPrefab, R3pos.GetComponent<Transform>().position - new Vector3(-1f, 1f, 0f), Quaternion.identity);
        tempUData = data.RedTeam[2];
        spawnedUnit.GetComponent<Unit>().SetData(tempUData);
        R3pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel(R3pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().GetCategory(), tempUData.UnitRole.ToString() + "Red");// new
        spawnedUnit = Instantiate(UnitPrefab, R4pos.GetComponent<Transform>().position - new Vector3(-1f, 1f, 0f), Quaternion.identity);
        tempUData = data.RedTeam[3];
        spawnedUnit.GetComponent<Unit>().SetData(tempUData);
        R4pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel(R4pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().GetCategory(), tempUData.UnitRole.ToString() + "Red");// new
    }

    public void ResetPreview()
    {
        if (resetPreview != null)
        {
            resetPreview(this, EventArgs.Empty);
        }
    }

    public void BlueTeamEdit()
    {
        GameObject spawnedUnit;
        Unit.UnitData tempUData;
        StartData.GameSettingsData data = StartData.Instance.getData();

        spawnedUnit = Instantiate(UnitPrefab, I_pos.GetComponent<Transform>().position - new Vector3 (1f, 2f, 0f), Quaternion.identity);
        tempUData = data.BlueTeam[0];
        spawnedUnit.GetComponent<Unit>().SetData(tempUData);
        I_pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel(I_pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().GetCategory(), tempUData.UnitRole.ToString());// new
        unitsEdited[0] = spawnedUnit.GetComponent<Unit>();
        spawnedUnit = Instantiate(UnitPrefab, II_pos.GetComponent<Transform>().position - new Vector3(1f, 2f, 0f), Quaternion.identity);
        tempUData = data.BlueTeam[1];
        spawnedUnit.GetComponent<Unit>().SetData(tempUData);
        II_pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel(II_pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().GetCategory(), tempUData.UnitRole.ToString());// new
        unitsEdited[1] = spawnedUnit.GetComponent<Unit>();
        spawnedUnit = Instantiate(UnitPrefab, III_pos.GetComponent<Transform>().position - new Vector3(1f, 2f, 0f), Quaternion.identity);
        tempUData = data.BlueTeam[2];
        spawnedUnit.GetComponent<Unit>().SetData(tempUData);
        III_pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel(III_pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().GetCategory(), tempUData.UnitRole.ToString());// new
        unitsEdited[2] = spawnedUnit.GetComponent<Unit>();
        spawnedUnit = Instantiate(UnitPrefab, IV_pos.GetComponent<Transform>().position - new Vector3(1f, 2f, 0f), Quaternion.identity);
        tempUData = data.BlueTeam[3];
        spawnedUnit.GetComponent<Unit>().SetData(tempUData);
        IV_pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel(IV_pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().GetCategory(), tempUData.UnitRole.ToString());// new
        unitsEdited[3] = spawnedUnit.GetComponent<Unit>();
    }

    public void RedTeamEdit()
    {
        GameObject spawnedUnit;
        Unit.UnitData tempUData;
        StartData.GameSettingsData data = StartData.Instance.getData();

        spawnedUnit = Instantiate(UnitPrefab, I_pos.GetComponent<Transform>().position - new Vector3(1f, 2f, 0f), Quaternion.identity);
        tempUData = data.RedTeam[0];
        spawnedUnit.GetComponent<Unit>().SetData(tempUData);
        I_pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel(I_pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().GetCategory(), tempUData.UnitRole.ToString());// new
        unitsEdited[0] = spawnedUnit.GetComponent<Unit>();
        spawnedUnit = Instantiate(UnitPrefab, II_pos.GetComponent<Transform>().position - new Vector3(1f, 2f, 0f), Quaternion.identity);
        tempUData = data.RedTeam[1];
        spawnedUnit.GetComponent<Unit>().SetData(tempUData);
        II_pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel(II_pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().GetCategory(), tempUData.UnitRole.ToString());// new
        unitsEdited[1] = spawnedUnit.GetComponent<Unit>();
        spawnedUnit = Instantiate(UnitPrefab, III_pos.GetComponent<Transform>().position - new Vector3(1f, 2f, 0f), Quaternion.identity);
        tempUData = data.RedTeam[2];
        spawnedUnit.GetComponent<Unit>().SetData(tempUData);
        III_pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel(III_pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().GetCategory(), tempUData.UnitRole.ToString());// new
        unitsEdited[2] = spawnedUnit.GetComponent<Unit>();
        spawnedUnit = Instantiate(UnitPrefab, IV_pos.GetComponent<Transform>().position - new Vector3(1f, 2f, 0f), Quaternion.identity);
        tempUData = data.RedTeam[3];
        spawnedUnit.GetComponent<Unit>().SetData(tempUData);
        IV_pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel(IV_pos.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().GetCategory(), tempUData.UnitRole.ToString());// new
        unitsEdited[3] = spawnedUnit.GetComponent<Unit>();
    }

    public void EditingSetup()
    {
        dataEdit = StartData.Instance.getData();
    }


}
