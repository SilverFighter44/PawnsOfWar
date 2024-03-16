using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project.WeaponData
{
    [CreateAssetMenu(menuName = "WeaponData/WeaponData")]

    public class WeaponData : ScriptableObject
    {
        public int damage;
        public string weaponName;
        public GameObject weaponPrefab;
    }
}
