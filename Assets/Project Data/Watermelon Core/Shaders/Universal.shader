Shader "WMelon/Universal"
{
    Properties
    {
        [HideInInspector]_Transparent("Transparent", float) = 0

        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
        
        //[Toggle(RECEIVE_SHADOWS_ON)]_ReceiveShadowsOn("Receive Shadows", float) = 1
        [KeywordEnum(None, Vertex, Pixel)] _Shadows("Receive Shadows", Float) = 2
        _SColor("Shadow Color", Color) = (0.5,0.5,0.5,1)

        [Toggle(EMISSION_ON)]_EmissionOn("Emission", float) = 0
        [HDR]_EmissionColor("Emission Color", Color) = (0,0,0,0)
        _EmissionTex("Emission Texture", 2D) = "black" {}

        [Toggle(RIM_ON)]_RimOn("Rim", float) = 0
        _RimColor("Rim Color", Color) = (1,1,1,1)
        _RimMin("Rim Min", range(0, 1)) = 0.5
        _RimMax("Rim Max", range(0, 1)) = 1
        _DirRim("Directional Rim", range(0, 1)) = 0
        
        [Toggle(LIGHT_COLOR_ON)]_LightColorOn("Uses Light Color", float) = 0

        _Specular("Specular Highlights", range(0, 1)) = 0.1
        _Shiness("Specular shiness", range(0.1, 1)) = 0.1
        _SpecularMin("Specular Min", Color) = (0,0,0,0)
        _SpecularMax("Specular Max", Color) = (1,1,1,1)

        [Toggle(TOON_ON)]_ToonOn("Toon", float) = 0

        _RampTex("Texture", 2D) = "white" {}
        _RampMin("Ramp Min", Color) = (0,0,0,1)
        _RampMax("Ramp Max", Color) = (1,1,1,1)

        [Toggle(CURVE_ON)]_CurveOn("Curve", float) = 0

        [Toggle(OUTLINE_ON)] _OutlineOn("Enable Outline", Int) = 0
        _OutlineWidth("Outline Width", Float) = 1.0
        _OutlineColor("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineScale("Outline Scale", Float) = 1.0
        _OutlineDepthOffset("Outline Depth Offset", Range(0, 1)) = 0.0
        _CameraDistanceImpact("Outline Camera Distance Impact", Range(0, 1)) = 0.0


        [HideInInspector]_SrcBlend ("", Float) = 1
        [HideInInspector]_DstBlend ("", Float) = 0
        [HideInInspector]_ZWrite ("", Float) = 1
        [HideInInspector]_Cull ("", Float) = 0
    }

    SubShader
    {
        Cull back

        Tags { 
            "RenderType" = "Opaque" 
    
            "RenderPipeline" = "UniversalPipeline"
            "LightMode" = "UniversalForward"

            "Queue" = "AlphaTest"
            "IgnoreProjector" = "True"

            "UniversalMaterialType" = "Lit" 
            "ShaderModel" = "4.5"
        }

        Pass
        {
            Name "MainPass"

            Tags{"LightMode" = "UniversalForward"}

            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]

            HLSLPROGRAM

            #pragma shader_feature_local CURVE_ON
            #pragma shader_feature_local EMISSION_ON
            #pragma shader_feature_local LIGHT_COLOR_ON
            #pragma shader_feature_local RIM_ON
            #pragma shader_feature_local _SHADOWS_NONE _SHADOWS_VERTEX _SHADOWS_PIXEL
            #pragma shader_feature_local TOON_ON

#ifndef _SHADOWS_NONE
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _SHADOWS_SOFT
#endif

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            #include "Includes/Params.hlsl"
            #include "Includes/Emission.hlsl"
            #include "Includes/Shadows.hlsl"
            #include "Includes/Toon.hlsl"
            #include "Includes/LightColor.hlsl"
            #include "Includes/Rim.hlsl"
            #include "Includes/Curve.hlsl"
            #include "Includes/Utils.hlsl"

            v2f vert(appdata input) 
            {
                v2f output = (v2f)0;

                GetWorldPos(input, output);

                GetCurve(input, output);

                GetWorldNormal(input, output);
                GetViewDir(input, output);

                output.pos = TransformObjectToHClip(input.pos.xyz);
                output.uv = input.uv;
                output.color = input.color;

                return output;
            }


            float4 frag(v2f input) : COLOR
            {
                half4 color = tex2D(_MainTex, input.uv) * _Color * input.color;

                GetLightColor(color, input);
                GetToon(color, input);
                GetRim(color, input);
                GetShadows(color, input);
                GetEmmision(color, input);

                //color.a = 0;
                return color;
            }

            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Tags{"LightMode" = "SRPDefaultUnlit"}

            Cull Front

            HLSLPROGRAM

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"


            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile OUTLINE_ON
            #pragma multi_compile_fog

			/* start CurvedWorld */
			//#define CURVEDWORLD_BEND_TYPE_CLASSICRUNNER_X_POSITIVE
			//#define CURVEDWORLD_BEND_ID_1
			//#pragma shader_feature_local CURVEDWORLD_DISABLED_ON
			//#pragma shader_feature_local CURVEDWORLD_NORMAL_TRANSFORMATION_ON
			//#include "Assets/Amazing Assets/Curved World/Shaders/Core/CurvedWorldTransform.cginc"
			/* end CurvedWorld */

            struct VertexInput
            {
                float4 position : POSITION;
                float3 normal : NORMAL;
            };

            struct VertexOutput
            {
                float4 position : SV_POSITION;
                float3 normal : NORMAL;

                float fogCoord : TEXCOORD1;
            };

            half4 _OutlineColor;
            half _OutlineWidth;
            half _OutlineScale;
            half _OutlineDepthOffset;
            half _CameraDistanceImpact;


            float4 ObjectToClipPos(float4 pos)
            {
                return mul(UNITY_MATRIX_VP, mul(UNITY_MATRIX_M, float4(pos.xyz, 1)));
            }

            VertexOutput vert(VertexInput v)
            {
                UNITY_SETUP_INSTANCE_ID(v);

                VertexOutput o = (VertexOutput)0;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                #if defined(OUTLINE_ON)
                float4 clipPosition = ObjectToClipPos(v.position * _OutlineScale);
                const float3 clipNormal = mul((float3x3)UNITY_MATRIX_VP, mul((float3x3)UNITY_MATRIX_M, v.normal));
                const half outlineWidth = _OutlineWidth;
                const half cameraDistanceImpact = lerp(clipPosition.w, 4.0, _CameraDistanceImpact);
                const float2 aspectRatio = float2(_ScreenParams.x / _ScreenParams.y, 1);
                const float2 offset = normalize(clipNormal.xy) / aspectRatio * outlineWidth * cameraDistanceImpact * 0.005;
                clipPosition.xy += offset;
                const half outlineDepthOffset = _OutlineDepthOffset;

                #if UNITY_REVERSED_Z
                clipPosition.z -= outlineDepthOffset * 0.1;
                #else
                clipPosition.z += outlineDepthOffset * 0.1 * (1.0 - UNITY_NEAR_CLIP_VALUE);
                #endif

                o.position = clipPosition;
                o.normal = clipNormal;

                o.fogCoord = ComputeFogFactor(o.position.z);
                #endif

                return o;
            }

            half4 frag(VertexOutput i) : SV_TARGET
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                half4 color = _OutlineColor;
                color.rgb = MixFog(color.rgb, i.fogCoord);
                return color;
            }
            ENDHLSL


        }

        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"

            ENDHLSL
        }
    }

    CustomEditor "Watermelon.Shader.UniversalGUI"
}
