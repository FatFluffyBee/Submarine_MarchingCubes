Shader "Unlit/HealthBar"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("Texture", 2D) = "white" {}
        _Border ("Texture", 2D) = "black" {}
        _Ratio ("Ratio", Range(0, 1)) = 0
        _ColorStart("ColorStart", Color) = (0, 0, 0, 0)
        _ColorEnd("ColorEnd", Color) = (1, 1, 1, 1) 
        _ThresholdStart("ThresholdStart", Range(0, 1)) = 0.2
        _ThresholdEnd("ThresholdEnd", Range(0, 1)) = 0.8
        _Frequency("Frequency", Float) = 1
        _Amplitude("Amplitude", Float) = 1
        _BorderSize("BorderSize", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent"  "Queue" = "Transparent"}
        
        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha  //usually for blending you wanna do src * srcAlpha + dst * (1 - srcAlpha);

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define TAU 6.281

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _Border;
            float4 _ColorStart;
            float4 _ColorEnd;
            float _Ratio;
            float _ThresholdStart;
            float _ThresholdEnd;
            float _Frequency;
            float _Amplitude;
            float _BorderSize;

            float InverseLerp(float a, float b, float v)
            {
                return (v - a) / (b - a);
            }

            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (Interpolators i) : SV_Target
            {
                float2 coords = i.uv;
                coords.x *= 8;

                float2 pointOnLineSeg = float2(clamp(coords.x, 0.5, 7.5), 0.5);
                float sdf = distance(coords, pointOnLineSeg) * 2 - 1;
                clip(-sdf);

                float borderSdf = sdf + _BorderSize;

                float pd = fwidth(borderSdf); //Get an apporx of rate of change of some value //screen space partial derivative of the signed distance field

                float borderMask = 1 - saturate(borderSdf / pd);
                //float borderMask = 1 - step(0, borderSdf);

                //return float4(borderMask.xxx, 1);

                float healthBarMask = _Ratio > i.uv.x;
                float remapRatio = saturate(InverseLerp(_ThresholdStart, _ThresholdEnd, _Ratio));
                float3 healthBarColor = tex2D(_MainTex, float2(remapRatio, i.uv.y));
                
                if(_Ratio < _ThresholdStart) 
                {
                    float pulsation = cos(_Time.y * _Frequency) * _Amplitude + 1;
                    healthBarColor *= pulsation;
                }
                
                healthBarColor *= healthBarMask;
                healthBarColor *= borderMask;
                return float4 (healthBarColor, 1);
            }
            ENDCG
        }
    }
}
