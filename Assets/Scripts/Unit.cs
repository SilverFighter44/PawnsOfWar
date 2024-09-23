using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using OutlineFx;

public class Unit : MonoBehaviour
{
    [System.Serializable]
    public struct UnitData
    {
        public weapon UnitWeapon;
        public skin UnitUniform;
        public gadget gadget1;
        public gadget gadget2;
        public role UnitRole;
        public number UnitNumber;
        public bool UnitTeam;
    };

    public struct UnitInfo
    {
        public bool crouched;
        public int mag;
        public int hp;
        public int movesCount;
        public gadget g1;
        public gadget g2;
        public number number;
        public role role;
        public UI_ClassSymbol.IconCategory team;
    };

    [SerializeField] private UnitData data;

    public enum weapon { rifle, sniper, lmg, smg, ar };
    public enum skin { Ally_US_1, Axis_Ger_1 };
    public enum gadget { grenade, smoke };
    public enum role { Infantryman, Rifleman, end, Spy, Sapper, Support, ExplosivesSpecialist, Sniper, GasSpecialist, LauncherOperator, CombatEngineer, Medic, Scout };
    public enum number { I = 0, II, III, IV, V, VI, VII, VIII };

    [SerializeField] private bool canMove, canShoot, team, crouched, gadgetActive, gadget1Active, moveActive = false;
    [SerializeField] private TextMeshPro unitNumberDisplay;
    [SerializeField] private UnityEngine.U2D.Animation.SpriteResolver unitShirt, unitShirtUpL, unitShirtUpR, unitShirtDnL, unitShirtDnR, unitPants, unitPantsUpL, unitPantsUpR, unitPantsDnL, unitPantsDnR, unitHelmet, unitHelmetBackground, unitBootsUpL, unitBootsUpR, unitBootsDnL, unitBootsDnR, unitRig, unitAmmoPouch;
    [SerializeField] private Project.WeaponData.WeaponManager weaponManager;
    [SerializeField] private Animator unitAnimator, faceAnimator;
    [SerializeField] private Vector3 onGridPosition;
    [SerializeField] private Slider healthBar;
    [SerializeField] private SpriteRenderer[] characterParts;
    [SerializeField] private int[] characterPartsLayerOrder;
    [SerializeField] private int maxMag, maxMoves, movesCount, range, mag, hp, id;
    [SerializeField] private GridTools.TileCoordinates maxOnGridPosition = new GridTools.TileCoordinates(1, 1), unitOnGridPosition = new GridTools.TileCoordinates(0, 0);
    [SerializeField] private GameObject helmet;
    [SerializeField] private gadget activeGadgetType;
    [SerializeField] private Queue<GridTools.TileCoordinates> movesQueue = new Queue<GridTools.TileCoordinates>();
    [SerializeField] private OutlineFx.Outline[] characterOutlines;

    // maxOnGridPosition: x = width, y = height

    public void hideOutlines()
    {
        for (int i = 0; i < characterOutlines.Length; i++)
        {
            characterOutlines[i].Color = Color.clear;
        }
    }

    public void showOutlines()
    {
        Color outlineColor = team ? Color.blue : Color.red;
        for (int i = 0; i < characterOutlines.Length; i++)
        {
            characterOutlines[i].Color = outlineColor;
        }
    }

    public GridTools.TileCoordinates getMaxOnGridPosition()
    {
        return maxOnGridPosition;
    }

    public GridTools.TileCoordinates getUnitOnGridPosition()
    {
        return unitOnGridPosition;
    }

    public int getAmmo()
    {
        return mag;
    }

    public bool whatTeam()
    {
        return team;
    }

    public bool CanMove()
    {
        return canMove;
    }

    public bool isGadgetActive()
    {
        return gadgetActive;
    }

    public bool CanShoot()
    {
        if (mag <= 0)
        {
            canShoot = false;
        }
        return canShoot;
    }

    public Vector3 getPosition()
    {
        return transform.position;
    }

    public void ReadyToShoot()
    {
        canShoot = true;
    }

    public bool isCrouched()
    {
        return crouched;
    }

    public gadget whatGadget() // gadget1Active: 1 = G1, 0 = G2
    {
        if (gadget1Active)
        {
            return data.gadget1;
        }
        else
        {
            return data.gadget2;
        }
    }

    public int whatRange()
    {
        return range;
    }

    public int whatID()
    {
        return id;
    }

    public int whatHP()
    {
        return hp;
    }

