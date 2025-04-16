Shader "Custom/EnvDistorionSimple" 
{
	Properties{
		[NoScaleOffset]_MainTex("Base (RGB)", 2D) = "white" {}
		[Header(Distortion Setting)]
		//[Toggle] _DIST("Distortion On", float) = 0
		[NoScaleOffset]_DistortionMap("DistortionMap", 2D) = "white" {}
		[NoScaleOffset]_MaskMap("MaskMap", 2D) = "white" {}
		_DistortionPower("_DistortionPower", Float) = 0
		_DistortionSpeed ("_DistortionSpeed", Float) = 0.3
		[Header(Caustic Setting)]
		[Toggle] _CAUSTIC("Caustic On", float) = 0
		_CausticMap("Caustic(RG)/Distortion(B)Map", 2D) = "white" {}
		_CausticCo1("Cuastic Color1", Color) = (1,1,1,1)
		_DistPower("Caustic Power", Float) = 0.2
		_DistSpeed("Caustic Speed", Float) = 0.2
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque""Queue" = "Geometry-4" }

		Lighting Off
		Cull Back

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			//#pragma shader_feature _DIST_ON
			#pragma shader_feature _CAUSTIC_ON
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile DISTORTION_ON DISTORTION_OFF

			sampler2D _MainTex;
			sampler2D _DistortionMap;
			sampler2D _MaskMap;

			uniform fixed _DistortionPower;
			uniform fixed _DistortionSpeed;

			sampler2D _CausticMap; float4 _CausticMap_ST;
			uniform fixed3 _CausticCo1;
			uniform fixed _DistPower;
			uniform fixed _DistSpeed;

			uniform half _GetTime; //Global Varriable

			struct appdata
			{
				fixed4 vertex : POSITION;
				half2 uv : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				fixed4 vertex : SV_POSITION;
				float2 uv_MainTex : TEXCOORD0;
				#if defined (DISTORTION_ON) || (_CAUSTIC_ON)
					float4 dist1Dist2UV : TEXCOORD1;
					half3 UVMasksDistValR : COLOR0;
				#endif
				#ifdef _CAUSTIC_ON
					half2 uv_CausticMap : TEXCOORD2;
				#endif

				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv_MainTex = v.uv;
				half2 uv_DistortionMap = 0;
				#ifdef DISTORTION_ON
					uv_DistortionMap = v.uv;
					o.dist1Dist2UV.x = uv_DistortionMap.x + (_GetTime * _DistortionSpeed);
					o.dist1Dist2UV.y = uv_DistortionMap.y;

					o.UVMasksDistValR.b = tex2Dlod(_DistortionMap, half4(o.dist1Dist2UV.xy, 0, 0)).r * _DistortionPower;
				#endif
				#ifdef _CAUSTIC_ON
					o.uv_CausticMap = TRANSFORM_TEX(v.uv, _CausticMap);
					o.dist1Dist2UV.zw = o.uv_CausticMap + _GetTime * _DistSpeed;
				#endif

				#if defined (DISTORTION_ON) || (_CAUSTIC_ON)
					o.UVMasksDistValR.rg = tex2Dlod(_MaskMap, half4(v.uv, 0, 0)).rb; // Make black as the complete 0
				#endif

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed3 col = 0;
				fixed4 finalCol = 0;

				#ifdef DISTORTION_ON
					half2 FinalUV = half2(i.uv_MainTex.x + (i.UVMasksDistValR.b * i.UVMasksDistValR.r), i.uv_MainTex.y);
					col = tex2D(_MainTex, FinalUV).rgb;
				#else
					col = tex2D(_MainTex, i.uv_MainTex).rgb;
				#endif
				#ifdef _CAUSTIC_ON	
					fixed distortion = tex2D(_CausticMap, i.dist1Dist2UV.zw).b * _DistPower;
					fixed caustic1 = tex2D(_CausticMap, half2(i.uv_CausticMap.x + distortion, i.uv_CausticMap.y)).r;
					fixed caustic2 = tex2D(_CausticMap, half2(i.uv_CausticMap.x - distortion, i.uv_CausticMap.y)).g;
					fixed3 causticAdd = fixed3((caustic1 + caustic2) * _CausticCo1) * i.UVMasksDistValR.g;
					finalCol = fixed4(col + causticAdd, 1);
				#else
					finalCol = fixed4(col, 1);
				#endif

				return finalCol;
			}
			ENDCG
		}
		
	}
	Fallback "Mobile/VertexLit"
}