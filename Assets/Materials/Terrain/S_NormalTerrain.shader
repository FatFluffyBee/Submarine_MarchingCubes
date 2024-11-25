Shader "Custom/NormalBasedColorLerp"
{
    Properties
    {
        _ColorTop ("Top Color", Color) = (1, 0, 0, 1)
        _ColorMid ("Middle Color", Color) = (0, 1, 0, 1)
        _ColorBottom ("Bottom Color", Color) = (0, 0, 1, 1)
        _ThresholdTopMid ("Threshold Top/Mid", Range(0, 1)) = 0.5
        _ThresholdMidBottom ("Threshold Mid/Bottom", Range(0, 1)) = 0.2

        _Metallic ("Metallic", Range(0, 1)) = 0.0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        struct Input
        {
            float3 worldNormal : NORMAL;
        };

        fixed4 _ColorTop;
        fixed4 _ColorMid;
        fixed4 _ColorBottom;
        float _ThresholdTopMid;
        float _ThresholdMidBottom;
        float _Metallic;
        float _Smoothness;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Calculate the dot product of the normal with the world up vector
            float upDot = dot(normalize(IN.worldNormal), float3(0, 1, 0));

            // Determine color based on the normal's alignment with the world up vector
            fixed4 color;
            if (upDot >= _ThresholdTopMid)
            {
                color = lerp(_ColorMid, _ColorTop, (upDot - _ThresholdTopMid) / (1.0 - _ThresholdTopMid));
            }
            else if (upDot >= _ThresholdMidBottom)
            {
                color = lerp(_ColorBottom, _ColorMid, (upDot - _ThresholdMidBottom) / (_ThresholdTopMid - _ThresholdMidBottom));
            }
            else
            {
                color = _ColorBottom;
            }

            // Apply the color and material properties to the output
            o.Albedo = color.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
            o.Alpha = color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}