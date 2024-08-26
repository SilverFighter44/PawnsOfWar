using System.Collections;
using UnityEngine;

    public static class Helper
    {
        public static float absDif(float a, float b)
        {
            if (a > b)
                return a - b;
            return b - a;
        }


        public static void setObjetsVisibility(GameObject _object, bool active)
        {
            if (_object)
            {
                _object.SetActive(active);
            }
        }
    }
