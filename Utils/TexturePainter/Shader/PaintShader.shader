Shader "Hidden/TexturePainter/PaintShader"
{

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Tags { "RenderType" = "Opaque" }
        HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
        TEXTURE2D(_SourceTex);SAMPLER(sampler_SourceTex);
        float3 _Mouse;
        float4x4 mesh_Object2World;

        float4 _BrushColor;
        float _BrushStrength;
        float _BrushHardness;
        float _BrushSize;
        float _IsDraw;

        int _DrawModel;
        int _DrawChannel;
        int _TexCoordChannel;
        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
            float2 uv2 : TEXCOORD1;
            float2 uv3 : TEXCOORD2;
            float2 uv4 : TEXCOORD3;
        };
        struct v2f
        {
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
            float3 worldPos : TEXCOORD1;
        };

        float2 GetUV(appdata v)
        {
            float2 uv;
            if (_TexCoordChannel == 0)
            {
                uv = v.uv;
            }
            else if (_TexCoordChannel == 1)
            {
                uv = v.uv2;
            }
            else if (_TexCoordChannel == 2)
            {
                uv = v.uv3;
            }
            else if (_TexCoordChannel == 3)
            {
                uv = v.uv4;
            }
            return uv;
        }

        v2f vert(appdata v)
        {
            v2f o;
            float2 uv = GetUV(v);

            float2 uvRemapped = uv;
            uvRemapped.y = 1. - uvRemapped.y;
            uvRemapped = uvRemapped * 2. - 1.;
            o.vertex = float4(uvRemapped.xy, 0., 1.);

            o.worldPos = mul(mesh_Object2World, v.vertex);
            o.uv = uv;
            return o;
        }

        v2f vertDefault(appdata v)
        {
            v2f o;
            o.vertex = TransformObjectToHClip(v.vertex);
            o.uv = GetUV(v);
            return o;
        }
        ENDHLSL
        // pass 0  Add
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float3 worldPos = i.worldPos;

                float3 mouse_pos = _Mouse.xyz;

                float4 prevColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                float size = _BrushSize;


                float dist;
                if (_DrawModel == 0)
                {
                    dist = length(worldPos - mouse_pos);
                }
                else
                {
                    dist = length(uv - mouse_pos);
                }

                float soft = _BrushHardness;
                float strength = _BrushStrength;
                float4 brushCol = smoothstep(size, size - soft, dist) * strength;

                if (_DrawChannel != 0 && _DrawChannel != 1 && _DrawChannel != 2 && _DrawChannel != 3)
                {
                    brushCol *= _BrushColor;
                }

                if (_DrawChannel == 0)
                {
                    prevColor = prevColor.rrrr;
                }
                else if (_DrawChannel == 1)
                {
                    prevColor = prevColor.gggg;
                }
                else if (_DrawChannel == 2)
                {
                    prevColor = prevColor.bbbb;
                }
                else if (_DrawChannel == 3)
                {
                    prevColor = prevColor.aaaa;
                }


                // 混合模式：滤色(Screen)，去黑留白
                // 公式：1-(1-a)(1-b)
                float4 col = 1 - (1 - brushCol) * (1 - prevColor);

                col = lerp(prevColor, col, _IsDraw);
                return half4(col.rgb, 1);
            }
            ENDHLSL
        }

        // pass 1 Sub
        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float3 worldPos = i.worldPos;
                float3 mouse_pos = _Mouse.xyz;

                float4 prevColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                float size = _BrushSize;

                float dist;
                if (_DrawModel == 0)
                {
                    dist = length(worldPos - mouse_pos);
                }
                else
                {
                    dist = length(uv - mouse_pos);
                }

                float soft = _BrushHardness;
                float strength = _BrushStrength;
                float4 col;
                col = prevColor - smoothstep(size, size - soft, dist) * strength;



                col = lerp(prevColor, col, _IsDraw);
                return col;
            }
            ENDHLSL
        }
        // pass 2 Blend
        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            float4 blendColor(float4 color1, float4 color2)
            {
                // 混合模式：滤色(Screen)，去黑留白
                // 公式：1-(1-a)(1-b)
                float4 col = 1 - (1 - color1) * (1 - color2);
                return col;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float4 maskCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                float4 sourceCol = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv);

                half4 col;
                if (_DrawChannel == 0)
                {
                    col.r = maskCol.r;
                    col.gba = sourceCol.gba;
                }
                else if (_DrawChannel == 1)
                {
                    col.g = maskCol.g;
                    col.rba = sourceCol.rba;
                }
                else if (_DrawChannel == 2)
                {
                    col.b = maskCol.b;
                    col.rga = sourceCol.rga;
                }
                else if (_DrawChannel == 3)
                {
                    col.a = maskCol.a;
                    col.rgb = sourceCol.rgb;
                }
                else
                {
                    col = blendColor(maskCol, sourceCol);
                }
                return col;
            }
            ENDHLSL
        }

        // pass 3 Fixed Edge
        // 修复边缘裂缝 - 扩张边缘
        Pass
        {

            HLSLPROGRAM

            #pragma  vertex  vertDefault
            #pragma  fragment frag
            float4 _MainTex_TexelSize;
            TEXTURE2D(_IlsandMap);
            SAMPLER(sampler_IlsandMap);


            half4 frag(v2f i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float map = SAMPLE_TEXTURE2D(_IlsandMap, sampler_IlsandMap, i.uv);

                float4 average = col;

                if (map.x < 0.2)
                {
                    int n = 0;
                    average = float4(0.0, 0.0, 0.0, 0.0);

                    UNITY_UNROLL for (float x = -1.5; x <= 1.5; x++)
                    {
                        UNITY_UNROLL for (float y = -1.5; y <= 1.5; y++)
                        {

                            float4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + _MainTex_TexelSize.xy * float2(x, y) * 2);
                            float m = SAMPLE_TEXTURE2D(_IlsandMap, sampler_IlsandMap, i.uv + _MainTex_TexelSize.xy * float2(x, y) * 2);

                            n += step(0.1, m);
                            average += c * step(0.1, m);
                        }
                    }

                    average /= n;
                }

                col = average;

                return col;
            }
            ENDHLSL
        }
    }
}
