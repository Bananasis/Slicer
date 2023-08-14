using System.Collections;
using UnityEngine;

public class Slicer : MonoBehaviour
{
    [SerializeField] private Transform source;
    [SerializeField] private Transform sink;
    [SerializeField] private Transform focus;
    [SerializeField] private float speed = 2;
    [SerializeField] private float flyTime = 2;
    [SerializeField] private Trajectory _trajectory;
    private bool thrown;

    IEnumerator Throw(float time)
    {
        thrown = true;
        var dir = sink.position - transform.position;
        var velocity = speed * (_transform.position - source.position);
        var acceleration = (dir - time * velocity) * 2 / (time * time);
        var wait = new WaitForFixedUpdate();
        while (time > 0)
        {
            yield return wait;
            time -= Time.fixedDeltaTime;
            transform.position += velocity * Time.fixedDeltaTime;
            velocity += acceleration * Time.fixedDeltaTime;

            var rotation = Quaternion.LookRotation(velocity, Vector3.up);
            var focusVector = focus.position - _transform.position;
            focusVector.z = 0;
            transform.rotation = rotation * Quaternion.FromToRotation(Vector3.up, focusVector);
        }

        thrown = false;
    }


    private Camera _camera;
    private Transform _transform;

    private void Start()
    {
        _transform = transform;
        _camera = Camera.main;
        InputManager.onAim += pos =>
        {
            if (thrown) return;
            var position = _transform.position;
            var dir = sink.position - transform.position;
            var velocity = speed * (position - source.position);
            var acceleration = (dir - flyTime * velocity) * 2 / (flyTime * flyTime);
            _trajectory.MakeLine(position, velocity, acceleration, flyTime);
            Vector3 posWithDepth = pos;
            posWithDepth.z = 2;
            transform.position = _camera.ScreenToWorldPoint(posWithDepth);
            var rotation = Quaternion.LookRotation(position - source.position, Vector3.up);
            var focusVector = focus.position - position;
            focusVector.z = 0;
            transform.rotation = rotation * Quaternion.FromToRotation(Vector3.up, focusVector);
        };

        InputManager.onThrow += () =>
        {
            if (thrown) return;
            StartCoroutine(Throw(flyTime));
            _trajectory.Hide();
        };
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out Sliceable sliceable)) return;
        //return if object is already sliced
        if (!sliceable.sliceable) return;

        var thisTransform = transform;
        var otherTransform = other.transform;

        //make a plane with normal perpendicular to the surface of a card and with central point of a card and transform it slicee local space
        var otherLocalNormal = otherTransform.localToWorldMatrix.transpose * thisTransform.up;
        var otherLocalPosition = otherTransform.InverseTransformPoint(thisTransform.position);
        var otherLocalNormalFlipped =
            Vector3.Dot(otherLocalNormal, Vector3.up) > 0 ? otherLocalNormal : -otherLocalNormal;
        var plane = new Plane(otherLocalNormalFlipped, otherLocalPosition);
        
        sliceable.Slice(plane);
    }
}