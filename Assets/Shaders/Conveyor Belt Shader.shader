Shader "Custom/Conveyor Belt Shader"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
        [PerRendererData] _ConveyorScaleX("Conveyor Scale X", Float) = 1
        [PerRendererData] _ConveyorScaleY("Conveyor Scale Y", Float) = 1
        [PerRendererData] _ConveyorOffsetX("Conveyor Offset X", Float) = 1
        [PerRendererData] _ConveyorOffsetY("Conveyor Offset Y", Float) = 1
        _BeltColor("Belt Color", Color) = (1,1,1,1)
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

            CGPROGRAM
            #pragma surface surf Lambert vertex:vert nofog nolightmap nodynlightmap keepalpha noinstancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            struct Input
            {
                float2 uv_MainTex;
                fixed4 color;
            };

            float _ConveyorScaleX;
            float _ConveyorScaleY;
            float _ConveyorOffsetX;
            float _ConveyorOffsetY;

            void vert(inout appdata_full v, out Input o)
            {
                v.vertex = UnityFlipSprite(v.vertex, _Flip);

                #if defined(PIXELSNAP_ON)
                v.vertex = UnityPixelSnap(v.vertex);
                #endif

                UNITY_INITIALIZE_OUTPUT(Input, o);

                o.color = v.color * _Color * _RendererColor;
            }

            void surf(Input IN, inout SurfaceOutput o)
            {
                IN.uv_MainTex += float2(_ConveyorOffsetX, _ConveyorOffsetY);
                IN.uv_MainTex *= float2(_ConveyorScaleX, _ConveyorScaleY);

                fixed4 c = SampleSpriteTexture(IN.uv_MainTex) * IN.color;
                o.Albedo = c.rgb * c.a;
                o.Alpha = c.a;
            }
            ENDCG
        }

            Fallback "Sprites/Diffuse"
}