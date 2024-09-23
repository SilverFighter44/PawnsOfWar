using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ChoiceMenuManager : MonoBehaviour
{
    public static ChoiceMenuManager Instance;

    [SerializeField] private TMP_Dropdown editUnitChoiceDropdown;
    [SerializeField] private GameObject UnitPrefab, menuIconPrefab;
    [SerializeField] private Transform blueUpperRowStart, blueBottomRowStart, redUpperRowStart, redBottomRowStart, MainMenuObject, editUnitsPosition;
    [SerializeField] private List<GameObject> mainBlueIcons, mainRedIcons, editUnitsIcons;
    [SerializeField] private UnityEngine.U2D.Animation.SpriteResolver roleChoicePreview, gadget1ChoicePreview, gadget2ChoicePreview, gameModePreview, sideBPreview, sideRPrewiev;
    [SerializeField] private bool activeTeam;
    [SerializeField] private Unit.number activeNumber;
    [SerializeField] private StartData.GameSettingsData dataEdit;
    [SerializeField] private Unit[] unitsEdited;
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

    public void SetEditUnit()
    {
        int n = editUnitChoiceDropdown.value;
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

    public void StartGame()
    {
        SceneManager.LoadScene(2);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void changeGameMode()
    {
        _gameMode++;
        bool tempBool = false;
        while(_gameMode < StartData.gameMode.end && !tempBool)
        {
            switch((int)_gameMode)
            {
                case 1:
                    if(!dataEdit.compatibleModes[1])
                    {
                        _gameMode++;
                    }
                    else
                    {
                        tempBool = true;
                    }
                    break;
                case 2:
                    if (!dataEdit.compatibleModes[0])
                    {
                        _gameMode++;
                    }
                    else
                    {
                        tempBool = true;
                    }
                    break;
                case 3:
                    if (!(dataEdit.compatibleModes[0] && dataEdit.compatibleModes[1]))
                    {
                        _gameMode++;
                    }
                    else
                    {
                        tempBool = true;
                    }
                    break;
                default:
                    tempBool = true;
                    break;
            }
        }
        
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
        editUnitsIcons[(int)activeNumber].GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel("NeutralIcon", _roleIcon.ToString());

        if (activeTeam)
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
        gadget1ChoicePreview.SetCategoryAndLabel(gadget1ChoicePreview.GetCategory(), _gadget1.ToString());
        StartData.Instance.UpdateData(getUnitsSettings());
        ResetPreview();//
        if(activeTeam)
        {
            //unitsEdited[(int)activeNumber].SetData(dataEdit.BlueTeam[(int)activeNumber]);
            BlueTeamEdit();//
        }
        else
        {
            //unitsEdited[(int)activeNumber].SetData(dataEdit.RedTeam[(int)activeNumber]);
            RedTeamEdit();//
        }
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
        gadget2ChoicePreview.SetCategoryAndLabel(gadget2ChoicePreview.GetCategory(), _gadget2.ToString());
        StartData.Instance.UpdateData(getUnitsSettings());
        ResetPreview();//
        if (activeTeam)
        {
            //unitsEdited[(int)activeNumber].SetData(dataEdit.BlueTeam[(int)activeNumber]);
            BlueTeamEdit();//
        }
        else
        {
            //unitsEdited[(int)activeNumber].SetData(dataEdit.RedTeam[(int)activeNumber]);
            RedTeamEdit();//
        }
    }

    public StartData.GameSettingsData getUnitsSettings()
    {
        return dataEdit;
    }

    void Start()
    {
        dataEdit = StartData.Instance.getData();
        string mapData = System.IO.File.ReadAllText(dataEdit.mapFilePath);
        GridTools.MapIntermediate mapIntermediate = JsonUtility.FromJson< GridTools.MapIntermediate >(mapData);
        GridTools.Map currentMap = GridTools.translateMapFromIntermediate(mapIntermediate);
        StartData.Instance.setTeamSize(mapIntermediate.teamSize);
        dataEdit = StartData.Instance.getData();
        dataEdit.compatibleModes = currentMap.modeCompatibility;
        StartData.Instance.UpdateData(getUnitsSettings());
        for (int i = 0; i < currentMap.teamSize; i++)
        {
            GameObject newBlueIcon = Instantiate(menuIconPrefab,new Vector3(blueBottomRowStart.position.x + 2f*((i < mapIntermediate.teamSize / 2) ? i : i -  mapIntermediate.teamSize / 2), ((i < currentMap.teamSize / 2) ? blueUpperRowStart.position.y : blueBottomRowStart.position.y), 0f), Quaternion.identity);
            mainBlueIcons.Add(newBlueIcon);
            newBlueIcon.transform.parent = MainMenuObject.transform;
            GameObject newRedIcon = Instantiate(menuIconPrefab, new Vector3(redBottomRowStart.position.x - 2f * ((i < mapIntermediate.teamSize / 2) ? i : i - mapIntermediate.teamSize / 2), ((i < currentMap.teamSize / 2) ? redUpperRowStart.position.y : redBottomRowStart.position.y), 0f), Quaternion.identity);
            mainRedIcons.Add(newRedIcon);
            newRedIcon.transform.parent = MainMenuObject.transform;
            GameObject newEditUnitsIcon = Instantiate(menuIconPrefab, editUnitsPosition.position + new Vector3(2f * i ,0f ,0f), Quaternion.identity);
            editUnitsIcons.Add(newEditUnitsIcon);
            newEditUnitsIcon.transform.parent = editUnitsPosition;
        }
        unitsEdited = new Unit[mapIntermediate.teamSize];
        ShowUnits();
        List<string> unitsNumbers = new List<string>();
        for(int i = 0; i < mapIntermediate.teamSize; i++)
        {
            unitsNumbers.Add((i + 1).ToString());
        }
        editUnitChoiceDropdown.AddOptions(unitsNumbers);
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

        for (int i = 0; i < mainBlueIcons.Count; i++)
        {
            spawnedUnit = Instantiate(UnitPrefab, mainBlueIcons[i].transform.position - new Vector3(1f, 1f, 0f), Quaternion.identity);
            tempUData = data.BlueTeam[i];
            spawnedUnit.GetComponent<Unit>().SetData(tempUData);
            mainBlueIcons[i].GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel("BlueIcon", tempUData.UnitRole.ToString() + "Blue");
            spawnedUnit = Instantiate(UnitPrefab, mainRedIcons[i].transform.position - new Vector3(-1f, 1f, 0f), Quaternion.identity);
            tempUData = data.RedTeam[i];
            spawnedUnit.GetComponent<Unit>().SetData(tempUData);
            mainRedIcons[i].GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel("RedIcon", tempUData.UnitRole.ToString() + "Red");
        }
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

        for (int i = 0; i < data.BlueTeam.Length; i++)
        {
            spawnedUnit = Instantiate(UnitPrefab, editUnitsIcons[i].transform.position - new Vector3(1f, 3f, 0f), Quaternion.identity);
            tempUData = data.BlueTeam[i];
            spawnedUnit.GetComponent<Unit>().SetData(tempUData);
            editUnitsIcons[i].GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel("NeutralIcon", tempUData.UnitRole.ToString());
            unitsEdited[i] = spawnedUnit.GetComponent<Unit>();
            spawnedUnit.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        }
    }

    public void RedTeamEdit()
    {
        GameObject spawnedUnit;
        Unit.UnitData tempUData;
        StartData.GameSettingsData data = StartData.Instance.getData();

        for (int i = 0; i < data.RedTeam.Length; i++)
        {
            spawnedUnit = Instantiate(UnitPrefab, editUnitsIcons[i].transform.position - new Vector3(1f, 3f, 0f), Quaternion.identity);
            tempUData = data.RedTeam[i];
            spawnedUnit.GetComponent<Unit>().SetData(tempUData);
            editUnitsIcons[i].GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel("NeutralIcon", tempUData.UnitRole.ToString());
            unitsEdited[i] = spawnedUnit.GetComponent<Unit>();
            spawnedUnit.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        }
    }

    public void EditingSetup()
    {
        dataEdit = StartData.Instance.getData();
    }
}
