Shader "Unlit/FogOfWarShader"
{
    Properties
    {
        _FirstTex("Current Vision Texture", 2D) = "black" {}
        _SecondTex("Explored Vision Texture", 2D) = "black" {}
        _ExploredFogAlpha("Explored Fog Alpha", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "TrasnsparentCutout" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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

            fixed _ExploredFogAlpha;

            sampler2D _FirstTex;
            float4 _FirstTex_ST;

            sampler2D _SecondTex;
            float4 _SecondTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _FirstTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture

                fixed4 col = tex2D(_FirstTex, i.uv) * (1 - _ExploredFogAlpha) + tex2D(_SecondTex, i.uv) * _ExploredFogAlpha;
                fixed alpha = 1 - (col.r + col.g + col.b) / 3;
                fixed4 newcol = fixed4(col.r, col.g, col.b, alpha);
                return newcol;
            }
            ENDCG
        }
    }
}
