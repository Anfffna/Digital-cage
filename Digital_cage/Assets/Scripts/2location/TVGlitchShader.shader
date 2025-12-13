Shader "Custom/TVGlitchShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "gray" {}
        _NoiseSpeed ("Noise Speed", Float) = 1.0
        _NoiseIntensity ("Noise Intensity", Range(0,1)) = 0.5
        _UseNoise ("Use Noise", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma only_renderers d3d11 glcore gles3 metal vulkan switch
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            float _NoiseSpeed;
            float _NoiseIntensity;
            float _UseNoise;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Основная текстура
                fixed4 mainColor = tex2D(_MainTex, i.uv);
    
                // Если эффект шума выключен, возвращаем основную текстуру
                if (_UseNoise < 0.5)
                    return mainColor;
    
                // Легкое движение шума
                float2 noiseUV = i.uv;
                noiseUV.x += _Time.y * _NoiseSpeed * 0.1;
    
                fixed4 noise = tex2D(_NoiseTex, noiseUV);
    
                // Чуть-чуть черного шума (только темные части текстуры)
                float blackNoise = noise.r * 0.1; // Очень слабый эффект
    
                // Слегка затемняем пиксели где есть шум
                fixed4 finalColor = mainColor * (1.0 - blackNoise * _NoiseIntensity);
    
                return finalColor;
            }
            ENDCG
        }
    }
    Fallback "Unlit/Texture"
}