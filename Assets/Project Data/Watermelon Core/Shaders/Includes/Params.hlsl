#ifndef W_PARAMS
#define W_PARAMS

    struct appdata
    {
        float4 pos : POSITION;
        float2 uv : TEXCOORD0;

        float4 color : COLOR0;
#if defined(RIM_ON) || defined(TOON_ON) || defined(LIGHT_COLOR_ON)
        float3 norm :NORMAL;
        float4 tan :TANGENT;
#endif
    };

    struct v2f
    {
        float4 pos : SV_POSITION;
        float2 uv : TEXCOORD0;
        float4 color : COLOR0;

#if !defined(_SHADOWS_NONE) || defined(LIGHT_COLOR_ON) || defined(TOON_ON) || defined(RIM_ON) || defined(CURVE_ON)
#define WORLD_POS
        float4 worldPos: TEXCOORD1;
#endif

#if defined(TOON_ON) || defined(RIM_ON) || defined(LIGHT_COLOR_ON)
#define WORLD_NORM
        float3 normWorld : TEXCOORD2;
#endif

#if defined(RIM_ON) || defined(LIGHT_COLOR_ON)
#define VIEW_DIR
        float3 viewDir : TEXCOORD3;
#endif
    };

    half4 _Color;
    sampler2D _MainTex;

    half Remap(half In, half2 InMinMax, half2 OutMinMax)
    {
        return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
    }

#endif