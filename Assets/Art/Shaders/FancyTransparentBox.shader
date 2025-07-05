Shader "Custom/FancyTransparentBox"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,0.2)
        _FresnelColor ("Fresnel Color", Color) = (0.5, 0.8, 1, 1)
        _FresnelPower ("Fresnel Power", Range(0.1, 8)) = 4
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        Pass
        {
            Name "FancyTransparent"
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
                float3 viewDirWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            float4 _BaseColor;
            float4 _FresnelColor;
            float _FresnelPower;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionHCS = posInputs.positionCS;
                OUT.normalWS = normalize(normInputs.normalWS);
                OUT.viewDirWS = normalize(GetCameraPositionWS() - posInputs.positionWS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float fresnel = pow(1.0 - saturate(dot(IN.normalWS, IN.viewDirWS)), _FresnelPower);
                float3 finalColor = _BaseColor.rgb + fresnel * _FresnelColor.rgb;
                float finalAlpha = _BaseColor.a + fresnel * _FresnelColor.a * 0.2;

                return float4(finalColor, saturate(finalAlpha));
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
