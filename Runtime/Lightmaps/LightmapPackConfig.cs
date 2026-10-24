using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Larje.Graphics.Lightmaps
{
    public sealed class LightmapPackConfig : ScriptableObject
    {
    #if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Lightmap Pack/Export Active Scene")]
        private static void ExportActiveScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.isLoaded)
            {
                Debug.LogError("Active scene is not loaded.");
                return;
            }

            LightmapData[] lightmaps = LightmapSettings.lightmaps;
            if (lightmaps == null || lightmaps.Length == 0)
            {
                Debug.LogError("No baked lightmaps found. Bake lighting first.");
                return;
            }

            string fileName = $"Lightpack_{scene.name.Replace(" ", "_").Replace(".","_")}";
            string path = UnityEditor.EditorUtility.SaveFilePanelInProject("Save LightmapSet", $"{fileName}_", "asset", "Choose where to save the LightmapSet asset");
            string dirName = $"{System.IO.Path.GetFileNameWithoutExtension(path)}_Lightmaps";
            string lmDir = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), dirName).Replace("\\", "/");

            EnsureFolderExists(lmDir);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            LightmapPackConfig asset = ScriptableObject.CreateInstance<LightmapPackConfig>();

            string packName = System.IO.Path.GetFileNameWithoutExtension(path);

            ExportLightmaps(asset, lightmaps, path, lmDir, packName);
            ExportLightprobes(asset, lmDir, packName);

            UnityEditor.AssetDatabase.CreateAsset(asset, path);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"Exported LightmapSet: {path}\n:Lightmaps Count{asset.lightmaps.Length}\nMeshRenderers mapped: {asset.rendererEntries.Length}");
            UnityEditor.Selection.activeObject = asset;
        }

        private static void ExportLightmaps(LightmapPackConfig asset, LightmapData[] lightmaps, string path, string lmDir, string packName)
        {
            int n = lightmaps.Length;
            asset.lightmaps = new LightmapSaveData[n];

            for (int i = 0; i < n; i++)
            {
                asset.lightmaps[i].color = SaveTextureToAddressables(lightmaps[i].lightmapColor, packName, lmDir);
                asset.lightmaps[i].direction = SaveTextureToAddressables(lightmaps[i].lightmapDir, packName, lmDir);
                asset.lightmaps[i].shadow = SaveTextureToAddressables(lightmaps[i].shadowMask, packName, lmDir);
            }

            List<string> rendNames = new List<string>();
            List<RendererEntry> list = new List<RendererEntry>();
#if UNITY_6000_4_OR_NEWER
            MeshRenderer[] allSceneRenderers = UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include);
#else
            MeshRenderer[] allSceneRenderers = UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
