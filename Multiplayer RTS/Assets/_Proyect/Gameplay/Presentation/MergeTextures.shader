Shader "Unlit/MergeTextures"
{
    Properties
    {
        _FirstTex ("First Texture", 2D) = "black" {}
        _SecondTex("Second Texture", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            ZTest Always
            ZWrite Off


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
                fixed4 col = tex2D(_FirstTex, i.uv) + tex2D(_SecondTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
