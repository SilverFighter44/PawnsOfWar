using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_Animation : MonoBehaviour
{
    [SerializeField] private Unit unitScript;
    [SerializeField] private Project.WeaponData.WeaponManager weaponManagerScript;
    [SerializeField] private Unit.UnitData blueDefault, redDefault;
    [SerializeField] private float _x, _y;

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _x = mousePos.x;
            _y = mousePos.y;
            unitScript.ShootAt(_x, _y, 0, 0);
        }
    }
    void Start()
    {
        unitScript.SetData(blueDefault);
    }

    public void setBlueDefault()
    {
        weaponManagerScript.ChangeWeapon();
        unitScript.SetData(blueDefault);
    }

    public void setRedDefault()
    {
        weaponManagerScript.ChangeWeapon();
        unitScript.SetData(redDefault);
        weaponManagerScript.FlipCharacter();
        weaponManagerScript.LookAtSide();
    }

    public void shoot()
    {
     // unitScript.ShootAt(_x, _y);
    }
}
