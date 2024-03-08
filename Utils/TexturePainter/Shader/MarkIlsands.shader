Shader "Hidden/TexturePainter/MarkIlsands"
{
	SubShader
	{
		// =====================================================================================================================
		Tags { "RenderType" = "Opaque" }
		LOD	   100
		ZTest  Off
		ZWrite Off
		Cull   Off

		Pass
		{
			CGPROGRAM

			#pragma vertex   vert
			#pragma fragment frag

			#include "UnityCG.cginc"

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
				float4 vertex : SV_POSITION;
			};
			int _TexCoordChannel;
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

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return float4(1., 0., 0., 1.);
			}
			ENDCG
		}
	}
}
