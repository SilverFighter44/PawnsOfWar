using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public abstract class Gadget : MonoBehaviour
{
    [SerializeField] public GameObject gadgetObject, pouchObject, gripPointBase, gripPointL, gripPointR, shadow;
    [SerializeField] private int supply, cost = 1;
    [SerializeField] private bool spent = false;
    [SerializeField] public Animator handsAnimator;
    [SerializeField] private Vector3 gripOffsetNormal, gripOffsetFlipped;
    [SerializeField] public Unit unitScript;
    [SerializeField] public int target_x, target_y;
    [SerializeField] private SpriteRenderer itemRenderer;
    [SerializeField] private AnimationCurve curve_up, curve_down;


    public void SetUnitScript (Unit _unitScript)
    {
        unitScript = _unitScript;
    }

    public bool isSpent()
    {
        return spent;
    }

    public void spendItem()
    {
        supply--;
        if(supply <= 0)
        {
            spent = true;
        }
    }

    public void createShadow()
    {
       shadow =  Instantiate(shadow, unitScript.getPosition(), Quaternion.identity);
    }

    public void EquipGadget(GameObject handL_IK, GameObject handR_IK, Transform weaponPivot, Transform handR, bool flipped)
    {
        gripPointBase.transform.position = weaponPivot.position;
        gripPointBase.transform.parent = weaponPivot;
        handL_IK.transform.position = gripPointL.transform.position;
        handL_IK.transform.parent = gripPointL.transform;
        handR_IK.transform.position = gripPointR.transform.position;
        handR_IK.transform.parent = gripPointR.transform;
        if (flipped)
        {
            gadgetObject.transform.position = handR.position + gripOffsetNormal;
        }
        else
        {
            gadgetObject.transform.position = handR.position + gripOffsetFlipped;
        }
        gadgetObject.transform.Rotate(0f, 0f, 90f, Space.Self);
        gadgetObject.transform.parent = handR;
        handsAnimator.ResetTrigger("Unequip");
        handsAnimator.SetTrigger("Equip");
    }
 
    public void UnequipGadget()
    {
        if(gadgetObject && !spent)
        {
            gadgetObject.transform.position = pouchObject.transform.position;
            gadgetObject.transform.rotation = Quaternion.identity;
            gadgetObject.transform.parent = pouchObject.transform;
        }
        handsAnimator.ResetTrigger("Equip");
        handsAnimator.SetTrigger("Unequip");
    }


    public virtual void UseGadget(int x, int y)
    {
        if (!isSpent() && unitScript.howManyMoves() >= cost)
        {
            spendItem();
            target_x = x;
            target_y = y;
            handsAnimator.SetTrigger("Throw");
            for(int i = 0; i < cost; i++)
            {
                unitScript.takeMove();
            }
        }
    }

    public void Throw()
    {
        ThrowTrajectory();
        unitScript.unequipGadget();
        GridManager.Instance.ResetHighlights();
    }

    public async Task ThrowTrajectory() //to improve parabolic with lerp
    {
        createShadow();
        int _multiplier = StartData.Instance.getLayerMultiplier();
        int MaxHeight = GridManager.Instance.getHeight();
        int MaxWidth = GridManager.Instance.getWidth();
        gadgetObject.transform.SetParent(null, true);
        float randomY = Random.Range(-0.25f, 0.25f);
        float randomX = Random.Range(-0.25f, 0.25f);
        float _WalkPath = ((float)target_y + randomY) - gadgetObject.transform.position.y;

        Vector3 _ShadowWalkPath = new Vector3(((float)target_x + randomX) - ((float)target_y + randomY) * 0.5f - shadow.transform.position.x, ((float)target_y + randomY) - shadow.transform.position.y, 0f);
        var endTime = Time.time + 1f;
        while (Time.time < endTime)
        {
            shadow.transform.position += _ShadowWalkPath * Time.deltaTime;
            int _y = (int)(shadow.transform.position.y + 0.5f);
            int _x = (int)(shadow.transform.position.x + (shadow.transform.position.y + 0.5f - _y) * 0.5f + _y * 0.5f);
            itemRenderer.sortingOrder = 1 + _multiplier * (((MaxHeight - _y - 1) * MaxWidth + MaxWidth - _x - 2) + 2);
            float arcTime;
            if(endTime - Time.time > 0.5f)
            {
                arcTime = curve_up.Evaluate((0.5f - (endTime - 0.5f - Time.time)) / 0.5f);
            }
            else
            {
                arcTime = -1f * curve_down.Evaluate((0.5f - (endTime - Time.time)) / 0.5f);
            }
            gadgetObject.transform.position = new Vector3( shadow.transform.position.x, gadgetObject.transform.position.y +  arcTime * Time.deltaTime + _WalkPath * Time.deltaTime);
            await Task.Yield();
        }
        gadgetObject.transform.position = shadow.transform.position;
        GadgetAction();
        await Task.Yield();
    }

    public virtual void GadgetAction()
    {

    }
}
