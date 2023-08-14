using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trajectory : MonoBehaviour
{
    [SerializeField] private LineRenderer line;
    [SerializeField] private int probes;

    private Vector3[] points;

    private void Start()
    {
        points = new Vector3[probes];
        line.positionCount = 50;
    }

    public void Hide()
    {
        line.enabled = false;
    }

    public void MakeLine(Vector3 pos, Vector3 velocity, Vector3 acceleration, float flyTime)
    {
        line.enabled = true;
        float physicsTime = flyTime;
        float probeTime = flyTime;
        float probeDeltaTime = flyTime / probes;
        int probeIdx = 0;
        Vector3 curPos = pos;
        

        while (probeTime > 0  && probeIdx < probes)
        {
            if (physicsTime <= probeTime)
            {
                points[probeIdx] = curPos;
                probeTime -= probeDeltaTime;
                probeIdx++;
            }

            curPos += velocity * Time.fixedDeltaTime;
            velocity += acceleration * Time.fixedDeltaTime;
            physicsTime -= Time.fixedDeltaTime;
        }

        for (int i = probeIdx; i < probes; i++)
        {
            points[i] = points[probeIdx - 1];
        }
        line.SetPositions(points);
    }
}