using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Follower : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private GameObject rotationParent;
    [SerializeField] private float cameraPanningDuration = 0.3f;

    private Vector3 _rotationParentStartingLocalEulerAngles;

    private Vector3 _startingLocalPosition;
    
    private void Awake()
    {
        _rotationParentStartingLocalEulerAngles = rotationParent.transform.localEulerAngles;
        _startingLocalPosition = transform.localPosition;
    }

    public void ResetCamera()
    {
        rotationParent.transform.DORotate(_rotationParentStartingLocalEulerAngles, 0.01f);
        transform.localPosition = _startingLocalPosition;
    }
    
    public void RotateAfterLaunch()
    {
        var targetLocalEulerAngles = target.transform.localEulerAngles;
        rotationParent.transform.DORotate(targetLocalEulerAngles, cameraPanningDuration);
        /* bug: if user stars gliding and turning before camera panning is complete,
        camera is sometimes left slightly off center */
    }
}
