using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Larje.Graphics.PostFX
{
    public sealed class PostFXPass : ScriptableRenderPass
    {
        private const string PASS_NAME = "LarjePostFX";

        private readonly IReadOnlyList<PostFXProcessor> _processors;

        public PostFXPass(IReadOnlyList<PostFXProcessor> processors, RenderPassEvent injectionPoint)
        {
            _processors = processors;
            renderPassEvent = injectionPoint;
            requiresIntermediateTexture = true;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resources = frameData.Get<UniversalResourceData>();
            if (resources.isActiveTargetBackBuffer)
            {
                return;
            }

            foreach (PostFXProcessor processor in _processors)
            {
                processor.Update();
                if (processor.Material != null && processor.Enabled)
                {
                    TextureHandle source = resources.activeColorTexture;
                    TextureDesc description = renderGraph.GetTextureDesc(source);
                    description.name = "CameraColor-" + PASS_NAME;
                    description.clearBuffer = false;

                    TextureHandle destination = renderGraph.CreateTexture(description);
                    RenderGraphUtils.BlitMaterialParameters parameters = new RenderGraphUtils.BlitMaterialParameters(source, destination, processor.Material, 0);
                    renderGraph.AddBlitPass(parameters, PASS_NAME);
                    resources.cameraColor = destination;
                }
            }
        }
    }
}
