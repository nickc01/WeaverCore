Shader "Sprites/Sprite - Faded Edges"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		[PerRendererData] _Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[PerRendererData]_SpriteCoords("Sprite Coords", Vector) = (0,0,1,1)
		[PerRendererData]_Sharpness("Sharpness", int) = 1
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Cull Off
			Lighting Off
			ZWrite Off
			Blend One OneMinusSrcAlpha

			Pass
			{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile _ PIXELSNAP_ON
				#include "UnityCG.cginc"

				struct appdata_t
				{
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord  : TEXCOORD0;
				};

				fixed4 _Color;
				float4 _SpriteCoords;

				inline float2 UnclampedInverseLerp(float2 a, float2 b, float2 v)
				{
					return (v - a) / (b - a);
				}

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex);
					OUT.texcoord = IN.texcoord;
					OUT.color = IN.color * _Color;
					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
					#endif

					return OUT;
				}

				sampler2D _MainTex;
				sampler2D _AlphaTex;
				float _AlphaSplitEnabled;
				int _Sharpness;


				fixed4 SampleSpriteTexture(float2 uv)
				{
					fixed4 color = tex2D(_MainTex, uv);

	#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
					if (_AlphaSplitEnabled)
						color.a = tex2D(_AlphaTex, uv).r;
	#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

					return color;
				}

				float2 Double(float2 value) {
					//return value * value;
					float2 result = value;
					for (int i = 0; i < _Sharpness; i++) 
					{
						result *= value;
					}	

					return result;
					//return pow(value, 2 * _Sharpness);
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;

					float2 actualUV = UnclampedInverseLerp(_SpriteCoords.xy, _SpriteCoords.zw, IN.texcoord);

					float2 alphaMult = 1.0 - Double(Double((2.0 * actualUV) - 1));

					c.a = c.a * alphaMult.x * alphaMult.y;

					c.rgb *= c.a;
					return c;
				}
			ENDCG
			}
		}
}