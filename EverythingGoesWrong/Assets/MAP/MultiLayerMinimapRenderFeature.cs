using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

[System.Serializable]
public class LayerColorMapping
{
    [Tooltip("Name of the layer as defined in Project Settings > Tags & Layers")]
    public string layerName;
    [Tooltip("Color to use for objects in this layer on the minimap")]
    public Color color = Color.white;
}



public class MultiLayerMinimapRenderFeature : ScriptableRendererFeature
{
    [SerializeField] private Shader minimapShader;
    [SerializeField] private LayerColorMapping[] layerMappings;

    // A render pass that draws objects for a specific layer with an override material.
    class LayerRenderPass : ScriptableRenderPass
    {
        private ShaderTagId shaderTag = new ShaderTagId("UniversalForward");
        private Material overrideMaterial;
        private FilteringSettings filteringSettings;

        public LayerRenderPass(Material material, int layerMask)
        {
            overrideMaterial = material;
            filteringSettings = new FilteringSettings(RenderQueueRange.opaque, layerMask);
            renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // Only execute for the camera named "MinimapCamera"
            if (renderingData.cameraData.camera.name != "MinimapCamera")
                return;

            CommandBuffer cmd = CommandBufferPool.Get("MinimapLayerPass");

            DrawingSettings drawingSettings = new DrawingSettings(shaderTag, new SortingSettings(renderingData.cameraData.camera))
            {
                overrideMaterial = overrideMaterial,
                overrideMaterialPassIndex = 0
            };

            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    List<LayerRenderPass> passes;

    public override void Create()
    {
        passes = new List<LayerRenderPass>();

        if (minimapShader == null || layerMappings == null || layerMappings.Length == 0)
        {
            Debug.LogWarning("Minimap shader or layer mappings not set.");
            return;
        }

        // For each mapping, create an override material and a render pass for its layer.
        foreach (var mapping in layerMappings)
        {
            int layerMask = LayerMask.GetMask(mapping.layerName);
            if (layerMask == 0)
            {
                Debug.LogWarning($"Layer '{mapping.layerName}' not found in project settings. Skipping mapping.");
                continue;
            }
            Material mat = new Material(minimapShader);
            mat.SetColor("_BaseColor", mapping.color);
            passes.Add(new LayerRenderPass(mat, layerMask));
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Enqueue all passes for the minimap camera only.
        if (renderingData.cameraData.camera.name == "MinimapCamera")
        {
            foreach (var pass in passes)
            {
                renderer.EnqueuePass(pass);
            }
        }
    }
}