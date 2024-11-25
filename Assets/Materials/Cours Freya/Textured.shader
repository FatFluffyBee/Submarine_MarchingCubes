Shader "Unlit/Texture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Rock ("Rock", 2D) = "white" {}
        _Pattern ("Pattern", 2D) = "white" {}
        _MipMapLevel ("MipMapLevel", Range(0, 10)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #define TAU 6.28318530718

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _Rock;
            sampler2D _Pattern;         
            float _MipMapLevel; 

            float4 _MainTex_ST;
            
            float GetWave(float2 coord)
            {
                float wave = cos ((coord - _Time.y * 0.1)* TAU * 5) * 0.5 + 0.5;
                wave *= ( 1 -coord);

                return wave;
            }

            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.worldPos = mul( UNITY_MATRIX_M, float4(v.vertex.xyz, 1));
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (Interpolators i) : SV_Target
            {
                float2 topDownProj = i.worldPos.xz;
                float4 moss = tex2D(_MainTex, topDownProj);
                float4 rock = tex2Dlod(_Rock, float4(topDownProj, _MipMapLevel.xx));
                float4 pattern = tex2D(_Pattern, i.uv).x;
                float4 finalColor = lerp( rock, moss, pattern);

                return finalColor;
            }
            ENDCG
        }
    }
}
