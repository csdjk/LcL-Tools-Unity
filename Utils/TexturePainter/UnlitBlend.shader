Shader "Custom/UnlitShaderExample"
{
	Properties
	{
		_BaseMap ("Texture", 2D) = "white" { }
		_BaseColor ("Colour", Color) = (1, 1, 1, 1)
		_BaseMap2 ("Texture2", 2D) = "white" { }
		_BaseMap3 ("Texture3", 2D) = "white" { }
		_BaseMap4 ("Texture4", 2D) = "white" { }
		_Mask ("Mask", 2D) = "white" { }
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

		HLSLINCLUDE
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

		CBUFFER_START(UnityPerMaterial)
			float4 _BaseMap_ST;
			float4 _BaseColor;
		CBUFFER_END
		ENDHLSL

		Pass
		{
			Tags { "LightMode" = "UniversalForward" }

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct Attributes
			{
				float4 positionOS : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			TEXTURE2D(_BaseMap);
			SAMPLER(sampler_BaseMap);

			TEXTURE2D(_BaseMap2);
			SAMPLER(sampler_BaseMap2);

			TEXTURE2D(_BaseMap3);
			SAMPLER(sampler_BaseMap3);

			TEXTURE2D(_BaseMap4);
			SAMPLER(sampler_BaseMap4);

			TEXTURE2D(_Mask);
			SAMPLER(sampler_Mask);
			Varyings vert(Attributes input)
			{
				Varyings output;

				VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
				output.positionCS = positionInputs.positionCS;
				output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
				output.color = input.color;
				return output;
			}

			half4 frag(Varyings input) : SV_Target
			{
				half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
				half4 baseMap2 = SAMPLE_TEXTURE2D(_BaseMap2, sampler_BaseMap2, input.uv);
				half4 baseMap3 = SAMPLE_TEXTURE2D(_BaseMap3, sampler_BaseMap3, input.uv);
				half4 baseMap1 = SAMPLE_TEXTURE2D(_BaseMap4, sampler_BaseMap4, input.uv);

				half4 mask = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, input.uv);

				half4 finalColor = baseMap * mask.r + baseMap2 * mask.g + baseMap3 * mask.b + baseMap1 * mask.a;

				return finalColor * _BaseColor * input.color;
			}
			ENDHLSL
		}
	}
}
