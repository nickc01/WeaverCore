Shader "Hollow Knight/Light Blur" 
{
	Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BlurInfo ("Blur Info", Vector) = (0.00052083336,0.0009259259,0,0)
	}
	
		SubShader
		{
			Pass
			{
				ZWrite Off
				Cull Off
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				struct appdata_t
				{
					float4 pos   : POSITION;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 position : SV_POSITION;
					float2 texcoord  : TEXCOORD0;
				};

				sampler2D _MainTex;
				float2 _BlurInfo;

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.position = UnityObjectToClipPos(IN.pos);
					OUT.texcoord = IN.texcoord;
					return OUT;
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					float4 u_xlat0;
					float3 u_xlat16_0;
					float4 u_xlat10_0;
					float4 u_xlat1;
					float3 u_xlat16_1;
					float4 u_xlat10_1;
					float4 u_xlat10_2;
					float4 output;

					u_xlat0 = _BlurInfo.xxxx * float4(3.23076892, 1.38461494, -2.76923108, -0.615384996) + IN.texcoord.xxxx;
					u_xlat1.xz = u_xlat0.yw;
					u_xlat1.yw = IN.texcoord.yy;
					u_xlat10_2 = tex2D(_MainTex, u_xlat1.xy);
					u_xlat10_1 = tex2D(_MainTex, u_xlat1.zw);
					u_xlat16_1.xyz = u_xlat10_1.xyz + u_xlat10_2.xyz;
					u_xlat16_1.xyz = u_xlat16_1.xyz * float3(0.316260993, 0.316260993, 0.316260993);
					u_xlat10_2 = tex2D(_MainTex, IN.texcoord.xy);
					u_xlat16_1.xyz = u_xlat10_2.xyz * float3(0.227026999, 0.227026999, 0.227026999) + u_xlat16_1.xyz;
					u_xlat0.yw = IN.texcoord.yy;
					u_xlat10_2 = tex2D(_MainTex, u_xlat0.xy);
					u_xlat10_0 = tex2D(_MainTex, u_xlat0.zw);
					u_xlat16_0.xyz = u_xlat10_0.xyz + u_xlat10_2.xyz;
					output.xyz = u_xlat16_0.xyz * float3(0.0702700019, 0.0702700019, 0.0702700019) + u_xlat16_1.xyz;
					output.w = 1.0;
					return output;
				}
			ENDCG
			}

			Pass
			{
				ZWrite Off
				Cull Off
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				struct appdata_t
				{
					float4 pos   : POSITION;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 position : SV_POSITION;
					float2 texcoord  : TEXCOORD0;
				};

				sampler2D _MainTex;
				float2 _BlurInfo;

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.position = UnityObjectToClipPos(IN.pos);
					OUT.texcoord = IN.texcoord;
					return OUT;
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					float4 u_xlat0;
					float3 u_xlat16_0;
					float4 u_xlat10_0;
					float4 u_xlat1;
					float3 u_xlat16_1;
					float4 u_xlat10_1;
					float4 u_xlat10_2;
					float4 output;


					u_xlat0.xz = IN.texcoord.xx;
					u_xlat1 = _BlurInfo.yyyy * float4(1.38461494, 3.23076892, -0.615384996, -2.76923108) + IN.texcoord.yyyy;
					u_xlat0.yw = u_xlat1.xz;
					u_xlat10_2 = tex2D(_MainTex, u_xlat0.xy);
					u_xlat10_0 = tex2D(_MainTex, u_xlat0.zw);
					u_xlat16_0.xyz = u_xlat10_0.xyz + u_xlat10_2.xyz;
					u_xlat16_0.xyz = u_xlat16_0.xyz * float3(0.316260993, 0.316260993, 0.316260993);
					u_xlat10_2 = tex2D(_MainTex, IN.texcoord.xy);
					u_xlat16_0.xyz = u_xlat10_2.xyz * float3(0.227026999, 0.227026999, 0.227026999) + u_xlat16_0.xyz;
					u_xlat1.xz = IN.texcoord.xx;
					u_xlat10_2 = tex2D(_MainTex, u_xlat1.xy);
					u_xlat10_1 = tex2D(_MainTex, u_xlat1.zw);
					u_xlat16_1.xyz = u_xlat10_1.xyz + u_xlat10_2.xyz;
					output.xyz = u_xlat16_1.xyz * float3(0.0702700019, 0.0702700019, 0.0702700019) + u_xlat16_0.xyz;
					output.w = 1.0;
					return output;
				}
			ENDCG
			}
		}

	//DummyShader
	/*SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Standard fullforwardshadows
#pragma target 3.0
		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};
		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
		}
		ENDCG
	}*/
}