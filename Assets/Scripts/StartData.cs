using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartData : MonoBehaviour
{
    public static StartData Instance;   

    public struct GameSettingsData
    {
        public Unit.UnitData [] BlueTeam;
        public Unit.UnitData [] RedTeam;
        public gameMode GameMode;
    };

    public enum gameMode { attack, defenceR, defenceB, defence, end };

    [SerializeField] private GameSettingsData data;
    [SerializeField] private Unit BlueUnit, RedUnit;
    [SerializeField] private int layerMultiplier;

    public int getLayerMultiplier()
    {
        return layerMultiplier;
    }

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

    public void UpdateData( GameSettingsData _data)
    {
        data = _data;
    }

    void Start()
    {
        data.BlueTeam = new Unit.UnitData[4] { BlueUnit.GetData(), BlueUnit.GetData(), BlueUnit.GetData(), BlueUnit.GetData() };
        data.RedTeam = new Unit.UnitData[4] { RedUnit.GetData(), RedUnit.GetData(), RedUnit.GetData(), RedUnit.GetData() };
        data.BlueTeam[0].UnitNumber = Unit.number.I;
        data.BlueTeam[1].UnitNumber = Unit.number.II;
        data.BlueTeam[2].UnitNumber = Unit.number.III;
        data.BlueTeam[3].UnitNumber = Unit.number.IV;
        data.RedTeam[0].UnitNumber = Unit.number.I;
        data.RedTeam[1].UnitNumber = Unit.number.II;
        data.RedTeam[2].UnitNumber = Unit.number.III;
        data.RedTeam[3].UnitNumber = Unit.number.IV;
        data.GameMode = gameMode.attack;
    }

    public GameSettingsData getData ()
    {
        return data;
    }

}

