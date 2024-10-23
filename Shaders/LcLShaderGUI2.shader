// LcLShaderGUI
Shader "LcL/ShaderGUI/LcLShaderGUI2"
{
    Properties
    {
        [Toggle(_SWITCH)] _SWITCH ("Toggle", int) = 0
        [ShowIf(_SWITCH, 1)]_Color("Color", Color) = (1, 1, 1, 1)
        [ShowIf(_SWITCH, 0)]_Tex("Texture2", 2D) = "white" { }

        _Test ("Test", float) = 0
        [Max(0.1)]_Test2 ("Test", float) = 0
        [Min(0.1,0,2,3)]_TestV ("TestV", Vector) = (0, 0, 0, 0)
        [VectorRange(0.1,3)]_TestV3 ("TestV", Vector) = (0, 0, 0, 0)
        [Max(0.1,3)]_TestV4 ("Test4", Vector) = (0, 0, 0, 0)

        [Enum(UnityEngine.Rendering.CullMode)]_CullMode ("CullMode", float) = 2
        [Foldout(_BACK_FACE)]_BACK_FACE ("Back Face", float) = 0
        [FoldoutEnd][Toggle]_UseBackFaceUV2 ("Use Back Face UV2", Float) = 0

        [Foldout]_Textures ("Textures", float) = 0
        [SingleLine]_MainTex ("MainTex", 2D) = "white" { }
        [SingleLine]_LightMapTex ("LightMapTex", 2D) = "white" { }
        [SingleLine]_MTMap ("MTMap(r:MT,g:Thickness)", 2D) = "white" { }
        [SingleLine]_MTSpecularRamp ("MTSpecularRamp", 2D) = "gray" { }
        [SingleLine]_PackedShadowRampTex ("PackedShadowRampTex", 2D) = "white" { }
        _MainTexAlphaCutoff ("Main Tex Alpha Cutoff", Range(0, 1)) = 0.5
        [FoldoutEnd]_MainTexAlphaUse ("Main Tex Alpha Use(0,1,2,3)", Float) = 2

        [Foldout]_Common ("Common", float) = 0
        [Toggle]_UseCoolShadowColorOrTex ("Use Cool Shadow Color Or Tex", Float) = 1
        [Toggle]_UseLightMapColorAO ("Use Light Map Color AO", Float) = 1
        [Toggle]_UseShadowTransition ("Use Shadow Transition", Float) = 0
        [Toggle]_UseVertexColorAO ("Use Vertex Color AO", Float) = 1
        [Toggle]_UseVertexRampWidth ("Use Vertex Ramp Width", Float) = 1
        _UseSaturation ("Use Saturation", Float) = 0
        _LightArea ("Light Area", Range(0, 1)) = 0.55
        _LightIntensity ("Light Intensity", Range(0, 1)) = 0.75
        _SpecularColor ("Specular Color", Color) = (1, 1, 1)
        _EmissionColor_MHY ("Emission Color MHY", Color) = (1, 1, 1)
        _EmissionScaler ("Emission Scaler", Range(0, 3)) = 1
        _EmissionStrengthLerp ("Emission Strength Lerp", Range(0, 1)) = 0
        _ShadowRampWidth ("Shadow Ramp Width", Range(0, 1)) = 1
        _FaceBlushColor ("Face Blush Color", Color) = (1, 0.60383, 0.44799)
        [FoldoutEnd]_FaceBlushStrength ("Face Blush Strength", Range(0, 1)) = 0

        [Foldout]_Material1 ("Material1", float) = 0
        _Color ("Color", Color) = (1, 1, 1, 1)
        _SpecMulti ("Spec Multi", Range(0, 1)) = 0.3
        _Shininess ("Shininess", Float) = 10
        [FoldoutEnd]_EmissionScaler1 ("Emission Scaler 1", Range(0, 3)) = 1

        [Foldout]_Material2 ("Material2", float) = 0
        [Toggle]_UseMaterial2 ("Use Material 2", Float) = 1
        _Color2 ("Color2", Color) = (1, 1, 1)
        _SpecMulti2 ("Spec Multi 2", Range(0, 1)) = 0.2
        _Shininess2 ("Shininess 2", Float) = 10
        [FoldoutEnd]_EmissionScaler2 ("Emission Scaler 2", Range(0, 3)) = 1

        [Foldout]_Material3 ("Material3", float) = 0
        [Toggle]_UseMaterial3 ("Use Material 3", Float) = 1
        _Color3 ("Color3", Color) = (1, 1, 1)
        _Shininess3 ("Shininess 3", Float) = 10
        _SpecMulti3 ("Spec Multi 3", Range(0, 1)) = 0.1
        [FoldoutEnd]_EmissionScaler3 ("Emission Scaler 3", Range(0, 3)) = 1

        [Foldout]_Material4 ("Material4", float) = 0
        [Toggle]_UseMaterial4 ("Use Material 4", Float) = 1
        _Color4 ("Color4", Color) = (1, 1, 1)
        _Shininess4 ("Shininess 4", Float) = 10
        _SpecMulti4 ("Spec Multi 4", Range(0, 1)) = 0.1
        [FoldoutEnd]_EmissionScaler4 ("Emission Scaler 4", Range(0, 3)) = 1

        [Foldout]_Material5 ("Material5", float) = 0
        [Toggle]_UseMaterial5 ("Use Material 5", Float) = 1
        _Color5 ("Color5", Color) = (1, 1, 1)
        _Shininess5 ("Shininess 5", Float) = 10
        _SpecMulti5 ("Spec Multi 5", Range(0, 1)) = 0.1
        [FoldoutEnd]_EmissionScaler5 ("Emission Scaler 5", Range(0, 3)) = 1

        [Foldout]_MTParams ("Metal Params", float) = 0
        _MTMapBrightness ("MT Map Brightness", Float) = 4
        _MTMapDarkColor ("MT Map Dark Color", Color) = (0.30499, 0.07036, 0.17755)
        _MTMapLightColor ("MT Map Light Color", Color) = (1, 0.99309, 0.90256)
        _MTMapTileScale ("MT Map Tile Scale", Float) = 1
        _MTShadowMultiColor ("MT Shadow Multi Color", Color) = (0.56952, 0.42276, 0.55207)
        _MTSharpLayerColor ("MT Sharp Layer Color", Color) = (1, 1, 1)
        _MTSharpLayerOffset ("MT Sharp Layer Offset", Float) = 1
        _MTShininess ("MT Shininess", Float) = 15
        _MTSpecularAttenInShadow ("MT Specular Atten In Shadow", Range(0, 1)) = 0.2
        _MTSpecularColor ("MT Specular Color", Color) = (1, 0.76079, 0.51114)
        _MTSpecularScale ("MT Specular Scale", Float) = 60
        [FoldoutEnd]_MTUseSpecularRamp ("MT Use Specular Ramp", Float) = 0


        [Foldout()]_HitEffect ("Hit Effect", float) = 0
        _HitColor ("Hit Color", Color) = (0, 0, 0, 0)
        _ElementRimColor ("Element Rim Color", Color) = (0, 0, 0, 0)
        _HitColorFresnelPower ("Hit Color Fresnel Power", Float) = 1.5
        [FoldoutEnd]_HitColorScaler ("Hit Color Scaler", Float) = 6

        [Foldout(_TRANSLUCENCY)]_TRANSLUCENCY ("Translucency", float) = 0
        _TransColor ("Trans Color", Color) = (1, 1, 1, 1)
        _TransIntensity ("Trans Intensity", Range(0, 5)) = 1
        _TransScattering ("Trans Scattering", Range(0.1, 5)) = 1
        [FoldoutEnd]_TransDistortion ("Trans Distortion", Range(0, 1)) = 0.3

        [Foldout]_TOON_OUT_LINE ("Outline", float) = 1
        _OutlineType ("Outline Type", Float) = 2
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 0)
        _OutlineColor2 ("Outline Color2", Color) = (0.31342, 0.08105, 0.08105)
        _OutlineColor3 ("Outline Color3", Color) = (0.27258, 0.08602, 0.08602)
        _OutlineColor4 ("Outline Color4", Color) = (0.4253, 0.13803, 0.13803)
        _OutlineColor5 ("Outline Color5", Color) = (0.03749, 0.05204, 0.11131)
        _OutlineWidth ("Outline Width", Range(0, 5)) = 2
        _OutlineCorrectionWidth ("Outline Correction Width", Range(0, 5)) = 1.3
        _OutlineWidthAdjustScales ("Outline Width Adjust Scales", Vector) = (0.105, 0.245, 0.6)
        _OutlineWidthAdjustZs ("Outline Width Adjust Zs", Vector) = (0.01, 2, 6)
        _MaxOutlineZOffset ("Max Outline Z Offset", Range(0, 10)) = 1
        _Scale ("Scale", Float) = 0.01
        [FoldoutEnd]_OffsetFactor ("Offset Factor", Range(-10, 10)) = 0

    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }
        LOD 100

        Pass
        {
            Tags
            {
                "LightMode" = "UniversalForward"
            }

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
            Tags
            {
                "LightMode" = "LitTest1"
            }

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