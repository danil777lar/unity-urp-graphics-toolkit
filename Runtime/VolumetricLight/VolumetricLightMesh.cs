using System.Collections.Generic;
using Larje.Core.Saves;
using UnityEngine;

namespace Larje.Graphics.VolumetricLight
{
    public sealed class VolumetricLightMesh : MonoBehaviour
    {
        [Header("Build")]
        [SerializeField] private bool buildOnStart = true;
        [SerializeField] private Vector3 localDirection = Vector3.forward;

        [Header("Shape")]
        [SerializeField] private float length = 5f;
        [SerializeField, Min(3)] private int corners = 4;
        [SerializeField] private Vector2 sizeStart = Vector2.one * 0.5f;
        [SerializeField] private Vector2 sizeEnd = Vector2.one;

        [Header("Look")]
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private VolumetricLightMaterials materials = new VolumetricLightMaterials();

        private ISaveService _saves;

#if LARJE_ZENJECT
        [Zenject.Inject]
#endif
        public void Construct(ISaveService saves)
        {
            _saves = saves;
        }

        public void SetColor(Color color)
        {
            Mesh mesh = GetMeshFilter().sharedMesh;
            if (mesh == null)
            {
                return;
            }

            Color[] colors = new Color[mesh.vertexCount];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }

            mesh.colors = colors;
        }

        public void GetMeshData(out Vector3[] vertices, out int[] triangles, out Vector2[] uvs, out Color[] colors)
        {
            vertices = new Vector3[corners * 2];
            triangles = new int[corners * 6];
            uvs = new Vector2[corners * 2];
            colors = new Color[corners * 2];

            Vector3 forward = localDirection.normalized;
            Vector3 right = Vector3.Cross(forward, Vector3.up).normalized;
            if (right.sqrMagnitude < 0.001f)
            {
                right = Vector3.right;
            }

            Vector3 up = Vector3.Cross(right, forward).normalized;
            for (int i = 0; i < corners; i++)
            {
                float angle = Mathf.Deg2Rad * (i * 360f / corners + 45f);
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);
                float u = (float)i / corners;

                Vector3 start = right * (cos * sizeStart.x) + up * (sin * sizeStart.y);
                Vector3 end = start + forward * length + right * (cos * sizeEnd.x) + up * (sin * sizeEnd.y);
                vertices[i] = start;
                vertices[i + corners] = end;
                uvs[i] = new Vector2(u, 0f);
                uvs[i + corners] = new Vector2(u, 1f);
                colors[i] = defaultColor;
                colors[i + corners] = defaultColor;

                int next = (i + 1) % corners;
                triangles[i * 6] = i;
                triangles[i * 6 + 1] = i + corners;
                triangles[i * 6 + 2] = next;
                triangles[i * 6 + 3] = next;
                triangles[i * 6 + 4] = i + corners;
                triangles[i * 6 + 5] = next + corners;
            }
        }

        [ContextMenu("Generate Mesh")]
        public void GenerateMesh()
        {
            GetMeshData(out Vector3[] vertices, out int[] triangles, out Vector2[] uvs, out Color[] colors);
            List<Vector4> positions = new List<Vector4>(vertices.Length);
            for (int i = 0; i < vertices.Length; i++)
            {
                positions.Add(transform.localPosition);
            }

            Mesh mesh = new Mesh { name = "Volumetric Light Mesh" };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.SetUVs(2, positions);
            mesh.colors = colors;
            mesh.RecalculateNormals();
            GetMeshFilter().sharedMesh = mesh;
        }

        private void Start()
        {
            if (!buildOnStart)
            {
                return;
            }

            GenerateMesh();
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            Material material = materials.Get(_saves);
            if (meshRenderer != null && material != null)
            {
                meshRenderer.sharedMaterial = material;
            }
        }

        private MeshFilter GetMeshFilter()
        {
            MeshFilter filter = GetComponent<MeshFilter>();
            return filter != null ? filter : gameObject.AddComponent<MeshFilter>();
        }

        private void OnDrawGizmosSelected()
        {
            GetMeshData(out Vector3[] vertices, out int[] triangles, out Vector2[] uvs, out Color[] colors);
            int half = vertices.Length / 2;
            Gizmos.color = Color.white;
            for (int i = 0; i < half; i++)
            {
                Vector3 a = transform.TransformPoint(vertices[i]);
                Vector3 b = transform.TransformPoint(vertices[(i + 1) % half]);
                Vector3 c = transform.TransformPoint(vertices[i + half]);
                Vector3 d = transform.TransformPoint(vertices[(i + 1) % half + half]);
                Gizmos.DrawLine(a, b);
                Gizmos.DrawLine(a, c);
                Gizmos.DrawLine(c, d);
            }
        }
    }
}
