# Larje Graphics

`com.larje.graphics`: URP graphics tools for Unity 6.

## Install

UPM cannot resolve git dependencies, so add UniTask, core and this package to `Packages/manifest.json`:

```json
"com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.5.10",
"com.larje.core": "https://github.com/danil777lar/unity-gameplay-core-services.git",
"com.larje.graphics": "https://github.com/danil777lar/unity-urp-graphics-toolkit.git"
```

Requires URP 17+ and Shader Graph.

## Contents

| Feature | Types | Notes |
|---|---|---|
| Post FX | `PostFXFeature` (ScriptableRendererFeature), `PostFXEffect`, `ShaderValueEffect` | Add the feature to a URP renderer and list effect assets on it |
| Outline | `OutlineDrawer`, `OutlineDrawerGlowAnim` | Mesh outlines with an optional glow animation |
| Volumetric light | `VolumetricLightMesh`, `VolumetricLightMeshCluster` | Mesh-based light shafts |
| Trajectory | `TrajectoryDrawer` | Draws a ballistic trajectory line |
| Utility | `GradientTextureGenerator` | Bakes gradients into textures for shaders |
| Lightmaps | `LightmapPackConfig`, `LightmapLocationAction` | Switches lightmap packs when a Larje location loads. Lives in `Larje.Graphics.Lightmaps` and needs Addressables (`LARJE_ADDRESSABLES`) |

`Assets/` holds Shader Graph shaders (toon/main, particles, UI, VFX, CRT/LCD displays, grid, triplanar UV), particle materials, meshes and textures.
