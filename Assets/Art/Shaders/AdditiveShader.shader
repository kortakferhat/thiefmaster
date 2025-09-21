Shader "Custom/URP_AdditiveUnlit"
{
    Properties
    {
        _BaseMap ("Base Map (RGBA)", 2D) = "white" {}
        _BaseColor ("Tint", Color) = (1,1,1,1)
        _Intensity ("Intensity", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderPipeline"="UniversalPipeline"
        }

        Cull Off
        ZWrite Off
        ZTest LEqual
        Blend One One

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // ---- Doku & Sampler tanımları (eksik olan kısım) ----
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            float4 _BaseMap_ST;

            float4 _BaseColor;
            float  _Intensity;

            Varyings vert (Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v,o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                o.color = v.color * _BaseColor;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);

                // Kenarları yumuşatmak için alfanın RGB’ye uygulanması
                tex.rgb *= tex.a;

                half3 rgb = tex.rgb * i.color.rgb * _Intensity;
                return half4(rgb, 1.0h); // Additive: hedef += rgb
            }
            ENDHLSL
        }
    }

    FallBack Off
}
