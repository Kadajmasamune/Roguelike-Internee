using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class WitchTimeBlitFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class WitchTimeSettings
    {
        public Material effectMaterial;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRendering;
    }

    public WitchTimeSettings settings = new WitchTimeSettings();
    WitchTimeBlitPass pass;

    public override void Create()
    {
        pass = new WitchTimeBlitPass(settings.effectMaterial)
        {
            renderPassEvent = settings.renderPassEvent
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }

    class WitchTimeBlitPass : ScriptableRenderPass
    {
        private Material material;

        public WitchTimeBlitPass(Material material)
        {
            this.material = material;
        }

        class PassData
        {
            public Material material;
            public TextureHandle source;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            renderGraph.AddRenderPass<PassData>("WitchTimePass", out var passData)
                .SetRenderFunc((PassData data, RenderGraphContext ctx) =>
                {
                    Blitter.BlitCameraTexture(ctx.cmd, data.source, data.source, data.material, 0);
                });

            passData.material = material;
            passData.source = resourceData.activeColorTexture;
        }
    }
}
