using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
// 高消耗Shader, 用于测试性能,
// 使GPU占用率达到100%, 用于测试其他Shader的性能
public class HeavyPostProcessingFeature : ScriptableRendererFeature
{
    public Shader shader;
    class HeavyPostProcessingPass : ScriptableRenderPass
    {
        public Material material;
        public HeavyPostProcessingPass()
        {
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("HeavyPostProcessing");
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            Blit(cmd, ref renderingData, material);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    HeavyPostProcessingPass m_ScriptablePass;

    public override void Create()
    {
        shader = shader ?? Shader.Find("Hidden/HeavyPostProcessing");
        if (shader == null) return;
        var material = CoreUtils.CreateEngineMaterial(shader);
        m_ScriptablePass = new HeavyPostProcessingPass
        {
            renderPassEvent = RenderPassEvent.AfterRenderingOpaques,
            material = material
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


