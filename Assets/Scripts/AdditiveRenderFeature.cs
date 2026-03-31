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
        class Variant
        {
            public Material material;
            public FilteringSettings filtering;
        }

        private List<Variant> variants = new List<Variant>();

        public TextureHandle intermediateTexture;

        public AdditiveRenderPass(Settings settings)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents;

            variants.Add(new Variant {
                material = settings.blueMaterial,
                filtering = new FilteringSettings(RenderQueueRange.all, settings.blueLayer)
            });

            variants.Add(new Variant {
                material = settings.brownMaterial,
                filtering = new FilteringSettings(RenderQueueRange.all, settings.brownLayer)
            });

            variants.Add(new Variant {
                material = settings.greenMaterial,
                filtering = new FilteringSettings(RenderQueueRange.all, settings.greenLayer)
            });

            variants.Add(new Variant {
                material = settings.redMaterial,
                filtering = new FilteringSettings(RenderQueueRange.all, settings.redLayer)
            });

            variants.Add(new Variant {
                material = settings.purpleMaterial,
                filtering = new FilteringSettings(RenderQueueRange.all, settings.purpleLayer)
            });

            // ensure all sorting layers
            for (int i = 0; i < variants.Count; i++)
            {
                variants[i].filtering.sortingLayerRange = SortingLayerRange.all;
            }
        }

        class PassData
        {
            public List<RendererListHandle> rendererLists;
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

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Additive Pass", out var passData))
{
                passData.rendererLists = new List<RendererListHandle>();

                foreach (var variant in variants)
                {
                    var drawSettings = CreateDrawingSettings(
                        new ShaderTagId("SRPDefaultUnlit"),
                        renderingData,
                        cameraData,
                        lightData,
                        SortingCriteria.CommonTransparent
                    );

                    drawSettings.overrideMaterial = variant.material;

                    var rendererListParams = new RendererListParams(
                        renderingData.cullResults,
                        drawSettings,
                        variant.filtering
                    );

                    var rl = renderGraph.CreateRendererList(rendererListParams);
                    passData.rendererLists.Add(rl);
                    builder.UseRendererList(rl);
                }

                builder.SetRenderAttachment(intermediateTexture, 0);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    foreach (var rl in data.rendererLists)
                    {
                        ctx.cmd.DrawRendererList(rl);
                    }
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
        public Material blueMaterial;
        public Material brownMaterial;
        public Material greenMaterial;
        public Material redMaterial;
        public Material purpleMaterial;

        public Material compositeMaterial;

        public LayerMask blueLayer;
        public LayerMask brownLayer;
        public LayerMask greenLayer;
        public LayerMask redLayer;
        public LayerMask purpleLayer;
    }

    public Settings settings = new Settings();

    AdditiveRenderPass additivePass;
    CompositePass compositePass;

    public override void Create()
    {
        additivePass = new AdditiveRenderPass(settings);
        compositePass = new CompositePass(settings.compositeMaterial, additivePass);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.compositeMaterial == null) return;

        additivePass = new AdditiveRenderPass(settings);
        compositePass = new CompositePass(settings.compositeMaterial, additivePass);

        renderer.EnqueuePass(additivePass);
        renderer.EnqueuePass(compositePass);
    }
}