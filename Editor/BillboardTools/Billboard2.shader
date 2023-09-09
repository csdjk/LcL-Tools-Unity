Shader "LcL/BillboardBaseWorldSpace"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
        _VerticalBillboarding("Vertical Restraints", Range(0,1)) = 0
        [Toggle(_MULTIPLE)] _Multiple ("Multiple Mesh", float) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Cull Off
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma shader_feature _MULTIPLE

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
            half _VerticalBillboarding;
            // 获取模型的中心点
            inline float3 GetModelCenterWorldPos()
            {
                return float3(UNITY_MATRIX_M[0].w, UNITY_MATRIX_M[1].w, UNITY_MATRIX_M[2].w);
            }
            // 获取模型的缩放值
            inline float3 GetModelScale()
            {
                return float3(
                    length(float3(unity_ObjectToWorld[0].x, unity_ObjectToWorld[1].x, unity_ObjectToWorld[2].x)), // scale x axis
                    length(float3(unity_ObjectToWorld[0].y, unity_ObjectToWorld[1].y, unity_ObjectToWorld[2].y)), // scale y axis
                    length(float3(unity_ObjectToWorld[0].z, unity_ObjectToWorld[1].z, unity_ObjectToWorld[2].z))  // scale z axis
                );
            }

            float3 ApplyBillboard(float3 offset, float3 positionOS)
            {
                float3 center = GetModelCenterWorldPos();
                positionOS -= offset;

                float3 normalDir = center - _WorldSpaceCameraPos.xyz;
                normalDir.y = normalDir.y * _VerticalBillboarding;
                normalDir = normalize(normalDir);
                float3 upDir = float3(0, 1, 0) ;
                float3 rightDir = normalize(cross(upDir, normalDir));
                upDir = normalize(cross(normalDir, rightDir));

                half3 scale = GetModelScale();
                positionOS = positionOS * scale;

                // positionOS = mul(float3x3(
                //     rightDir.x, upDir.x, normalDir.x,
                //     rightDir.y, upDir.y, normalDir.y,
                //     rightDir.z, upDir.z, normalDir.z
                // ), positionOS);

                positionOS = rightDir * positionOS.x + upDir * positionOS.y;


                positionOS += offset;

                float3 positionWS = positionOS + center;
                return positionWS;
            }

            Output vert(Varings input)
            {
                #ifdef _MULTIPLE
                    float3 positionWS = ApplyBillboard(input.tangent, input.positionOS.xyz);
                #else
                    float3 positionWS = ApplyBillboard(0, input.positionOS.xyz);
                #endif

                Output output;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.uv = input.uv;
                return output;
            }
            half4 frag(Output i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                return col;
            }
            ENDHLSL
        }
    }
}