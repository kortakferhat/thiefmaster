Shader "URP/Unlit/CubemapSky"
{
    Properties
    {
        _Cube("Cubemap", CUBE) = "" {}
        _Exposure("Exposure", Range(0.0, 4.0)) = 1.0
    }
    SubShader
    {
        Tags{ "RenderType"="Opaque" "Queue"="Background" "RenderPipeline"="UniversalPipeline"}
        Cull Front          // kürenin iç yüzeyi
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "UnlitCubemapSky"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURECUBE(_Cube); SAMPLER(sampler_Cube);
            float _Exposure;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 posCS   : SV_POSITION;
                float3 worldPos: TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                float3 posWS = TransformObjectToWorld(v.vertex.xyz); // <-- düzeltme
                o.worldPos = posWS;
                o.posCS = TransformWorldToHClip(posWS);              // <-- düzeltme
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float3 camPosWS = GetCameraPositionWS();
                float3 dir = normalize(i.worldPos - camPosWS);
                half3 col = SAMPLE_TEXTURECUBE(_Cube, sampler_Cube, dir).rgb;
                return half4(col * _Exposure, 1);
            }
            ENDHLSL
        }
    }
}