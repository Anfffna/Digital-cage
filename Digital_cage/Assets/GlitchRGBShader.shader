Shader "Custom/GlitchRGB"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Offset ("Offset", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Fog { Mode Off }

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Offset;

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                float2 rUV = uv + _Offset.xy;
                float2 gUV = uv + _Offset.zw;
                float2 bUV = uv;

                fixed r = tex2D(_MainTex, rUV).r;
                fixed g = tex2D(_MainTex, gUV).g;
                fixed b = tex2D(_MainTex, bUV).b;

                return fixed4(r,g,b,1);
            }
            ENDCG
        }
    }
}