    public int howManyMoves()
    {
        return movesCount;
    }

    public UnitInfo GetInfoToDisplay()
    {
        UnitInfo info;
        info.crouched = crouched;
        info.mag = mag;
        info.hp = hp;
        info.g1 = data.gadget1;
        info.g2 = data.gadget2;
        info.number = data.UnitNumber;
        info.role = data.UnitRole;
        info.movesCount = movesCount;
        if (team)
        {
            info.team = UI_ClassSymbol.IconCategory.Blue;
        }
        else
        {
            info.team = UI_ClassSymbol.IconCategory.Red;
        }
        return info;
    }

    public Unit.gadget whatG1()
    {
        return data.gadget1;
    }

    public Unit.gadget whatG2()
    {
        return data.gadget2;
    }

    public void UseGadget(int x, int y) // gadget1Active: 1 = G1, 0 = G2
    {
        if (gadget1Active)
        {
            weaponManager.UseGadget1(x, y);
        }
        else
        {
            weaponManager.UseGadget2(x, y);
        }
    }

    public void useGadget1()
    {
        if (!moveActive)
        {
            if (gadgetActive)
            {
                unequipGadget();
            }
            else
            {
                gadgetActive = true;
                gadget1Active = true;
                weaponManager.EquipGadget1();
            }
        }
    }

    public void useGadget2()
    {
        if (!moveActive)
        {
            if (gadgetActive)
            {
                unequipGadget();
            }
            else
            {
                gadgetActive = true;
                gadget1Active = false;
                weaponManager.EquipGadget2();
            }
        }
    }

    public void unequipGadget()
    {
        gadgetActive = false;
        weaponManager.UnequipGadget();
    }

    public UnitData GetData()
    {
        return data;
    }

    public async void Walk(int _x, int _y, MoveHighlight moveHighlight)
    {
        int x = unitOnGridPosition.x;
        SetOnGridPosition(_x, _y);
        if (crouched && canMove)
        {
            crouch();
        }
        if (canMove)
        {
            Queue<GridTools.TileCoordinates> highlightQueue = moveHighlight.GetComponent<MoveHighlight>().getMovesQeue();
            Debug.Log("movesQueue: " + movesQueue.Count + " highlightQueue: " + highlightQueue.Count);
            while (highlightQueue.Count > 0)
            {
                movesQueue.Enqueue(highlightQueue.Dequeue());
                Debug.Log("movesQueue: " + movesQueue.Count + " highlightQueue: " + highlightQueue.Count);
            }
            if(!moveActive && movesQueue.Count > 0)
            {
                moveActive = true;
                while(movesQueue.Count > 0)
                {

                    GridTools.TileCoordinates newWalkPosition = movesQueue.Dequeue();
                    characterParts = GetComponentsInChildren<SpriteRenderer>();
                    characterPartsLayerOrder = new int[characterParts.Length];
                    for (int i = 0; i < characterParts.Length; i++)
                    {
                        characterPartsLayerOrder[i] = characterParts[i].sortingOrder;
                    }
                    SortLayers();
                    for (int i = 0; i < characterParts.Length; i++)
                    {
                        characterParts[i].sortingOrder = i + GridTools.OnGridObjectLayer(maxOnGridPosition.x, maxOnGridPosition.y, newWalkPosition.x, newWalkPosition.y);
                    }
                    bool _directional = (x != newWalkPosition.x);
                    bool _moveSide = (x < newWalkPosition.x);
                    weaponManager.MoveSide(_moveSide, _directional);
                    await WalkMovement(new Vector3(newWalkPosition.x - newWalkPosition.y * 0.5f, newWalkPosition.y));
                    onGridPosition = new Vector3(newWalkPosition.x - newWalkPosition.y * 0.5f, newWalkPosition.y);
                    x = newWalkPosition.x;
                }
                moveActive = false;
            }
        }
    }

    public async void Blink()
    {
        var endTime = Time.time + UnityEngine.Random.Range(2.0f, 6.0f);
        while (Time.time < endTime)
        {
            await Task.Yield();
        }
        if(faceAnimator)
        {
            faceAnimator.SetTrigger("Blink");
        }
        Blink();
    }

    public void crouch()
    {
        if (!moveActive)
        {
            if (movesCount > 0)
            {
                if (crouched)
                {
                    crouched = false;
                    unitAnimator.ResetTrigger("Crouch");
                    unitAnimator.SetTrigger("Uncrouch");
                    // uncrouch
                }
                else
                {
                    crouched = true;
                    unitAnimator.ResetTrigger("Uncrouch");
                    unitAnimator.SetTrigger("Crouch");
                    // crouch
                }
                takeMove();
            }
        }
    }

