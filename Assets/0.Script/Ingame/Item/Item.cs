using System;
using Fusion;
using Fusion.XR.Shared.Grabbing;
using TMPro;
using UnityEngine;

public class Item : Grabbable
{
    [SerializeField] private TMP_Text ownerName;

    public void Init()
    {
        
    }

    public override void Grab(Grabber newGrabber, Transform grabPointTransform = null)
    {
        base.Grab(newGrabber, grabPointTransform);
    }

    public override void Ungrab()
    {
        base.Ungrab();
    }
}