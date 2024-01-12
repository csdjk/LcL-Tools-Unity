using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace LcLTools
{
    // 高消耗Shader, 用于测试性能,
    // 使GPU占用率达到100%, 用于测试其他Shader的性能
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

    public class HeavyPostProcessingFeature
    {
        static HeavyPostProcessingPass m_ScriptablePass;
        static Shader shader;
        public static void Enable(int count = 200)
        {
            Disable();
            
            RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
            shader = shader ?? Shader.Find("Hidden/HeavyPostProcessing");
            if (shader == null) return;

            var material = CoreUtils.CreateEngineMaterial(shader);
            material.SetInt("_Count", count);
            m_ScriptablePass = new HeavyPostProcessingPass
            {
                renderPassEvent = RenderPassEvent.AfterRenderingOpaques,
                material = material
            };
        }
        public static void Disable()
        {
            RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;

            m_ScriptablePass = null;
        }
        static void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            CameraType cameraType = camera.cameraType;
            if (cameraType == CameraType.Preview) return;

            ScriptableRenderer renderer = camera.GetUniversalAdditionalCameraData().scriptableRenderer;
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }
}


