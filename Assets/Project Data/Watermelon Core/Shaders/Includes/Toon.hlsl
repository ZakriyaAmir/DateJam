#ifndef W_TOON
#define W_TOON

#ifdef TOON_ON
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Includes/Lighting.hlsl"

    sampler2D _RampTex;
    half4 _RampMin;
    half4 _RampMax;
#endif

    void GetToon(inout half4 color, v2f input)
    {
#ifdef TOON_ON
        half3 lightDir = LightDirectionHalf(input.worldPos.xyz);
        half tempToon = Remap(dot(input.normWorld, lightDir), half2(-1, 1), half2(0, 1));

        half3 rampTemp =  lerp(_RampMin, _RampMax, tempToon).rgb;
        half3 rampTextTemp = tex2D(_RampTex, half2(tempToon, 0.5)).rgb;

        half3 toon = rampTemp * rampTextTemp;

        color *= half4(toon, 1);
#endif
    }

#endif