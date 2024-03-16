using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Project.WeaponData
{
    public class WeaponManager : MonoBehaviour
    {
        [SerializeField] private WeaponData weaponData, AllyRifle, AxisRifle;
        [SerializeField] private GadgetData gadgetData1, gadgetData2, AllyGrenade, AxisGrenade, AllySmokeGrenade, AxisSmokeGrenade;
        [SerializeField] private GameObject EquiptedWeapon, EquiptedGadget1, EquiptedGadget2, handL_IK, handR_IK, defHandL, defHandR;
        [SerializeField] private Transform weaponPivot, numberTransform, gadget_I_position, gadget_II_position, handR;
        [SerializeField] private Weapon weaponScript;
        [SerializeField] private LookTowards headLT, handLLT, handRLT, torsoLT;
        [SerializeField] private Unit.gadget gadget1, gadget2;
        [SerializeField] private Unit.weapon weaponType;
        [SerializeField] private Unit.skin weaponSkin;
        [SerializeField] private Unit unitScript;
        [SerializeField] public Gadget gadget_I_script, gadget_II_script, gadgetInUse;

        private bool flipped;

        public bool isFlipped()
        {
            return flipped;
        }

        public void ChangeWeapon()
        {
            weaponScript.RemoveWeapon();
        }

        public void ResetGadgets()
        {
            if(EquiptedGadget1)
            {
            Destroy(EquiptedGadget1);
            }
            if(EquiptedGadget2)
            {
            Destroy(EquiptedGadget2);
            }
        }

        public void UseGadget1(int x, int y)
        {
            gadget_I_script.UseGadget(x, y);
        }

        public void UseGadget2(int x, int y)
        {
            gadget_II_script.UseGadget(x, y);
        }

        public void EquipGadget1()
        {
            if(!gadget_I_script.isSpent())
            {
                HolsterWeapon();
                gadget_I_script.EquipGadget(handL_IK, handR_IK, weaponPivot, handR, flipped);
                gadgetInUse = gadget_I_script;
            }
        }

        public void EquipGadget2()
        {
            if (!gadget_II_script.isSpent())
            { 
                HolsterWeapon();
                gadget_II_script.EquipGadget(handL_IK, handR_IK, weaponPivot, handR, flipped);
                gadgetInUse = gadget_II_script;
            }
        }

        public void UnequipGadget()
        {
            UnholsterWeapon();
            gadgetInUse.UnequipGadget();
        }

        public void HolsterWeapon()
        {
            handL_IK.transform.parent = defHandL.transform;
            handR_IK.transform.parent = defHandR.transform;
            LookDown();
        }

        public void UnholsterWeapon()
        {
            weaponScript.GetAGrip(handL_IK, handR_IK);
            LookAtSide();
        }

        public void MoveSide (bool _side, bool _directional)
        {
            if (_directional)
            {
                if(_side == flipped)
                {
                    FlipCharacter();
                }
            }
            LookAtSide();
        }

        public void Shoot()
        {
            weaponScript.Shoot(weaponType, weaponSkin);
        }

        public void Reload()
        {
            weaponScript.Reload(weaponType, weaponSkin);
        }

        public void FlipCharacter()
        {
            transform.localScale = new Vector3(transform.localScale.x * -1f, transform.localScale.y, transform.localScale.z);
            if(!flipped)
            {
                flipped = true;
                numberTransform.localScale = new Vector3(numberTransform.localScale.x * -1f, numberTransform.localScale.y, numberTransform.localScale.z);//
            }
            else
            {
                flipped = false;
            }
            if(weaponScript)
            {
              weaponScript.SetFlipped(flipped);
            }
        }

        public void EquipWeapon( Unit.weapon _weapon, Unit.skin _skin)
        {
            if (_skin == Unit.skin.Ally_US_1)
            {
                if (_weapon == Unit.weapon.rifle)
                {
                   weaponData = AllyRifle;
                }
            }
            else if (_skin == Unit.skin.Axis_Ger_1)
            {
                if (_weapon == Unit.weapon.rifle)
                {
                    weaponData = AxisRifle;
                }
            }
            weaponType = _weapon;
            weaponSkin = _skin;

            EquiptedWeapon = Instantiate(weaponData.weaponPrefab, weaponPivot);
            weaponScript = EquiptedWeapon.GetComponent<Weapon>();
            weaponScript.GetAGrip(handL_IK, handR_IK);
            weaponScript.GetUnitScript(unitScript);
            weaponScript.SetFlipped(flipped);
        }

        public void EquipGadgets(Unit.gadget _gadget1, Unit.gadget _gadget2, Unit.skin _skin)
        {
            if (_skin == Unit.skin.Ally_US_1)
            {
                switch (_gadget1)
                {
                    case Unit.gadget.grenade:
                        gadgetData1 = AllyGrenade;
                        break;
                    case Unit.gadget.smoke:
                        gadgetData1 = AllySmokeGrenade;
                        break;
                    default:
                        break;
                }
                switch (_gadget2)
                {
                    case Unit.gadget.grenade:
                        gadgetData2 = AllyGrenade;
                        break;
                    case Unit.gadget.smoke:
                        gadgetData2 = AllySmokeGrenade;
                        break;
                    default:
                        break;
                }
            }
            else if (_skin == Unit.skin.Axis_Ger_1)
            {
                switch (_gadget1)
                {
                    case Unit.gadget.grenade:
                        gadgetData1 = AxisGrenade;
                        break;
                    case Unit.gadget.smoke:
                        gadgetData1 = AxisSmokeGrenade;
                        break;
                    default:
                        break;
                }
                switch (_gadget2)
                {
                    case Unit.gadget.grenade:
                        gadgetData2 = AxisGrenade;
                        break;
                    case Unit.gadget.smoke:
                        gadgetData2 = AxisSmokeGrenade;
                        break;
                    default:
                        break;
                }
            }
            gadget1 = _gadget1;
            gadget2 = _gadget2;

            //EquiptedWeapon = Instantiate(weaponData.weaponPrefab, weaponPivot);
            EquiptedGadget1 = Instantiate(gadgetData1.gadgetPrefab, gadget_I_position);
            EquiptedGadget1.transform.parent = gadget_I_position;
            gadget_I_script = EquiptedGadget1.GetComponent<Gadget>();
            gadget_I_script.SetUnitScript(unitScript);
            EquiptedGadget2 = Instantiate(gadgetData2.gadgetPrefab, gadget_II_position);
            EquiptedGadget2.transform.parent = gadget_II_position;
            gadget_II_script = EquiptedGadget2.GetComponent<Gadget>();
            gadget_II_script.SetUnitScript(unitScript);
        }

        public void LookAtSide()
        {
            if (flipped)
            {
                AllLookTowards(weaponPivot.position.x - 5f, weaponPivot.position.y);
            }
            else
            {
                AllLookTowards(weaponPivot.position.x + 5f, weaponPivot.position.y);
            }
        }

        public void LookDown()
        {
            if (flipped)
            {
                weaponScript.RotateTowards(weaponPivot.position.x, weaponPivot.position.y - 5f, 180f);
                handLLT.RotateTowards(90f, 180f);
                handRLT.RotateTowards(90f, 180f);
            }
            else
            {
                weaponScript.RotateTowards(weaponPivot.position.x, weaponPivot.position.y - 5f, 0f);
                handLLT.RotateTowards(90f, 0f);
                handRLT.RotateTowards(90f, 0f);
            }
        }

        public async void AllLookTowards(float x, float y)
        {
            float adjustment = 0f;
            Vector3 Diff = new Vector3(x, y, 0f) - new Vector3(weaponPivot.position.x, weaponPivot.position.y, 0f);
            float angle = Mathf.Atan2(Diff.y, Diff.x) * Mathf.Rad2Deg;
            if ((angle < -90 || angle > 90) && !flipped)
            {
                FlipCharacter();
            }
            else if ((angle > -90 && angle < 90) && flipped)
            {
                FlipCharacter();
            }
            
            if(flipped)
            {
                adjustment = 180f;
                if(angle < 0)
                {
                    await torsoLT.RotateTowards(angle / 3, -30f);
                }
                else
                {
                    await torsoLT.RotateTowards(angle / 3, 210f);
                }

            }
            else
            {
                await torsoLT.RotateTowards(angle / 3  , 90f);
            }
            await weaponScript.RotateTowards(x, y, adjustment);
            Diff = new Vector3(x, y, 0f) - new Vector3(headLT.transform.position.x, headLT.transform.position.y, 0f);
            angle = Mathf.Atan2(Diff.y, Diff.x) * Mathf.Rad2Deg;
            await headLT.RotateTowards(angle, 90f);
            Diff = new Vector3(x, y, 0f) - new Vector3(handLLT.transform.position.x, handLLT.transform.position.y, 0f);
            angle = Mathf.Atan2(Diff.y, Diff.x) * Mathf.Rad2Deg;
            await handLLT.RotateTowards(angle, adjustment);
            Diff = new Vector3(x, y, 0f) - new Vector3(handRLT.transform.position.x, handRLT.transform.position.y, 0f);
            angle = Mathf.Atan2(Diff.y, Diff.x) * Mathf.Rad2Deg;
            await handRLT.RotateTowards(angle, adjustment);
        }
    }
}