#endif

            foreach (MeshRenderer rend in allSceneRenderers)
            {
                if (rend.lightmapIndex >= 0)
                {
                    RendererEntry entry = new RendererEntry
                    {
                        path = GetTransformPath(rend.transform),
                        lightmapIndex = rend.lightmapIndex,
                        scaleOffset = rend.lightmapScaleOffset
                    };

                    if (rendNames.Contains(entry.path))
                    {
                        Debug.LogError($"Duplicate renderer path: {entry.path}. Skipping.", rend);
                    }
                    else
                    {
                        rendNames.Add(entry.path);
                        list.Add(entry);
                    }
                }
            }

            asset.rendererEntries = list.ToArray();
        }

        public static void ExportLightprobes(LightmapPackConfig asset, string targetDir, string packName)
        {
            LightProbes probes = LightmapSettings.lightProbes;
            if (probes == null)
            {
                Debug.LogError("LightmapSettings.lightProbes is null");
                return;
            }

            string fullPath = System.IO.Path.Combine(targetDir, $"{packName}_lightProbes.asset");
            string targetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(fullPath);
            string path = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(fullPath);

            UnityEngine.Object copy = UnityEngine.Object.Instantiate(probes);
            UnityEditor.AssetDatabase.CreateAsset(copy, path);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            asset.probePath = SaveToAddressables(path, packName);
        }

        private static string SaveTextureToAddressables(Texture2D src, string packName, string targetDir)
        {
            string address = "";

            if (src == null)
            {
                return null;
            }

            string srcPath = UnityEditor.AssetDatabase.GetAssetPath(src);
            if (string.IsNullOrEmpty(srcPath))
            {
                Debug.LogWarning($"Texture '{src.name}' has no asset path (not an asset?). Skipping copy.");
                return address;
            }

            string ext = System.IO.Path.GetExtension(srcPath);
            if (string.IsNullOrEmpty(ext))
            {
                ext = ".asset";
            }

            string fullPath = System.IO.Path.Combine(targetDir, $"{packName}_{src.name}{ext}");
            string targetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(fullPath);

            bool ok = UnityEditor.AssetDatabase.CopyAsset(srcPath, targetPath);
            if (!ok)
            {
                Debug.LogWarning($"Failed to copy '{srcPath}' -> '{targetPath}'. Using original reference.");
                return address;
            }

            UnityEditor.AssetDatabase.ImportAsset(targetPath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            return SaveToAddressables(targetPath, packName);
        }

        private static string SaveToAddressables(string targetPath, string packName)
        {
            string address = "";
            string guid = UnityEditor.AssetDatabase.AssetPathToGUID(targetPath);

            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogError($"Failed to get GUID for copied asset: {targetPath}");
                return null;
            }

            UnityEditor.AddressableAssets.Settings.AddressableAssetSettings settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("Addressables settings not found.");
                return null;
            }

            UnityEditor.AddressableAssets.Settings.AddressableAssetGroup group = GetOrCreateGroup(packName);
            if (group == null)
            {
                Debug.LogError("Default Addressables group not found.");
                return null;
            }

            UnityEditor.AddressableAssets.Settings.AddressableAssetEntry entry = settings.FindAssetEntry(guid);
            if (entry == null)
            {
                entry = settings.CreateOrMoveEntry(guid, group);
            }
            else if (entry.parentGroup != group)
            {
                settings.MoveEntry(entry, group);
            }

            address = System.IO.Path.GetFileNameWithoutExtension(targetPath);
            entry.address = address;

            UnityEditor.EditorUtility.SetDirty(settings);
            UnityEditor.AssetDatabase.SaveAssets();

            return address;
        }

        private static UnityEditor.AddressableAssets.Settings.AddressableAssetGroup GetOrCreateGroup(string groupName)
        {
            UnityEditor.AddressableAssets.Settings.AddressableAssetSettings settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("Addressables settings not found.");
                return null;
            }

            UnityEditor.AddressableAssets.Settings.AddressableAssetGroup group = settings.FindGroup(groupName);
            if (group != null)
            {
                return group;
            }

            group = settings.CreateGroup(groupName, false, false, true, null, 
                typeof(UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema),
                typeof(UnityEditor.AddressableAssets.Settings.GroupSchemas.ContentUpdateGroupSchema));

            UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema bundledSchema = group.GetSchema<UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema>();
            if (bundledSchema != null)
            {
                bundledSchema.BundleMode = UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            }

            UnityEditor.EditorUtility.SetDirty(settings);
            UnityEditor.AssetDatabase.SaveAssets();

            return group;
        }

        private static void EnsureFolderExists(string assetFolderPath)
        {
            if (UnityEditor.AssetDatabase.IsValidFolder(assetFolderPath))
            {
                return;
            }

            string[] parts = assetFolderPath.Split('/');
            if (parts.Length == 0 || parts[0] != "Assets")
            {
                throw new Exception("Folder path must start with 'Assets/'");
            }

            string current = "Assets";
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!UnityEditor.AssetDatabase.IsValidFolder(next))
                {
                    UnityEditor.AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }

        private static string GetTransformPath(Transform t)
        {
            Stack<string> stack = new Stack<string>();
            while (t != null)
            {
                stack.Push(t.name);
                t = t.parent;
            }
            return string.Join("/", stack);
        }
    #endif


        [SerializeField] private LightmapSaveData[] lightmaps;
        [SerializeField] private string probePath;
        [Space]
        [SerializeField] private RendererEntry[] rendererEntries;

        [ContextMenu("Apply")]
        private void ApplyFromMenu()
        {
            ApplyAsync().Forget();
        }

        public async UniTask ApplyAsync()
        {
            LightmapData[] lightmaps = await LoadLightmapsAsync();
            LightmapSettings.lightmaps = lightmaps;

            foreach (RendererEntry entry in rendererEntries)
            {
                Transform t = FindTransformByPath(entry.path);
                if (t != null)
                {
                    MeshRenderer rend = t.GetComponent<MeshRenderer>();
                    if (rend != null)
                    {
                        rend.lightmapIndex = entry.lightmapIndex;
                        rend.lightmapScaleOffset = entry.scaleOffset;
                    }
                }
            }

            ApplyLightprobes();
        }

        public async UniTask<LightmapData[]> LoadLightmapsAsync()
        {
            int n = lightmaps.Length;
            LightmapData[] arr = new LightmapData[n];

            for (int i = 0; i < n; i++)
            {
                LightmapSaveData lm = lightmaps[i];

                Texture2D color = null;
                Texture2D dir = null;
                Texture2D mask = null;

                if (!string.IsNullOrEmpty(lm.color))
                {
                    color = await Addressables.LoadAssetAsync<Texture2D>(lm.color).ToUniTask();
                }

                if (!string.IsNullOrEmpty(lm.direction))
                {
                    dir = await Addressables.LoadAssetAsync<Texture2D>(lm.direction).ToUniTask();
                }

                if (!string.IsNullOrEmpty(lm.shadow))
                {
                    mask = await Addressables.LoadAssetAsync<Texture2D>(lm.shadow).ToUniTask();
                }

                LightmapData d = new LightmapData();
                d.lightmapColor = color;
                d.lightmapDir = dir;
                d.shadowMask = mask;

                arr[i] = d;
            }

            return arr;
        }

        private void ApplyLightprobes()
        {
            if (string.IsNullOrEmpty(probePath))
            {
                Debug.LogError("Probe asset path is empty.");
                return;
            }

            LightProbes loaded = Addressables.LoadAssetAsync<LightProbes>(probePath).WaitForCompletion();
            if (loaded == null)
            {
                Debug.LogError($"Failed to load LightProbes asset at path: {probePath}");
                return;
            }

            LightProbes copy = UnityEngine.Object.Instantiate(loaded);
            LightmapSettings.lightProbes = copy;
        }

        private SphericalHarmonicsL2 DeserializeSH(float[] data)
        {
            SphericalHarmonicsL2 sh = new SphericalHarmonicsL2();

            if (data == null || data.Length != 27)
            {
                Debug.LogError("Invalid SH coefficient array.");
                return sh;
            }

            int k = 0;
            for (int rgb = 0; rgb < 3; rgb++)
            {
                for (int coef = 0; coef < 9; coef++)
                {
                    sh[rgb, coef] = data[k++];
                }
            }

            return sh;
        }

        private Transform FindTransformByPath(string path, Transform root = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            string[] parts = path.Split('/');

            Transform current = null;
            if (root != null)
            {
                if (root.name != parts[0])
                {
                    return null;
                }

                current = root;
                for (int i = 1; i < parts.Length; i++)
                {
                    current = current.Find(parts[i]);
                    if (current == null)
                    {
                        return null;
                    }
                }
                return current;
            }

            Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            GameObject[] roots = scene.GetRootGameObjects();

            foreach (GameObject go in roots)
            {
                if (go.name == parts[0])
                {
                    Transform found = FindChildPath(go.transform, parts);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }

        private static Transform FindChildPath(Transform root, string[] parts)
        {
            Transform current = root;
            for (int i = 1; i < parts.Length && current != null; i++)
            {
                current = current.Find(parts[i]);
            }

            return current;
        }

        [Serializable]
        public struct LightmapSaveData
        {
            public string color;
            public string direction;
            public string shadow;
        }    

        [Serializable]
        public class LightprobeSaveData
        {
            public Vector3 position;
            public float[] coefficients = new float[27];
        }

        [Serializable]
        public struct RendererEntry
        {
            public string path;
            public int lightmapIndex;
            public Vector4 scaleOffset;
        }   
    }
}
