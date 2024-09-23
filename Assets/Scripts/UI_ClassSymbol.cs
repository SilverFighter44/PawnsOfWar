using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ClassSymbol : MonoBehaviour
{
    [SerializeField] private List<Sprite> neuralClasses, blueClasses, redClasses, neutralNumbers, blueNumbers, redNumbers;
    [SerializeField] private Image image;
    public enum IconCategory { neutral, Blue, Red };

    public void ChangeIcon (Unit.role _role, IconCategory _category)
    {
        switch(_category)
        {// neutral, blu, red
            case IconCategory.neutral:
                image.sprite = neuralClasses[(int)_role];
                break;
            case IconCategory.Blue:
                image.sprite = blueClasses[(int)_role];
                break;
            case IconCategory.Red:
                image.sprite = redClasses[(int)_role];
                break;
            default:
                break;
        }
    }

    public void ChangeNumber(Unit.number _number, IconCategory _category)
    {
        switch (_category)
        {
            case IconCategory.neutral:
                image.sprite = neutralNumbers[(int)_number];
                break;
            case IconCategory.Blue:
                image.sprite = blueNumbers[(int)_number];
                break;
            case IconCategory.Red:
                image.sprite = redNumbers[(int)_number];
                break;
            default:
                break ;
        }
    }

}
        

