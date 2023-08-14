using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class Sliceable : MonoBehaviour
{
    public bool sliceable = true;
    public Mesh mesh => meshFilter.mesh;
    public MeshCollider meshCollider => _meshCollider;
    public Rigidbody sliceRigidbody => _sliceRigidbody;
    public MeshFilter meshFilter => _meshFilter;

    [SerializeField] private MeshCollider _meshCollider;
    [SerializeField] private Rigidbody _sliceRigidbody;
    [SerializeField] private MeshFilter _meshFilter;
    //cut object using plane in local coordinates
    public void Slice(Plane plane)
    {
        Sliceable[] slices = new SliceMaker(plane).MakeSlices(this);
        slices[0].sliceable = false;
        slices[1].sliceable = false;
        slices[0].gameObject.SetActive(true);
        slices[1].gameObject.SetActive(true);
        
        //todo make impulse direction dependant on cutting plane
        slices[0]._sliceRigidbody.AddForce(Random.insideUnitSphere*10,ForceMode.Impulse);
        slices[1]._sliceRigidbody.AddForce(Random.insideUnitSphere*10,ForceMode.Impulse);
        // todo make pooling/ recycling of objects
        Destroy(gameObject);
    }
}