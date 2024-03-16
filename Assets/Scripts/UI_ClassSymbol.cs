using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ClassSymbol : MonoBehaviour
{
    [SerializeField] private Sprite Infantryman, Spy, Sapper, Rifleman, Support, ExplosivesSpecialist, Sniper, GasSpecialist, LauncherOperator, CombatEngineer, Medic, Scout, BlueInfantryman, BlueSpy, BlueSapper, BlueRifleman, BlueSupport, BlueExplosivesSpecialist, BlueSniper, BlueGasSpecialist, BlueLauncherOperator, BlueCombatEngineer, BlueMedic, BlueScout, RedInfantryman, RedSpy, RedSapper, RedRifleman, RedSupport, RedExplosivesSpecialist, RedSniper, RedGasSpecialist, RedLauncherOperator, RedCombatEngineer, RedMedic, RedScout;
    [SerializeField] private Image image;
    public enum IconCategory { neutral, Blue, Red };

    public void ChangeIcon (Unit.role _role, IconCategory _category)
    {
        switch (_category)
        {
            case IconCategory.neutral:
            {
                switch (_role)
                {
                    case Unit.role.Infantryman:
                        {
                            image.sprite = Infantryman;
                            break;
                        }
                    case Unit.role.Spy:
                        {
                            image.sprite = Spy;
                            break;
                        }
                    case Unit.role.Sapper:
                        {
                            image.sprite = Sapper;
                            break;
                        }
                    case Unit.role.Rifleman:
                        {
                            image.sprite = Rifleman;
                            break;
                        }
                    case Unit.role.Support:
                        {
                            image.sprite = Support;
                            break;
                        }
                    case Unit.role.ExplosivesSpecialist:
                        {
                            image.sprite = ExplosivesSpecialist;
                            break;
                        }
                    case Unit.role.Sniper:
                        {
                            image.sprite = Sniper;
                            break;
                        }
                    case Unit.role.GasSpecialist:
                        {
                            image.sprite = GasSpecialist;
                            break;
                        }
                    case Unit.role.LauncherOperator:
                        {
                            image.sprite = LauncherOperator;
                            break;
                        }
                    case Unit.role.CombatEngineer:
                        {
                            image.sprite = CombatEngineer;
                            break;
                        }
                    case Unit.role.Medic:
                        {
                            image.sprite = Medic;
                            break;
                        }
                    case Unit.role.Scout:
                        {
                            image.sprite = Scout;
                            break;
                        }
                    default:
                        break;
                }
                break;
            }
            case IconCategory.Blue:
            {
                switch (_role)
                {
                    case Unit.role.Infantryman:
                        {
                            image.sprite = BlueInfantryman;
                            break;
                        }
                    case Unit.role.Spy:
                        {
                            image.sprite = BlueSpy;
                            break;
                        }
                    case Unit.role.Sapper:
                        {
                            image.sprite = BlueSapper;
                            break;
                        }
                    case Unit.role.Rifleman:
                        {
                            image.sprite = BlueRifleman;
                            break;
                        }
                    case Unit.role.Support:
                        {
                            image.sprite = BlueSupport;
                            break;
                        }
                    case Unit.role.ExplosivesSpecialist:
                        {
                            image.sprite = BlueExplosivesSpecialist;
                            break;
                        }
                    case Unit.role.Sniper:
                        {
                            image.sprite = BlueSniper;
                            break;
                        }
                    case Unit.role.GasSpecialist:
                        {
                            image.sprite = BlueGasSpecialist;
                            break;
                        }
                    case Unit.role.LauncherOperator:
                        {
                            image.sprite = BlueLauncherOperator;
                            break;
                        }
                    case Unit.role.CombatEngineer:
                        {
                            image.sprite = BlueCombatEngineer;
                            break;
                        }
                    case Unit.role.Medic:
                        {
                            image.sprite = BlueMedic;
                            break;
                        }
                    case Unit.role.Scout:
                        {
                            image.sprite = BlueScout;
                            break;
                        }
                    default:
                        break;
                }
                    break;
            }
            case IconCategory.Red:
            {
                switch (_role)
                {
                    case Unit.role.Infantryman:
                        {
                            image.sprite = RedInfantryman;
                            break;
                        }
                    case Unit.role.Spy:
                        {
                            image.sprite = RedSpy;
                            break;
                        }
                    case Unit.role.Sapper:
                        {
                            image.sprite = RedSapper;
                            break;
                        }
                    case Unit.role.Rifleman:
                        {
                            image.sprite = RedRifleman;
                            break;
                        }
                    case Unit.role.Support:
                        {
                            image.sprite = RedSupport;
                            break;
                        }
                    case Unit.role.ExplosivesSpecialist:
                        {
                            image.sprite = RedExplosivesSpecialist;
                            break;
                        }
                    case Unit.role.Sniper:
                        {
                            image.sprite = RedSniper;
                            break;
                        }
                    case Unit.role.GasSpecialist:
                        {
                            image.sprite = RedGasSpecialist;
                            break;
                        }
                    case Unit.role.LauncherOperator:
                        {
                            image.sprite = RedLauncherOperator;
                            break;
                        }
                    case Unit.role.CombatEngineer:
                        {
                            image.sprite = RedCombatEngineer;
                            break;
                        }
                    case Unit.role.Medic:
                        {
                            image.sprite = RedMedic;
                            break;
                        }
                    case Unit.role.Scout:
                        {
                            image.sprite = RedScout;
                            break;
                        }
                    default:
                        break;
                }
                    break;
            }
            default:
                break;
        }
    }

}
        

