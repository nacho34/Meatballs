using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using System.Collections.Generic;

public class AdditiveRenderFeature : ScriptableRendererFeature
{
    // --- Pass 1: renders objects into an intermediate texture ---
    class AdditiveRenderPass : ScriptableRenderPass
    {
        private Material overrideMaterial;
        private FilteringSettings filteringSettings;

        // The handle is written here so Pass 2 can read it
        public TextureHandle intermediateTexture;

        public AdditiveRenderPass(Material material, LayerMask layerMask)
        {
            overrideMaterial = material;
            filteringSettings = new FilteringSettings(RenderQueueRange.all, layerMask);
            filteringSettings.sortingLayerRange = SortingLayerRange.all;
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        class PassData
        {
            public RendererListHandle rendererList;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();
            var renderingData = frameData.Get<UniversalRenderingData>();
            var cameraData = frameData.Get<UniversalCameraData>();
            var lightData = frameData.Get<UniversalLightData>();

            // Create the intermediate texture, same size as the camera
            var desc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
            desc.name = "AdditiveIntermediate";
            desc.clearBuffer = true;
            desc.clearColor = Color.clear;
            intermediateTexture = renderGraph.CreateTexture(desc);

            var drawSettings = CreateDrawingSettings(
                new ShaderTagId("SRPDefaultUnlit"),
                renderingData,
                cameraData,
                lightData,
                SortingCriteria.CommonTransparent
            );
            drawSettings.overrideMaterial = overrideMaterial;

            var rendererListParams = new RendererListParams(
                renderingData.cullResults,
                drawSettings,
                filteringSettings
            );

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Additive Pass", out var passData))
            {
                passData.rendererList = renderGraph.CreateRendererList(rendererListParams);
                builder.UseRendererList(passData.rendererList);

                // Write to intermediate, NOT the camera color target
                builder.SetRenderAttachment(intermediateTexture, 0);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    ctx.cmd.DrawRendererList(data.rendererList);
                });
            }
        }
    }

    // --- Pass 2: reads the intermediate texture, composites to screen ---
    class CompositePass : ScriptableRenderPass
    {
        private Material compositeMaterial;
        private AdditiveRenderPass sourcePass;

        public CompositePass(Material material, AdditiveRenderPass source)
        {
            compositeMaterial = material;
            sourcePass = source;
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents + 1;
        }

        class PassData
        {
            public TextureHandle sourceTexture;
            public Material material;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var resourceData = frameData.Get<UniversalResourceData>();

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Composite Pass", out var passData))
            {
                // Read the texture written by Pass 1
                passData.sourceTexture = sourcePass.intermediateTexture;
                passData.material = compositeMaterial;

                // Declare we're reading it
                builder.UseTexture(passData.sourceTexture);

                // Write to the actual camera output
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                builder.AllowGlobalStateModification(true); 

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    Blitter.BlitTexture(
                        ctx.cmd,
                        data.sourceTexture,
                        new Vector4(1,1,0,0),
                        data.material,
                        0
                    );
                });
            }
        }
    }

    [System.Serializable]
    public class Settings
    {
        public Material additiveMaterial;
        public Material compositeMaterial;
        public LayerMask layerMask;
    }

    public Settings settings = new Settings();

    AdditiveRenderPass additivePass;
    CompositePass compositePass;

    public override void Create()
    {
        additivePass = new AdditiveRenderPass(settings.additiveMaterial, settings.layerMask);
        compositePass = new CompositePass(settings.compositeMaterial, additivePass);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.additiveMaterial == null || settings.compositeMaterial == null) return;

        additivePass = new AdditiveRenderPass(settings.additiveMaterial, settings.layerMask);
        compositePass = new CompositePass(settings.compositeMaterial, additivePass);

        renderer.EnqueuePass(additivePass);
        renderer.EnqueuePass(compositePass);
    }
}