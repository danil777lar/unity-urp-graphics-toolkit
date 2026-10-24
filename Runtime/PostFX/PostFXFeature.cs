using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Larje.Graphics.PostFX
{
    public sealed class PostFXFeature : ScriptableRendererFeature
    {
        private static PostFXFeature s_active;

        [SerializeField] private Settings settings = new Settings();

        private PostFXPass _pass;
        private readonly List<PostFXProcessor> _processors = new List<PostFXProcessor>();

        public static bool TryGetProcessor<T>(PostFXEffect effect, out T processor) where T : PostFXProcessor
        {
            processor = null;
            if (s_active == null)
            {
                return false;
            }

            foreach (PostFXProcessor candidate in s_active._processors)
            {
                if (candidate.Effect == effect && candidate is T typed)
                {
                    processor = typed;
                    return true;
                }
            }

            return false;
        }

        public override void Create()
        {
            ReleaseProcessors();
            foreach (PostFXEffect effect in settings.Effects)
            {
                if (effect != null)
                {
                    PostFXProcessor processor = effect.CreateProcessor();
                    if (processor != null)
                    {
                        _processors.Add(processor);
                    }
                }
            }

            _pass = new PostFXPass(_processors, settings.InjectionPoint);
            s_active = this;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_processors.Count == 0)
            {
                return;
            }

            if (!settings.RunInSceneView && renderingData.cameraData.isSceneViewCamera)
            {
                return;
            }

            if (settings.OnlyMainCamera && renderingData.cameraData.camera != Camera.main && !renderingData.cameraData.isSceneViewCamera)
            {
                return;
            }

            renderer.EnqueuePass(_pass);
        }

        protected override void Dispose(bool disposing)
        {
            ReleaseProcessors();
            if (s_active == this)
            {
                s_active = null;
            }
        }

        private void ReleaseProcessors()
        {
            foreach (PostFXProcessor processor in _processors)
            {
                processor.Destroy();
            }

            _processors.Clear();
        }

        [Serializable]
        private sealed class Settings
        {
            public RenderPassEvent InjectionPoint = RenderPassEvent.BeforeRenderingPostProcessing;
            public bool RunInSceneView = true;
            public bool OnlyMainCamera = true;
            public List<PostFXEffect> Effects = new List<PostFXEffect>();
        }
    }
}
