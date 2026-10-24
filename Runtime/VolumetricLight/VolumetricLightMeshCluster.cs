using System.Collections.Generic;
using Larje.Core.Saves;
using UnityEngine;

namespace Larje.Graphics.VolumetricLight
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class VolumetricLightMeshCluster : MonoBehaviour
    {
        [SerializeField] private VolumetricLightMaterials materials = new VolumetricLightMaterials();

        private ISaveService _saves;

#if LARJE_ZENJECT
        [Zenject.Inject]
#endif
        public void Construct(ISaveService saves)
        {
            _saves = saves;
        }

        private void Start()
        {
            Material material = materials.Get(_saves);
            if (material != null)
            {
                GetComponent<MeshRenderer>().sharedMaterial = material;
            }

            GetComponent<MeshFilter>().sharedMesh = Build();
        }

        private Mesh Build()
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector4> positions = new List<Vector4>();
            List<Color> colors = new List<Color>();

            foreach (VolumetricLightMesh light in GetComponentsInChildren<VolumetricLightMesh>())
            {
                light.GetMeshData(out Vector3[] lightVertices, out int[] lightTriangles, out Vector2[] lightUvs, out Color[] lightColors);
                int offset = vertices.Count;
                foreach (Vector3 vertex in lightVertices)
                {
                    vertices.Add(transform.InverseTransformPoint(light.transform.TransformPoint(vertex)));
                    positions.Add(light.transform.localPosition);
                }

                foreach (int index in lightTriangles)
                {
                    triangles.Add(index + offset);
                }

                uvs.AddRange(lightUvs);
                colors.AddRange(lightColors);
            }

            Mesh mesh = new Mesh { name = "Volumetric Light Cluster" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetUVs(0, uvs);
            mesh.SetUVs(2, positions);
            mesh.SetColors(colors);
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}