    public void SetOnGridPosition(int x, int y)
    {
        unitOnGridPosition = new GridTools.TileCoordinates(x, y);
    }

    public async Task WalkMovement(Vector3 _WalkTarget)
    {
        Vector3 _WalkPath = new Vector3(_WalkTarget.x - onGridPosition.x, _WalkTarget.y - onGridPosition.y, 0f);
        unitAnimator.ResetTrigger("EndWalk");
        unitAnimator.SetTrigger("StartWalk");
        var endTime = Time.time + 1f;
        while (Time.time < endTime)
        {
            transform.position += _WalkPath * Time.deltaTime;
            await Task.Yield();
        }
        unitAnimator.SetTrigger("EndWalk");
        await Task.Yield();
    }

    public void debugWalkAnimation()
    {
        unitAnimator.ResetTrigger("EndWalk");
        unitAnimator.SetTrigger("StartWalk");
        Invoke("debugWalkAnimationEnd", 2f);
        
    }

    public void debugWalkAnimationEnd()
    {
        unitAnimator.SetTrigger("EndWalk");
    }

    public void ShootAt(float lookTargetX, float lookTargetY, int x, int y)
    {
        if (!moveActive)
        {
            if (canShoot)
            {
                canShoot = false;
                weaponManager.AllLookTowards(lookTargetX, lookTargetY);
                weaponManager.Shoot();
                int DamageDealt = 50;   // for constant damage to improve later
                if(GridManager.Instance)
                {
                    GridManager.Instance.onBoardEntities[x, y].takeDamage(DamageDealt);
                }
                mag--;
                takeMove();
            }
        }
    }

    public void reload()
    {
        weaponManager.LookAtSide();
        weaponManager.Reload();
        mag = maxMag;
    }

    public async void SortLayers()
    {
        int temp_int;
        SpriteRenderer temp_SpriteRenderer;
        for (int i = 0; i < characterParts.Length; i++)
        {
            for(int j = 1; j < characterParts.Length - i; j++)
            {
                if(characterPartsLayerOrder[j] < characterPartsLayerOrder[j-1])
                {
                    temp_int = characterPartsLayerOrder[j];
                    temp_SpriteRenderer = characterParts[j];
                    characterParts[j] = characterParts[j - 1];
                    characterPartsLayerOrder[j] = characterPartsLayerOrder[j - 1];
                    characterParts[j - 1] = temp_SpriteRenderer;
                    characterPartsLayerOrder[j - 1] = temp_int;
                }
            }
            characterPartsLayerOrder[characterParts.Length - i - 1] = characterParts.Length - i - 1;
        }
        characterPartsLayerOrder[0] = 0;
        await Task.Yield();
    }

    public void SetData(UnitData n)
    {
        weaponManager.ResetGadgets();
        string _weaponAmmoType = "";
        hp = 100;
        onGridPosition = transform.position;
        data = n;
        unitNumberDisplay.text = data.UnitNumber.ToString();  // display number
        id = ((int)data.UnitNumber);
        team = data.UnitTeam;
        if(!team && !weaponManager.isFlipped())
        {
            weaponManager.FlipCharacter();
        }
        weaponManager.EquipWeapon(data.UnitWeapon, data.UnitUniform);
        weaponManager.EquipGadgets(data.gadget1, data.gadget2, data.UnitUniform);
        weaponManager.LookAtSide();
        switch (data.UnitWeapon)    // async?
        {
            case Unit.weapon.rifle:
                {
                    _weaponAmmoType = "Rifle";
                    range = 5; // to balance
                    maxMag = 5;
                    break;
                }
            default:
                break;
        }
        DressUp(data.UnitUniform.ToString(), _weaponAmmoType);
        mag = maxMag;
        maxMoves = 4;
        switch (data.UnitRole)
        {
            case Unit.role.Infantryman:
                {
                    // maxMoves = 4;    // move here
                    break;
                }
            default:
                break;
        }
        characterParts = GetComponentsInChildren<SpriteRenderer>();
        characterPartsLayerOrder = new int[characterParts.Length];
        for (int i = 0; i < characterParts.Length; i++)
        {
            characterPartsLayerOrder[i] = characterParts[i].sortingOrder;
        }
        SortLayers(); 
        for (int i = 0; i < characterParts.Length; i++)
        {
            characterParts[i].sortingOrder = characterPartsLayerOrder[i] + GridTools.OnGridObjectLayer(maxOnGridPosition.x, maxOnGridPosition.y, unitOnGridPosition.x, unitOnGridPosition.y);
        }
        characterOutlines = GetComponentsInChildren<OutlineFx.Outline>();
        hideOutlines();
        canShoot = true;
    }
    
