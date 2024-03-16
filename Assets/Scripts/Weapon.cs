using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private Transform bone, ejectionPort;
    [SerializeField] private GameObject gripPointL, gripPointR, muzzle, muzzleFlash, shellPrefab, magazinePrefab, magazine;
    [SerializeField] private Animator weaponAnimator;
    [SerializeField] private Unit unitScript;
    [SerializeField] private bool flipped;

    // to do: weapon data inside weapon script

    public void SetFlipped( bool _flipped )
    {
        flipped = _flipped;
    }

    public void GetUnitScript(Unit _unitScript)
    {
        unitScript = _unitScript;
    }

    public void GetAGrip(GameObject gripL, GameObject gripR)
    {
        gripL.transform.position = gripPointL.transform.position;
        gripL.transform.parent = gripPointL.transform;
        gripR.transform.position = gripPointR.transform.position;
        gripR.transform.parent = gripPointR.transform;
    }

    public async Task RotateTowards(float x, float y, float adjustment)
    {
        Vector3 Diff = new Vector3(x, y, 0f) - new Vector3(bone.position.x, bone.position.y, 0f);
        float angle = Mathf.Atan2(Diff.y, Diff.x) * Mathf.Rad2Deg;
        bone.transform.rotation = Quaternion.AngleAxis(angle + adjustment, Vector3.forward);
        await Task.Yield();
    }

    public void Shoot(Unit.weapon _weapon, Unit.skin _skin)
    {
        switch (_weapon)
        {
            case Unit.weapon.rifle:
                {
                    ShootSequenceBoltAction();
                    break;
                }
            default:
                break;
        }
    }

    public void Reload(Unit.weapon _weapon, Unit.skin _skin)
    {
        switch (_weapon)
        {
            case Unit.weapon.rifle:
                {
                    ReloadSequenceBoltAction();
                    break;
                }
            default:
                break;
        }
    }

    public void PullOutMag()
    {
        magazine = Instantiate(magazinePrefab, gripPointR.transform.position + new Vector3(0.02f, 0.05f, 0f),  Quaternion.identity);
        if (flipped)
        {
            magazine.transform.localScale = new Vector3(magazine.transform.localScale.x * -1f, magazine.transform.localScale.y, magazine.transform.localScale.z);
            magazine.transform.position = new Vector3(magazine.transform.position.x - 0.04f, magazine.transform.position.y, magazine.transform.position.z);
        }
        magazine.transform.parent = gripPointR.transform;
    }

    public void InsertMag()
    {
        magazine.GetComponent<Explosion>().DestroyAfterExplosion();
    }

    public void EjectCase()
    {
        GameObject _shellCase = Instantiate(shellPrefab , ejectionPort.position, Quaternion.identity);
        _shellCase.GetComponent<SpriteRenderer>().sortingOrder = 15 + unitScript.GetSortingLayer();
        float xVel = Random.Range(3f, 5f);
        float yVel = Random.Range(3f, 5f);
        if(!flipped)
        {
            xVel *= -1;
        }
        _shellCase.GetComponent<AmmoShell>().setVel(xVel, yVel);
        _shellCase.GetComponent<AmmoShell>().eject();
    }

    public void FinishShootSequenceBoltAction()
    {
        weaponAnimator.ResetTrigger("LoadRound");
        Debug.Log(unitScript.getAmmo());
        if (unitScript.getAmmo() <= 0)
        {
            unitScript.reload();
        }
        else
        {
            unitScript.ReadyToShoot();
        }
    }

    public void FinishReloadSequenceBoltAction()
    {
        weaponAnimator.ResetTrigger("ReloadGun");
        unitScript.ReadyToShoot();
    }

    public void RemoveWeapon()
    {
        Destroy(gameObject);
    }

    void ShootSequenceBoltAction() // to do semiauto and auto
    {
        Instantiate(muzzleFlash, muzzle.transform);
        weaponAnimator.SetTrigger("LoadRound");
    }

    void ReloadSequenceBoltAction()
    {
        weaponAnimator.SetTrigger("ReloadGun");
    }
    
}
