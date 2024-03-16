using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTurn : MonoBehaviour
{
    public void Change()
    {
        GridManager.Instance.ChangeTurn();
    }
}
