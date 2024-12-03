#ifndef W_CURVE
#define W_CURVE

    uniform half _CurveOffset;
    uniform half _CurvePower;

    void GetCurve(inout appdata input, inout v2f output)
    {
#ifdef CURVE_ON
        half pos = output.worldPos.z - _CurveOffset;
        half temp = pos / 32;
        temp = temp * temp * _CurvePower * (pos > 0);

        half4 offset = mul(unity_WorldToObject, half4(0, temp, 0, 0));

        input.pos -= offset;
#endif
    }

#endif