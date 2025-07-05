Shader "Custom/FancyTransparentBoxPro"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,0.2)
        _FresnelColor ("Fresnel Color", Color) = (0.5, 0.9, 1, 1)
        _FresnelPower ("Fresnel Power", Range(0.1, 8)) = 4

        _GradientTopColor ("Top Color", Color) = (1,1,1,0.4)
        _GradientBottomColor ("Bottom Color", Color) = (1,1,1,0.1)
        _GradientHeight ("Gradient Height", Float) = 1.0

        _TimeScrollSpeed ("Scroll Speed", Float) = 0.5
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 300

        Pass
        {
            Name "FancyTransparentPass"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
            };

            float4 _BaseColor;
            float4 _FresnelColor;
            float _FresnelPower;
            float4 _GradientTopColor;
            float4 _GradientBottomColor;
            float _GradientHeight;
            float _TimeScrollSpeed;
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionHCS = posInputs.positionCS;
                OUT.normalWS = normalize(normInputs.normalWS);
                OUT.worldPos = posInputs.positionWS;
                OUT.viewDirWS = normalize(GetCameraPositionWS() - posInputs.positionWS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Fresnel effect
                float fresnel = pow(1.0 - saturate(dot(IN.normalWS, IN.viewDirWS)), _FresnelPower);

                // Gradient effect by world height
                float heightFactor = saturate((IN.worldPos.y + _Time * _TimeScrollSpeed) / _GradientHeight);
                float4 gradientColor = lerp(_GradientBottomColor, _GradientTopColor, heightFactor);

                // Combine all
                float3 finalColor = _BaseColor.rgb + fresnel * _FresnelColor.rgb + gradientColor.rgb;
                float finalAlpha = _BaseColor.a + fresnel * _FresnelColor.a * 0.2 + gradientColor.a;

                return float4(finalColor, saturate(finalAlpha));
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
