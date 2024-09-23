using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartData : MonoBehaviour
{
    public static StartData Instance;
    [SerializeField] private int teamSize;

    [System.Serializable]
    public struct GameSettingsData
    {
        public Unit.UnitData [] BlueTeam;
        public Unit.UnitData [] RedTeam;
        public gameMode GameMode;
        public string mapFilePath;
        public bool[] compatibleModes;
    };

    public enum gameMode { attack, defenceR, defenceB, defence, end };

    [SerializeField] private GameSettingsData data;
    [SerializeField] private Unit BlueUnit, RedUnit;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void setTeamSize(int teamSize)
    {
        this.teamSize = teamSize;
        data.BlueTeam = new Unit.UnitData[teamSize];// { BlueUnit.GetData(), BlueUnit.GetData(), BlueUnit.GetData(), BlueUnit.GetData() };
        data.RedTeam = new Unit.UnitData[teamSize];// { RedUnit.GetData(), RedUnit.GetData(), RedUnit.GetData(), RedUnit.GetData() };
        for (int i = 0; i < teamSize; i++)
        {
            data.BlueTeam[i] = BlueUnit.GetData();
            data.BlueTeam[i].UnitNumber = (Unit.number)i;
            data.RedTeam[i] = RedUnit.GetData();
            data.RedTeam[i].UnitNumber = (Unit.number)i;
        }
    }

    public void UpdateData( GameSettingsData _data)
    {
        data = _data;
    }

    void Start()
    {
        data.GameMode = gameMode.attack;
    }

    public GameSettingsData getData ()
    {
        return data;
    }

}

