using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AdditiveRenderFeature : ScriptableRendererFeature
{
    class AdditiveRenderPass : ScriptableRenderPass
    {
        private Material overrideMaterial;
        private FilteringSettings filteringSettings;
        private ShaderTagId shaderTagId = new ShaderTagId("UniversalForward");

        public AdditiveRenderPass(Material material, LayerMask layerMask)
        {
            overrideMaterial = material;
            filteringSettings = new FilteringSettings(RenderQueueRange.all, layerMask);

            renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        }
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureTarget(renderingData.cameraData.renderer.cameraColorTargetHandle);
            ConfigureClear(ClearFlag.None, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Additive Pass");

            using (new ProfilingScope(cmd, new ProfilingSampler("Additive Pass")))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                var drawingSettings = CreateDrawingSettings(shaderTagId, ref renderingData, SortingCriteria.None);

                // Force additive material
                drawingSettings.overrideMaterial = overrideMaterial;

                context.DrawRenderers(
                    renderingData.cullResults,
                    ref drawingSettings,
                    ref filteringSettings
                );
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    [System.Serializable]
    public class Settings
    {
        public Material additiveMaterial;
        public LayerMask layerMask;
    }

    public Settings settings = new Settings();

    AdditiveRenderPass pass;

    public override void Create()
    {
        pass = new AdditiveRenderPass(settings.additiveMaterial, settings.layerMask);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.additiveMaterial != null)
        {
            renderer.EnqueuePass(pass);
        }
    }
}