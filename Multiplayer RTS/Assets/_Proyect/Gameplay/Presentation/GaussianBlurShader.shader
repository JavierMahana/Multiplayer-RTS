Shader "Custom/GaussianBlurShader"
{

    //first we are going to hard code the sigma value.
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

             float normpdf(float x, float sigma)
             {
                return 0.39894 * exp(-0.5 * x * x / (sigma * sigma)) / sigma;
             }


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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            fixed4 frag (v2f i) : SV_Target
            {
                //matrix size
                const int mSize = 5;
                //kernel size from center
                const int kSize = (mSize - 1) / 2;
                float sigma = 0.8;
                float Z = 0.0;
                float4 finalCol;

                //read out the texels
                for (int x = -kSize; x <= kSize; ++x)
                {
                    float2 uv = i.uv + float2(x * _MainTex_TexelSize.x, 0);
                    float gaussianFactor = normpdf(x, sigma);
                    Z += gaussianFactor;
                    finalCol += (tex2D(_MainTex, uv) * gaussianFactor);
                }
                return fixed4(finalCol.r / Z, finalCol.g / Z, finalCol.b / Z, 1);
            }
            ENDCG
        }


                Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

             float normpdf(float x, float sigma)
             {
                return 0.39894 * exp(-0.5 * x * x / (sigma * sigma)) / sigma;
             }


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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            fixed4 frag(v2f i) : SV_Target
            {
            //matrix size
            const int mSize = 11;
            //kernel size from center
            const int kSize = (mSize - 1) / 2;
            float sigma = 7.0;
            float Z = 0.0;
            float4 finalCol;

            //read out the texels
            for (int y = -kSize; y <= kSize; ++y)
            {
                float2 uv = i.uv + float2(0, y * _MainTex_TexelSize.y);
                float gaussianFactor = normpdf(y, sigma);
                Z += gaussianFactor;
                finalCol += (tex2D(_MainTex, uv) * gaussianFactor);
            }
            return fixed4(finalCol.r / Z, finalCol.g / Z, finalCol.b / Z, 1);
            }
            ENDCG
        }
    }
}
