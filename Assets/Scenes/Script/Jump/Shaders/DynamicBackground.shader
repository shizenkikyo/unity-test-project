Shader "Custom/DynamicBackground"
{
    Properties
    {
        _GroundColor ("Ground Color", Color) = (0.6, 0.4, 0.2, 1)
        _SkyColor ("Sky Color", Color) = (0.3, 0.6, 1.0, 1)
        _HighSkyColor ("High Sky Color", Color) = (0.1, 0.2, 0.5, 1)
        _PlayerHeight ("Player Height", Float) = 0
        _ColorChangeHeight ("Color Change Height", Float) = 30
        _MaxHeight ("Max Height", Float) = 100
        _NoiseScale ("Noise Scale", Float) = 1
        _NoiseSpeed ("Noise Speed", Float) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" }
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
            
            float4 _GroundColor;
            float4 _SkyColor;
            float4 _HighSkyColor;
            float _PlayerHeight;
            float _ColorChangeHeight;
            float _MaxHeight;
            float _NoiseScale;
            float _NoiseSpeed;
            
            // ノイズ関数
            float noise(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // ノイズを追加して雲のような効果を作成
                float2 noiseUV = i.uv * _NoiseScale + _Time.y * _NoiseSpeed;
                float noiseValue = noise(noiseUV) * 0.1;
                
                // 高さに基づく色の計算
                float heightRatio = clamp(_PlayerHeight / _ColorChangeHeight, 0, 1);
                float4 baseColor;
                
                if (_PlayerHeight > _ColorChangeHeight)
                {
                    // 高高度では高高度の空色に
                    float highRatio = clamp((_PlayerHeight - _ColorChangeHeight) / (_MaxHeight - _ColorChangeHeight), 0, 1);
                    baseColor = lerp(_SkyColor, _HighSkyColor, highRatio);
                }
                else
                {
                    // 低高度では地面色から空色に
                    baseColor = lerp(_GroundColor, _SkyColor, heightRatio);
                }
                
                // ノイズを適用
                baseColor.rgb += noiseValue;
                
                return baseColor;
            }
            ENDCG
        }
    }
} 