    private void DressUp (string setName, string ammoType)
    {
        unitShirt.SetCategoryAndLabel(unitShirt.GetCategory(), setName + "UniformShirt");
        unitShirtUpL.SetCategoryAndLabel(unitShirtUpL.GetCategory(), setName + "UniformShirtUpL");
        unitShirtUpR.SetCategoryAndLabel(unitShirtUpR.GetCategory(), setName + "UniformShirtUpR");
        unitShirtDnL.SetCategoryAndLabel(unitShirtDnL.GetCategory(), setName + "UniformShirtDnL");
        unitShirtDnR.SetCategoryAndLabel(unitShirtDnR.GetCategory(), setName + "UniformShirtDnR");
        unitPants.SetCategoryAndLabel(unitPants.GetCategory(), setName + "UniformPants");
        unitPantsUpL.SetCategoryAndLabel(unitPantsUpL.GetCategory(), setName + "UniformPantsUpL");
        unitPantsUpR.SetCategoryAndLabel(unitPantsUpR.GetCategory(), setName + "UniformPantsUpR");
        unitPantsDnL.SetCategoryAndLabel(unitPantsDnL.GetCategory(), setName + "UniformPantsDnL");
        unitPantsDnR.SetCategoryAndLabel(unitPantsDnR.GetCategory(), setName + "UniformPantsDnR");
        unitHelmet.SetCategoryAndLabel(unitHelmet.GetCategory(), (setName + "HelmetShell"));
        unitHelmetBackground.SetCategoryAndLabel(unitHelmetBackground.GetCategory(), (setName + "HelmetBackground"));
        unitBootsUpL.SetCategoryAndLabel(unitBootsUpL.GetCategory(), (setName + "BootsUpL"));
        unitBootsUpR.SetCategoryAndLabel(unitBootsUpR.GetCategory(), (setName + "BootsUpR"));
        unitBootsDnL.SetCategoryAndLabel(unitBootsDnL.GetCategory(), (setName + "BootsDnL"));
        unitBootsDnR.SetCategoryAndLabel(unitBootsDnR.GetCategory(), (setName + "BootsDnR"));
        unitRig.SetCategoryAndLabel(unitRig.GetCategory(), (setName + "Rig"));
        unitAmmoPouch.SetCategoryAndLabel(unitAmmoPouch.GetCategory(), (setName + "Ammo" + ammoType));
    }

    public void giveMove()
    {
        canMove = true;
        movesCount = maxMoves;
    }

    public void takeMove()
    {
        movesCount--;
        if(movesCount<=0)
        {
            canMove = false;
        }
    }

    public bool _canMove()
    {
        return canMove;
    }

    public void takeDamage( int damage)
    {
        hp -= damage;
        healthBar.value = hp / 100;
        if (hp <= 0)
        {
            GridManager.Instance.eliminateUnit(team);
            die();
        }
        else
        {
            faceAnimator.SetTrigger("Hit");
        }

    }

    public void die()
    {
        helmet.transform.parent = null;
        float xVel = UnityEngine.Random.Range(3f, 5f);
        float yVel = UnityEngine.Random.Range(3f, 5f);
        if (!weaponManager.isFlipped())
        {
            xVel *= -1;
        }
        helmet.GetComponent<AmmoShell>().setVel(xVel, yVel);
        helmet.GetComponent<AmmoShell>().eject();
        Destroy(gameObject);
    }

    private void Awake()
    {
        if (ChoiceMenuManager.Instance)
        {
             ChoiceMenuManager.Instance.resetPreview += deleteUnit;
        }
        else if (GridManager.Instance)    
        {
            maxOnGridPosition.x = GridManager.Instance.getWidth();
            maxOnGridPosition.y = GridManager.Instance.getHeight();
            //to do reset board
        }
        Blink();
    }

    public void deleteUnit(object sender, EventArgs e)
    {
        ChoiceMenuManager.Instance.resetPreview -= deleteUnit;
        Destroy(gameObject);
    }
}
