using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SliceTest : MonoBehaviour
{
    [SerializeField] private Sliceable _sliceable;

    // Update is called once per frame
    private void Update()
    {
        if(!Input.anyKeyDown) return;
        _sliceable.Slice(new Plane(Random.onUnitSphere, Vector3.zero));
       
    }
}