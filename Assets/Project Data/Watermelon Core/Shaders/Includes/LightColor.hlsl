#ifndef W_LIGHT_COLOR
#define W_LIGHT_COLOR

#ifdef LIGHT_COLOR_ON
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    #include "Includes/Lighting.hlsl"

    half _Specular;
    half _Shiness;
    half4 _SpecularMin;
    half4 _SpecularMax;
#endif

    void GetLightColor(inout half4 color, v2f input)
    {
#ifdef LIGHT_COLOR_ON

        half4 lightColor = half4(LightColorHalf(input.worldPos.xyz), 1);
        color *= lightColor;
        
        half3 lightDir = LightDirectionHalf(input.worldPos);

        half3 lightReflectDirection = reflect(-lightDir, input.normWorld);
        half lightSeeDirection = saturate(dot(lightReflectDirection, normalize(input.viewDir)));

        if(lightSeeDirection < _Specular) 
        {
            lightSeeDirection = 0; 
        } 
        else 
        {
            lightSeeDirection = Remap(lightSeeDirection, half2(_Specular, 1), half2(0, 1));
        }

        half shininessPower = length(pow(lightSeeDirection, _Shiness));

        half4 specColor = lerp(_SpecularMin, _SpecularMax, lightSeeDirection);

        half3 specularReflection = specColor.rgb * shininessPower * specColor.a;

        color += half4(specularReflection, 0);
#endif
    }

#endif