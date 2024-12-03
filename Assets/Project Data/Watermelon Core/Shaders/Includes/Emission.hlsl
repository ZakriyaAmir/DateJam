#ifndef W_EMISSION
#define W_EMISSION


#ifdef EMISSION_ON
    half4 _EmissionColor;
    sampler2D _EmissionTex;
#endif

    void GetEmmision(inout half4 color, v2f input)
    {
#ifdef EMISSION_ON
        color += tex2D(_EmissionTex, input.uv) * _EmissionColor;
#endif
    }

#endif