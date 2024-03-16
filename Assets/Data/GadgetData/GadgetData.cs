using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//namespace Project.GadgetData
//{
    [CreateAssetMenu(menuName = "GadgetData/GadgetData")]

    public class GadgetData : ScriptableObject
    {
        public string gadgetName;
        public GameObject gadgetPrefab;
    }
//}
