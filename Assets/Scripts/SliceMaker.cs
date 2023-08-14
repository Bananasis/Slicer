using System;
using Unity.VisualScripting;
using UnityEngine;

public class SliceMaker
{
    private Plane _plane { get; }

    public SliceMaker(Plane plane)
    {
        _plane = plane;
    }


    public Sliceable[] MakeSlices(Sliceable sliceable)
    {
       
        Mesh mesh = sliceable.mesh;
        var a = mesh.GetSubMesh(0);

        //top and bottom slice of an object
        Mesh[] newMeshes = new MeshSliceHelper(_plane).ComputeNewMeshes(mesh);

        // todo make pooling/ recycling of objects
        Sliceable topObject = GameObject.Instantiate(sliceable);
        topObject.gameObject.SetActive(false);
       // topObject.name = $"{sliceable.name}Top";
        Sliceable bottomObject = GameObject.Instantiate(sliceable);
        bottomObject.gameObject.SetActive(false);
       // bottomObject.name = $"{sliceable.name}Bottom";

        var topMeshData = newMeshes[0]; 
        var bottomMeshData = newMeshes[1]; 

        topObject.meshFilter.mesh = topMeshData;
        bottomObject.meshFilter.mesh = bottomMeshData;

        SetColliderMeshAndRigidBody( topObject, topMeshData);
        SetColliderMeshAndRigidBody( bottomObject, bottomMeshData);

        return new Sliceable[] {topObject, bottomObject};
    }

    private static void SetColliderMeshAndRigidBody( Sliceable sliceable, Mesh mesh)
    {
        sliceable.meshCollider.sharedMesh = mesh;
        sliceable.meshCollider.convex = true;
        sliceable.sliceRigidbody.useGravity = true;
    }
}