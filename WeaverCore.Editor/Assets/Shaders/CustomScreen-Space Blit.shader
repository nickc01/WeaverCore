Shader "Custom/Screen-Space Blit" 
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "IGNOREPROJECTOR" = "true" "QUEUE" = "Geometry" "RenderType" = "Opaque" }
		ZWrite Off
		Cull Off
		LOD 200
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			sampler2D _MainTex;

			struct appdata_t
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float4 texcoord : TEXCOORD0;
			};

			v2f vert(appdata_t v)
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);

				float4 u_xlat1;
				float4 oldPos = o.position;

				oldPos.y = oldPos.y * _ProjectionParams.x;
				u_xlat1.xzw = oldPos.xwy * float3(0.5, 0.5, 0.5);
				o.texcoord.zw = oldPos.zw;
				o.texcoord.xy = u_xlat1.zz + u_xlat1.xw;
				return o;
			}


			fixed4 frag(v2f i) : SV_Target
			{
				float2 samp = i.texcoord.xy / i.texcoord.ww;
				fixed4 c = tex2D(_MainTex, samp);
				return c;
			}
			ENDCG
		}
	}
}