Shader "GPUInstancer/TextureVariationShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
		_MainTex("Albedo Atlas", 2D) = "" {}
		_NormalMap("Normal Atlas", 2D) = "" {}
		_MetallicSmoothness("Metallic/Smoothness Atlas", 2D) = "" {}
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
		_TextureUV("Atlas UV / Offset", Color) = (0, 0, 0.5, 0.5)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        CGPROGRAM
		#include "UnityCG.cginc"
		#include "../../../Shaders/Include/GPUICrowdInclude.cginc"
		#pragma shader_feature_vertex GPUI_CA_TEXTURE
		#pragma multi_compile_instancing
		#pragma instancing_options procedural:setupGPUI
        #pragma surface surf Standard noshadow vertex:textureVariationVert 

        #pragma target 3.0

		sampler2D _MainTex;
		sampler2D _NormalMap;
		sampler2D _MetallicSmoothness;

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		StructuredBuffer<float4> textureUVBuffer;
#else
		UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(float4, _TextureUV)
		UNITY_INSTANCING_BUFFER_END(Props)
#endif

        struct Input
        {
			float2 globalUV;
        };

        half _Smoothness;
        fixed4 _Color;

		void textureVariationVert(inout appdata_full v, out Input o) 
		{
			GPUI_CROWD_VERTEX(v);
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.globalUV = v.texcoord;

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			float4 texUV = textureUVBuffer[gpui_InstanceID];
#else
			float4 texUV = UNITY_ACCESS_INSTANCED_PROP(Props, _TextureUV);
#endif
			o.globalUV.xy *= texUV.zw;
			o.globalUV.xy += texUV.xy;
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			float4 c = tex2D(_MainTex, IN.globalUV) * _Color;
			//float4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
			float4 metallicSmoothness = tex2D(_MetallicSmoothness, IN.globalUV);
			o.Metallic = metallicSmoothness.r;
            o.Smoothness = metallicSmoothness.a * _Smoothness;
			
            o.Alpha = c.a;
			o.Normal = UnpackNormal(tex2D(_NormalMap, IN.globalUV));
        }
        ENDCG
    }
}
