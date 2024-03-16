using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeFragUS : Gadget, IGrenade
{
    [SerializeField] private Explosion pin, gadgetItemExplosion;
    [SerializeField] private AmmoShell spoon;
    [SerializeField] private GrenadeAnimation grenadeAnimation;
    [SerializeField] private GameObject explosion, warning;
    [SerializeField] private int activationTime = 2;

    public override void GadgetAction()
    {
        GridManager.Instance.HideWallForGadget(target_x, target_y);
        GridManager.Instance.nextTurn += delayedActivation;
        warning = Instantiate(warning, gadgetObject.transform.position, Quaternion.identity);
        warning.transform.parent = gadgetObject.transform;
    }

    public void delayedActivation(object sender, EventArgs e)
    {
        activationTime--;
        if (activationTime <= 0)
        {
            Instantiate(explosion, gadgetObject.transform.position, Quaternion.identity);
            GridManager.Instance.nextTurn -= delayedActivation;
            GridManager.Instance.GrenadeExplosion(target_x, target_y);
            gadgetItemExplosion.DestroyAfterExplosion();
            GridManager.Instance.UnHideWallForGadget(target_x, target_y);
        }
    }

    public void Start()
    {
        grenadeAnimation.SetGrenade(this);
    }

    public void PullThePin()
    {
        pin.DestroyAfterExplosion();
    }

    public void DropTheSpoon()
    {
        spoon.eject();
    }
}
