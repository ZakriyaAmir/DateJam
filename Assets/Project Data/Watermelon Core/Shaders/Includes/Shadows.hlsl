#ifndef W_SHADOWS
#define W_SHADOWS

    

#ifndef _SHADOWS_NONE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Includes/Lighting.hlsl"

    half4 _SColor;
#endif

    void GetShadows(inout half4 color, v2f input)
    {
#ifndef _SHADOWS_NONE

#ifdef _SHADOWS_PIXEL
    half shadowAtten = ShadowAttenHalf(input.worldPos.xyz);
#elif _SHADOWS_VERTEX
    half shadowAtten = input.worldPos.w;
#endif
    half4 shadowTemp = half4(lerp(_Color.rgb, _SColor.rgb, _SColor.a), 1);
    color *= lerp(shadowTemp, _Color, shadowAtten);
#endif
    }

#endif