using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class MeshBuilder
    {
        private List<Vector3> _vertices;
        private List<int> _triangles;
        private List<Vector2> _uvs;
        private List<Vector3> _normals;


        public Mesh mesh
        {
            get
            {
                var _mesh = new Mesh();
                _mesh.vertices = _vertices.ToArray();
                _mesh.triangles = _triangles.ToArray();
                _mesh.normals = _normals.ToArray();
                _mesh.uv = _uvs.ToArray();
                return _mesh;
            }
        }

        public MeshBuilder()
        {
            _triangles = new List<int>();
            _vertices = new List<Vector3>();
            _uvs = new List<Vector2>();
            _normals = new List<Vector3>();
        }

        private void AddVertNormalUv(List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs,
            List<int> triangles, Vector3 vertex, Vector3 normal, Vector2 uv)
        {
            vertices.Add(vertex);
            normals.Add(normal);
            uvs.Add(uv);
            triangles.Add(vertices.Count - 1);
        }

        public void AddTrianglesNormalsAndUvs(Vector3 vertex1, Vector3 normal1, Vector2 uv1,
            Vector3 vertex2, Vector3 normal2, Vector2 uv2, Vector3 vertex3, Vector3 normal3, Vector2 uv3)
        {
            AddVertNormalUv(_vertices, _normals, _uvs, _triangles, vertex1, normal1, uv1);
            AddVertNormalUv(_vertices, _normals, _uvs, _triangles, vertex2, normal2, uv2);
            AddVertNormalUv(_vertices, _normals, _uvs, _triangles, vertex3, normal3, uv3);
        }
    }
}