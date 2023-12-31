Shader "Custom/Sewage"
{
    Properties
    {
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _ReplacedColor ("Replaced Color", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        _SecondaryTex ("Sewage", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
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
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
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
                float2 flowUv : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.flowUv = o.uv;
                o.flowUv += _Time.y * 0.005;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float distance = 1;
                float3 warble = tex2D(_NoiseTex, i.worldPos / 32 + _Time.y * 0.05);
                warble += tex2D(_NoiseTex, float2(i.worldPos.x / 20 + _Time.y * 0.06, i.worldPos.y / 40 - _Time.y * 0.03));
                //clip(clippingGradient * warble.x - 0.045);
                float3 distortion = (warble - 0.5) * distance * 0.01;
                //return fixed4(warble.xy, 1, 1);

                // sample the texture
                float4 col = tex2D(_MainTex, i.uv + distortion.xx);
                col = tex2D(_SecondaryTex, float2(i.uv.x + distortion.x * 2 + sin(i.uv.y + _Time.y * 2) * 0.005, i.uv.y + _Time.y * 0.15)) * col;
                //float testForColor = col.gb == float2(0, 0);

                //col *= _Color;
                //col *= i.color;

                //col = saturate(col - testForColor) + testForColor * tex2D(_SecondaryTex, i.worldPos.xy);
                //col = saturate(col - testForColor) + testForColor * _ReplacedColor * col.rrra;
                //col = saturate(col - testForColor) + testForColor * i.color * col.rrra;
                return col;
            }
            ENDCG
        }
    }
}
