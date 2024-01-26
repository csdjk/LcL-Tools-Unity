// 高消耗Shader, 用于测试性能,
// 使GPU占用率达到100%, 用于测试其他Shader的性能
Shader "Hidden/HeavyPostProcessing"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            struct Attributes
            {
                float4 vertex : POSITION;
                uint vertexID : SV_VertexID;
                float2 uv : TEXCOORD0;
            };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 viewRay : TEXCOORD1;
            };
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.vertex);
                output.uv = input.uv;
                return output;
            }
            float3 hsv2rgb(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
            }
            
            int _Count;
            half4 frag(Varyings input) : SV_Target
            {
                float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                float hue = 0.0;
                for (int j = 0; j < _Count; j++)
                {
                    hue += sin(color.r) * cos(color.g) + tan(color.b);
                    hue = frac(hue); // Keep hue in [0, 1] range

                }
                // Convert hue to RGB color
                float3 rgb = hsv2rgb(float3(hue, 1.0, 1.0));
                return float4(lerp(color, rgb, 0.05), color.a); // Keep original alpha

            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}