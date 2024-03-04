// LcLShaderGUI
Shader "lcl/ShaderGUI/LcLShaderGUI"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilFail ("Stencil Fail", Float) = 0
        [KeywordEnum(None, Add, Multiply)] _Overlay ("Overlay mode", Float) = 0
        [PassEnum(UniversalForward, LitTest1)]_PassEnum ("Type", float) = 0

        [SingleLine] _BaseMap ("Main Texture", 2D) = "white" { }

        [Foldout(_SWITCH)] _SWITCH ("文件夹1", int) = 0
        [LightDir]_LightDir ("Light Dir", Vector) = (0, 0, -1, 0)
        [PowerSlider(2)]_WaveAmp3 ("Wave Amp3", Range(0, 1)) = 1.0
        [HDR]_Color ("Color", Color) = (1, 1, 1, 1)
        [SingleLine]_RampTex ("Ramp", 2D) = "white" { }
        [VectorRange(0, 1)]_WorldSize ("World Size", vector) = (1, 1, 1, 1)

        [Foldout(_SWITCH2)] _SWITCH2 ("文件夹2", int) = 0
        [HDR]_Color2 ("Color2", Color) = (1, 1, 1, 1)
        _Property1 ("Property1", Float) = 0
        _Property2 ("Property2", Float) = 0
        [Foldout(_SWITCH3)] _SWITCH3 ("文件夹3", int) = 0
        _Property5 ("Property5", Float) = 0
        [FoldoutEnd]_Property3 ("Property3", Float) = 0
        [FoldoutEnd]_Property5 ("Property5", float) = 1.2
        [FoldoutEnd]_WaveAmp ("Wave Amp", float) = 1.0

        [Foldout(_SWITCH4)] _SWITCH4 ("文件夹4", int) = 0
        [SingleLine] _WindTex ("Wind Texture", 2D) = "white" { }
        [Toggle(_TEST)] _TEST ("Toggle", float) = 0
        [ShowIf(_TEST)]_HeightFactor ("Height Factor", float) = 1.0
        [PowerSlider(2)]_WaveAmp2 ("Wave Amp2", Range(0, 1)) = 1.0
        _HeightCutoff ("Height Cutoff", float) = 1.2
        [FoldoutEnd] _WindSpeed ("Wind Speed", vector) = (1, 1, 1, 1)

        _HeightCutoff ("Height Cutoff", float) = 1.2
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            #pragma multi_compile _ _TEST

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _BaseMap;
            float4 _BaseMap_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_BaseMap, i.uv);
                return col;
            }
            ENDCG
        }

        Pass
        {
            Tags { "LightMode" = "LitTest1" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma multi_compile _ _TEST
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * float4(1, 0, 0, 1);
                return col;
            }
            ENDCG
        }
    }
    CustomEditor "LcLShaderEditor.LcLShaderGUI"
}
