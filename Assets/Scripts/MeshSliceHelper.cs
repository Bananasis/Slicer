using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;

public class MeshSliceHelper
{
    private readonly List<Vector3> _pointsAlongPlane;
    private Plane _plane;
    private Mesh _mesh;

    private MeshBuilder topMeshBuilder;
    private MeshBuilder bottomMeshBuilder;

    public MeshSliceHelper(Plane plane)
    {
        _pointsAlongPlane = new List<Vector3>();
        _plane = plane;
        topMeshBuilder = new MeshBuilder();
        bottomMeshBuilder = new MeshBuilder();
    }


    private Vector3 GetNormal(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
    {
        var vec1 = vertex2 - vertex1;
        var vec2 = vertex3 - vertex1;
        return Vector3.Cross(vec1, vec2).normalized;
    }

    // normal data must be generated from vertices
    private void AddTrianglesNormalAndUvs(bool side, Vector3 vertex1, Vector2 uv1,
        Vector3 vertex2, Vector2 uv2, Vector3 vertex3, Vector2 uv3
    )
    {
        var normal = GetNormal(vertex1, vertex2, vertex3);
        AddTrianglesNormalAndUvs(side, vertex1, normal, uv1,
            vertex2, normal, uv2, vertex3, normal, uv3);
    }

    // case where normals are present (one sided triangle)
    private void AddTrianglesNormalAndUvs(bool side, Vector3 vertex1, Vector3 normal1, Vector2 uv1,
        Vector3 vertex2, Vector3 normal2, Vector2 uv2, Vector3 vertex3, Vector3 normal3, Vector2 uv3
    )
    {
        var meshBuilder = side ? topMeshBuilder : bottomMeshBuilder;
        meshBuilder.AddTrianglesNormalsAndUvs(vertex1, normal1, uv1, vertex2, normal2, uv2, vertex3, normal3, uv3);
    }


    public Mesh[] ComputeNewMeshes(Mesh mesh)
    {
        int[] meshTriangles = mesh.triangles;
        Vector3[] meshVerts = mesh.vertices;
        Vector3[] meshNormals = mesh.normals;
        Vector2[] meshUvs = mesh.uv;

        Vector3[] vertices = new Vector3[3];
        //int[] vertexIndices = new int[3];
        Vector2[] uvs = new Vector2[3];

        Vector3[] normals = new Vector3[3];
        bool[] sides = new bool[3];
        for (int i = 0; i < meshTriangles.Length; i += 3)
        {
            // loading all of the triangle data
            for (int j = 0; j < 3; j++)
            {
                var vertexIdx = meshTriangles[i + j];
                vertices[j] = meshVerts[vertexIdx];
                uvs[j] = meshUvs[vertexIdx];
                normals[j] = meshNormals[vertexIdx];
                sides[j] = _plane.GetSide(vertices[j]);
            }

            //whole trinagle on one side
            if (sides[0] == sides[1] && sides[1] == sides[2])
            {
                AddTrianglesNormalAndUvs(sides[0], vertices[0], normals[0], uvs[0], vertices[1], normals[1], uvs[1],
                    vertices[2], normals[2], uvs[2]);
            }
            else
            {
                //there are exactly 2 points of a triangle that are on a plane
                Vector3 intersection1;
                Vector3 intersection2;
                Vector2 intersection1Uv;
                Vector2 intersection2Uv;
                if (sides[0] == sides[1])
                {
                    
                    intersection1 = GetRayPlaneIntersectionPointAndUv(vertices[1], uvs[1], vertices[2], uvs[2],
                        out intersection1Uv);
                    intersection2 =
                        GetRayPlaneIntersectionPointAndUv(vertices[2], uvs[2], vertices[0], uvs[0],
                            out intersection2Uv);

                    //top
                    AddTrianglesNormalAndUvs(sides[0], vertices[0], uvs[0], vertices[1], uvs[1],
                        intersection1,
                        intersection1Uv);
                    AddTrianglesNormalAndUvs(sides[0], vertices[0], uvs[0], intersection1, intersection1Uv,
                        intersection2, intersection2Uv);
                    //bottom
                    AddTrianglesNormalAndUvs(!sides[0], intersection1, intersection1Uv, vertices[2], uvs[2],
                        intersection2, intersection2Uv);
                }

                else if (sides[0] == sides[2])
                {
                    intersection1 =
                        GetRayPlaneIntersectionPointAndUv(vertices[0], uvs[0], vertices[1], uvs[1],
                            out intersection1Uv);
                    intersection2 = GetRayPlaneIntersectionPointAndUv(vertices[1], uvs[1], vertices[2], uvs[2],
                        out intersection2Uv);

                    //top
                    AddTrianglesNormalAndUvs(sides[0], vertices[0], uvs[0], intersection1, intersection1Uv,
                        vertices[2],
                        uvs[2]);
                    AddTrianglesNormalAndUvs(sides[0], intersection1, intersection1Uv, intersection2,
                        intersection2Uv, vertices[2], uvs[2]);
                    //botton
                    AddTrianglesNormalAndUvs(!sides[0], intersection1, intersection1Uv, vertices[1], uvs[1],
                        intersection2, intersection2Uv);
                }

                else
                {
                    intersection1 =
                        GetRayPlaneIntersectionPointAndUv(vertices[0], uvs[0], vertices[1], uvs[1],
                            out intersection1Uv);
                    intersection2 =
                        GetRayPlaneIntersectionPointAndUv(vertices[0], uvs[0], vertices[2], uvs[2],
                            out intersection2Uv);
                    //top
                    AddTrianglesNormalAndUvs(sides[0], vertices[0], uvs[0], intersection1, intersection1Uv,
                        intersection2, intersection2Uv);
                    //bottom
                    AddTrianglesNormalAndUvs(!sides[0], intersection1, intersection1Uv, vertices[1], uvs[1],
                        vertices[2],
                        uvs[2]);
                    AddTrianglesNormalAndUvs(!sides[0], intersection1, intersection1Uv, vertices[2], uvs[2],
                        intersection2, intersection2Uv);
                }


                _pointsAlongPlane.Add(intersection1);
                _pointsAlongPlane.Add(intersection2);
            }
        }

        JoinPointsAlongPlane();
        return new[] {topMeshBuilder.mesh, bottomMeshBuilder.mesh};
    }

    private Vector3 GetMiddle()
    {
        Vector3 sum = default;
        for (int i = 0; i < _pointsAlongPlane.Count; i++)
        {
            sum += _pointsAlongPlane[i];
        }

        return sum / _pointsAlongPlane.Count;
    }

    private void JoinPointsAlongPlane()
    {
        Vector3 middle = GetMiddle();

        for (int i = 0; i < _pointsAlongPlane.Count; i += 2)
        {
            var firstVertex = _pointsAlongPlane[i];
            var secondVertex = _pointsAlongPlane[i + 1];

            Vector3 normal3 = GetNormal(middle, secondVertex, firstVertex);
            normal3.Normalize();

            // attention! not the same as plane.getside() IT IS NORMAL. NOT THE POINT !!!! >:(
            var direction = Vector3.Dot(normal3, _plane.normal);
            var flipped = direction < 0;
            // todo add some way to modify uv resolution strategy
            AddTrianglesNormalAndUvs(!flipped, middle, -normal3, Vector2.zero, firstVertex, -normal3,
                Vector2.zero, secondVertex, -normal3, Vector2.zero);
            AddTrianglesNormalAndUvs(flipped, middle, normal3, Vector2.zero, secondVertex, normal3,
                Vector2.zero, firstVertex, normal3, Vector2.zero);
        }
    }


    private float GetDistanceRelativeToPlane(Vector3 vertex1, Vector3 vertex2, out Vector3 pointOfintersection)
    {
        Ray ray = new Ray(vertex1, (vertex2 - vertex1));
        _plane.Raycast(ray, out float distance);
        pointOfintersection = ray.GetPoint(distance);
        return distance;
    }

    private Vector2 InterpolateUvs(Vector2 uv1, Vector2 uv2, float distance)
    {
        Vector2 uv = Vector2.Lerp(uv1, uv2, distance);
        return uv;
    }

    private Vector3 GetRayPlaneIntersectionPointAndUv(Vector3 vertex1, Vector2 vertex1Uv, Vector3 vertex2,
        Vector2 vertex2Uv, out Vector2 uv)
    {
        float distance = GetDistanceRelativeToPlane(vertex1, vertex2, out Vector3 pointOfIntersection);
        uv = InterpolateUvs(vertex1Uv, vertex2Uv, distance);
        return pointOfIntersection;
    }
}