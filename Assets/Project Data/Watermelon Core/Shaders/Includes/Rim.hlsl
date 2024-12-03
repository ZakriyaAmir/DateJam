#ifndef W_RIM
#define W_RIM

#ifdef RIM_ON
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Includes/Lighting.hlsl"
    #include "Includes/RimLighting.hlsl"
#endif

#ifdef RIM_ON
    half4 _RimColor;
    half _RimMin;
    half _RimMax;
    half _DirRim;
#endif

    void GetRim(inout half4 color, v2f input)
    {
#ifdef RIM_ON

        half4 dirRim = half4(RimDirLightingHalf(_RimMin, _RimMax, _RimColor, input.viewDir, input.normWorld, input.worldPos), 0);
        half4 rim = half4(RimLightingHalf(_RimMin, _RimMax, _RimColor, input.viewDir, input.normWorld), 0);

        color += lerp(rim, dirRim, _DirRim);
#endif
    }

#endif