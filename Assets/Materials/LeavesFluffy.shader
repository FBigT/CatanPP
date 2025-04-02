Shader "Custom/LeavesFluffy"
{
Properties
    {
        _BaseMap ("Albedo (RGB) Alpha (A)", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
        _WindSpeed ("Wind Speed", Float) = 1.0
        _WindStrength ("Wind Strength", Float) = 0.1
        _SSSColor ("Subsurface Color", Color) = (0.5, 1.0, 0.3, 1.0)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="AlphaTest" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { 
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0; 
            };
            struct Varyings { 
                float2 uv : TEXCOORD0; 
                float4 positionCS : SV_POSITION; 
                float3 worldPos : TEXCOORD1; 
            };

            sampler2D _BaseMap;
            float _Cutoff, _WindSpeed, _WindStrength;
            float4 _SSSColor;

            Varyings vert(Attributes v)
            {
                Varyings o;

                o.worldPos = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.worldPos);
                o.uv = v.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 tex = tex2D(_BaseMap, i.uv);
                clip(tex.a - _Cutoff);

                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float rim = 1.0 - saturate(dot(viewDir, float3(0, 1, 0)));
                tex.rgb += _SSSColor * rim * 0.5;
                return tex;
            }
            ENDHLSL
        }
    }
}
