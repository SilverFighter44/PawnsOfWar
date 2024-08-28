using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeSmokeGer : Gadget, IGrenade
{
    [SerializeField] private Explosion gadgetItemExplosion;
    [SerializeField] private AmmoShell cap;
    [SerializeField] private GrenadeAnimation grenadeAnimation;
    [SerializeField] private GameObject explosion, pinString, warning;
    [SerializeField] private Transform pinStringPoint, smokePoint;

    public override void GadgetAction()
    {
        GridManager.Instance.HideWallForGadget(target_x, target_y);
        GameObject smoke = Instantiate(explosion, smokePoint.position, Quaternion.identity);
        smoke.GetComponent<Smoke>().setCoordinates(target_x, target_y);
        GridManager.Instance.nextTurn += delayedActivation;
        warning = Instantiate(warning, gadgetObject.transform.position, Quaternion.identity);
        warning.transform.parent = gadgetObject.transform;
    }

    public void delayedActivation(object sender, EventArgs e)
    {
        GridManager.Instance.nextTurn -= delayedActivation;
        GridManager.Instance.SmokeExplosion(target_x, target_y);
        gadgetItemExplosion.DestroyAfterExplosion();
        GridManager.Instance.UnHideWallForGadget(target_x, target_y);
    }

    public void Start()
    {
        grenadeAnimation.SetGrenade(this);
    }

    public void PullThePin()
    {
        cap.eject();
    }

    public void DropTheSpoon()
    {
        GameObject pinStringObject = Instantiate(pinString, pinStringPoint);
        AmmoShell pinStringEjection = pinStringObject.GetComponent<AmmoShell>();
        pinStringEjection.eject();
    }
}
