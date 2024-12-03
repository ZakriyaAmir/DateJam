#ifndef W_UNILS
#define W_UNILS

    void GetWorldPos(appdata input, inout v2f output)
    {
#ifdef WORLD_POS
    VertexPositionInputs positionInputs = GetVertexPositionInputs(input.pos.xyz);
    output.worldPos = float4(positionInputs.positionWS, 1);

#ifdef _SHADOWS_VERTEX
    output.worldPos.w = ShadowAttenHalf(positionInputs.positionWS);
#endif
#endif 
    }

    void GetWorldNormal(appdata input, inout v2f output)
    {
#ifdef WORLD_NORM
        VertexNormalInputs normalInputs = GetVertexNormalInputs(input.norm, input.tan);
        output.normWorld = normalInputs.normalWS;
#endif
    }

    void GetViewDir(appdata input, inout v2f output)
    {
#ifdef VIEW_DIR
        output.viewDir = GetWorldSpaceViewDir(output.worldPos.xyz);
#endif
    }

#endif