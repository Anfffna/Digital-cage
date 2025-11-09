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
            // УБИРАЕМ проблемные директивы и оставляем только нужные:
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
            sampler2D _NoiseTex;
            float4 _MainTex_ST;
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
                
                // Генерируем движущийся шум
                float2 noiseUV = i.uv;
                noiseUV.x += _Time.y * _NoiseSpeed * 0.1;
                noiseUV.y += _Time.y * _NoiseSpeed * 0.05;
                
                fixed4 noise = tex2D(_NoiseTex, noiseUV);
                
                // Добавляем еще один слой шума для большего хаоса
                float2 noiseUV2 = i.uv * 2.0;
                noiseUV2.x -= _Time.y * _NoiseSpeed * 0.07;
                noiseUV2.y += _Time.y * _NoiseSpeed * 0.03;
                
                fixed4 noise2 = tex2D(_NoiseTex, noiseUV2);
                
                // Смешиваем шумы
                fixed4 combinedNoise = (noise + noise2) * 0.5;
                
                // Создаем глитч-эффект с случайными смещениями
                float glitchOffset = (combinedNoise.r - 0.5) * _NoiseIntensity * 0.1;
                float2 glitchUV = i.uv;
                glitchUV.x += glitchOffset;
                
                // Получаем цвет со смещением для глитча
                fixed4 glitchColor = tex2D(_MainTex, glitchUV);
                
                // Смешиваем основной цвет с шумом и глитчем
                fixed4 finalColor = lerp(mainColor, combinedNoise, _NoiseIntensity * combinedNoise.r);
                finalColor = lerp(finalColor, glitchColor, _NoiseIntensity * 0.3);
                
                return finalColor;
            }
            ENDCG
        }
    }
    Fallback "Unlit/Texture"
}