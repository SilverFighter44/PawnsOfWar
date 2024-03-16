using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HudManager : MonoBehaviour
{
    [SerializeField] private UI_ClassSymbol classIconB1, classIconB2, classIconB3, classIconB4, classIconR1, classIconR2, classIconR3, classIconR4;
    StartData.GameSettingsData data;

    public void Start()
    {
        data = StartData.Instance.getData();
        classIconB1.ChangeIcon(data.BlueTeam[0].UnitRole, UI_ClassSymbol.IconCategory.Blue);
        classIconB2.ChangeIcon(data.BlueTeam[1].UnitRole, UI_ClassSymbol.IconCategory.Blue);
        classIconB3.ChangeIcon(data.BlueTeam[2].UnitRole, UI_ClassSymbol.IconCategory.Blue);
        classIconB4.ChangeIcon(data.BlueTeam[3].UnitRole, UI_ClassSymbol.IconCategory.Blue);
        classIconR1.ChangeIcon(data.RedTeam[0].UnitRole, UI_ClassSymbol.IconCategory.Red);
        classIconR2.ChangeIcon(data.RedTeam[1].UnitRole, UI_ClassSymbol.IconCategory.Red);
        classIconR3.ChangeIcon(data.RedTeam[2].UnitRole, UI_ClassSymbol.IconCategory.Red);
        classIconR4.ChangeIcon(data.RedTeam[3].UnitRole, UI_ClassSymbol.IconCategory.Red);
    }

}
