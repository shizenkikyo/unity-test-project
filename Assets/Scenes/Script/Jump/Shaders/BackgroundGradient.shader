Shader "Custom/BackgroundGradient"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.2, 0.4, 0.8, 1)
        _BottomColor ("Bottom Color", Color) = (0.8, 0.6, 0.4, 1)
        _GradientHeight ("Gradient Height", Float) = 10
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
            
            float4 _TopColor;
            float4 _BottomColor;
            float _GradientHeight;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float gradient = i.uv.y;
                return lerp(_BottomColor, _TopColor, gradient);
            }
            ENDCG
        }
    }
} 