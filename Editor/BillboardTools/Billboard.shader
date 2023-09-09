Shader "LcL/BillboardBaseLocalSpace"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
        _AlphaClip ("AlphaClip", Range(0, 1)) = 0.1
        [Toggle(_MULTIPLE)] _Multiple ("Multiple Mesh", float) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma shader_feature _MULTIPLE
            #pragma enable_d3d11_debug_symbols
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            half _AlphaClip;

            struct Varings
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;

                #ifdef _MULTIPLE
                    float3 tangent : TANGENT;
                #endif
            };
            struct Output
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            //https://zhuanlan.zhihu.com/p/568974914
            Output vert(Varings v)
            {
                float3 centerOffset = float3(0, 0, 0);

                #ifdef _MULTIPLE
                    centerOffset = v.tangent;
                #endif

                v.positionOS.xyz -= centerOffset;

                float3 viewerLocal = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1)); //将摄像机的坐标转换到物体模型空间
                float3 localDir = viewerLocal - centerOffset; //计算新的“forward”

                localDir.y = 0; //这里有两种方式，一种是仰面也要对齐，涉及XYZ面，另外一种就是只考虑XY面，即把y值置0。
                localDir = normalize(localDir); //归一化。

                float3 upLocal = float3(0, 1, 0); //默认模型空间的up轴全部为（0,1,0）
                float3 rightLocal = normalize(cross(localDir, upLocal)); //计算新的right轴
                upLocal = cross(rightLocal, localDir); //计算新的up轴。

                float3 BBLocalPos = rightLocal * v.positionOS.x + upLocal * v.positionOS.y; //将原本的xy坐标以在新轴上计算，相当于一个线性变换【原模型空间】->【新模型空间】

                #ifdef _MULTIPLE
                    BBLocalPos += centerOffset;
                #endif

                Output o;
                o.positionCS = TransformObjectToHClip(BBLocalPos); //MVP变换
                o.uv = v.uv;
                return o;
            }
            half4 frag(Output i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                clip(col.a - _AlphaClip); //透明度裁切

                return col;
            }
            ENDHLSL
        }
    }
}