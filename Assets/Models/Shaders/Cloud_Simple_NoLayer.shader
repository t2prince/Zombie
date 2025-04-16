Shader "Custom/Cloud_Simple_NoLayer" 
{
	Properties
	{
		[HDR]_CloudColor("Cloud Color", Color) = (0.5,0.5,0.5,0.5)
		_ShadowColor("Shadow Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex("Cloud Texture", 2D) = "white" {}
		_InfoTex("Alpha & Color Texture", 2D) = "white" {}
		_LayerIntensity("Layer Intensity", Range(0,1)) = 1
		_Speed("Cloud Speed", Vector) = (0,0,0,0)
		_Offset("Shadow Offset", Range(0,1.0)) = 1.0
		_LightPow("Light Power", Range(0,5.0)) = 1.0
	}

	SubShader
	{
		//Tags{ "Queue" = "Transparent-1" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Tags{ "Queue" = "Geometry-3"  "RenderType" = "Transparent" "PreviewType" = "Plane" }

		
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Cull Back Lighting Off ZWrite Off ZTest Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			sampler2D _MainTex; half4 _MainTex_ST;
			sampler2D _InfoTex;
			fixed _LayerIntensity;

			fixed _LightPow;
			fixed2 _Speed;
			fixed _Offset;
			fixed4 _CloudColor;
			fixed4 _ShadowColor;

			
			struct VertexInput
			{
				half3 vertex : POSITION;
				float2 uv : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			struct VertexOutput
			{
				half4 pos : SV_POSITION;
				float2 defaultUV : TEXCOORD0;
				float4 couldUVResizedUV : TEXCOORD1;

				UNITY_VERTEX_OUTPUT_STEREO
			};

			VertexOutput vert(VertexInput v)
			{
				VertexOutput o = (VertexOutput)0;

				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(VertexOutput, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				
				o.defaultUV.xy = v.uv.xy;
				float2 cloudSpeed = frac(_Time.y * _Speed.xy * 0.35);
				o.couldUVResizedUV.xy = v.uv.xy + cloudSpeed; //UV Panning
				o.couldUVResizedUV.zw = TRANSFORM_TEX(o.couldUVResizedUV.xy, _MainTex);

				o.pos = UnityObjectToClipPos(v.vertex);

				return o;
			}
			fixed4 frag(VertexOutput i) : COLOR
			{
				fixed4 layerDirection = tex2D(_InfoTex, i.defaultUV.xy);
				fixed2 offsetUV = (0.5 - layerDirection.rg) * _Offset;

				fixed layerCloudR = tex2D(_MainTex, i.couldUVResizedUV.zw).r;
				fixed cloudAlpha = 1 - (layerCloudR * _LayerIntensity);
				
				fixed layerLightG = tex2D(_MainTex, (i.couldUVResizedUV.xy + offsetUV)).g;
				fixed cloudBlurAlpha = layerLightG * _LayerIntensity;

				fixed4 col;
				col.rgb = _CloudColor.rgb + ((1 - (cloudBlurAlpha * 1.2)) * layerDirection.b * _LightPow);
				col.rgb -= (cloudBlurAlpha * (1 - _ShadowColor));
				col.a = (_CloudColor.a * layerDirection.a) - cloudAlpha;

				fixed4 finalColor = col.rgba;

				return finalColor;
			}
			ENDCG
		}
		

		/*
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask RGB
		Cull Back Lighting Off ZWrite Off ZTest Off
		CGPROGRAM
		#pragma surface surf NoLighting alpha noambient noforwardadd noshadow nolightmap nodirlightmap novertexlights 
		#pragma multi_compile_instancing
		#pragma fragmentoption ARB_precision_hint_fastest
		sampler2D _MainTex;
		sampler2D _MainTexBlur;
		sampler2D _InfoTex;

		fixed _LayerR;
		fixed _LayerG;
		fixed _LayerB;
		fixed _LightPow;
		//fixed _ShadowPow;
		fixed2 _Speed;
		fixed _Offset;
		fixed4 _CloudColor;
		fixed4 _ShadowColor;

		struct Input 
		{
			fixed2 uv_MainTex;
			fixed2 uv_InfoTex;
		};

		fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			fixed4 c;
			c.rgb = s.Albedo;
			c.a = s.Alpha;
			return c;
		}

		void surf(Input IN, inout SurfaceOutput o) {

			fixed2 cloudSpeed = frac(_Time.y *  _Speed.xy);

			half2 cloudUV = IN.uv_MainTex.xy + cloudSpeed;
			fixed4 layerDirection = tex2D(_InfoTex, IN.uv_InfoTex.xy);
			fixed2 offsetUV = fixed2(0.5 - layerDirection.r, 0.5 - layerDirection.g)*_Offset;
			fixed2 cloudShadowUV = cloudUV + offsetUV;

			
			//fixed3 layerCTRL = fixed3(_LayerR, _LayerG, _LayerB);
			//fixed3 layerCloud = tex2D(_MainTex, cloudUV) * layerCTRL;
			//fixed3 layerLight = tex2D(_MainTexBlur, cloudShadowUV) * layerCTRL;
			//fixed cloudAlpha = 1 - saturate(layerCloud.r + layerCloud.g + layerCloud.b);
			//fixed cloudBlurAlpha = saturate(layerLight.r + layerLight.g + layerLight.b);
			
			
			fixed3 layerCloud = tex2D(_MainTex, cloudUV);
			fixed3 layerLight = tex2D(_MainTexBlur, cloudShadowUV);
			fixed cloudAlpha = 1- saturate((layerCloud.r*_LayerR) + (layerCloud.g*_LayerG) + (layerCloud.b*_LayerB));
			fixed cloudBlurAlpha = saturate((layerLight.r*_LayerR) + (layerLight.g*_LayerG) + (layerLight.b*_LayerB));
			
			fixed4 col;
			//col.rgb = _CloudColor + cloudAlpha*layerDirection.b*_ShadowColor + cloudBlurAlpha*_ShadowColor*_ShadowColor.a;
			//col.rgb = _CloudColor + cloudAlpha*layerDirection.b*_LightPow*_CloudColor - cloudBlurAlpha*(1 - _ShadowColor)*(1 - layerDirection.b);
			col.rgb = _CloudColor + ((1 - (cloudBlurAlpha * 1.2)) * layerDirection.b * _LightPow) - (cloudBlurAlpha * (1 - _ShadowColor));

			col.a = (_CloudColor.a * layerDirection.a) - cloudAlpha;
			//col.a = layerDirection.a;
			o.Albedo = col.rgb;
			o.Alpha = col.a;

		}
		ENDCG
		*/
	}
	Fallback "Mobile/VertexLit"
}

