using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeAnimation : MonoBehaviour
{
    private IGrenade grenade;

    public void SetGrenade (IGrenade _grenade)
    {
        grenade = _grenade;
    }

    public void Throw()
    {
        grenade.Throw();
    }
    public void PullThePin()
    {
        grenade.PullThePin();
    }
    public void DropTheSpoon()
    {
        grenade.DropTheSpoon();
    }
}
