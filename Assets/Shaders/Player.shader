Shader "Custom/Player"
{
    Properties
    {
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _ReplacedColor ("Replaced Color", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _SecondaryTex ("Effect Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha

        ZWrite off
        Cull off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _SecondaryTex;
            fixed4 _Color;
            fixed4 _ReplacedColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = tex2D(_MainTex, i.uv);
                float testForColor = col.gb == float2(0, 0);

                //col *= _Color;
                //col *= i.color;

                //col = saturate(col - testForColor) + testForColor * tex2D(_SecondaryTex, i.worldPos.xy);
                //col = saturate(col - testForColor) + testForColor * _ReplacedColor * col.rrra;
                col = saturate(col - testForColor) + testForColor * i.color * col.rrra;
                return col;
            }
            ENDCG
        }
    }
}
