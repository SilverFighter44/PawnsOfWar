using System.Collections;
using UnityEngine;

    public class Buttons : MonoBehaviour
{
        public static void ReloadButtton()
        {
            GridManager.Instance.selectedUnit.reload();
            GridManager.Instance.UpdateMovesCount();
        }

        public static void CrouchButtton()
        {
            GridManager.Instance.selectedUnit.crouch();
            GridManager.Instance.ResetHighlights();
        }

        public static void Gadget1Buttton()
        {
            GridManager.Instance.selectedUnit.useGadget1();
            GridManager.Instance.ResetHighlights();
        }

        public static void Gadget2Buttton()
        {
            GridManager.Instance.selectedUnit.useGadget2();
            GridManager.Instance.ResetHighlights();
        }
}