
Shader "Custom/WaterVertexOffset_Simple" 
{
    Properties 
	{
		[Header(Main Color)]
        [HDR]_Color ("Color", Color) = (0.5019608,0.5019608,0.5019608,1)
		[HDR]_CubeColor("Cubemap Color", Color) = (1,1,1,1)
		_ColorLerp("Color Lerp", Range(0, 1)) = 0
		_FresnelPow("Frenel Power", float) = 1
		[NoScaleOffset]_MainTex ("Base Color", 2D) = "white" {}

		[Header(Wave Setting)]
		_ExtraHeight("Extra Height", float) = 0
		[NoScaleOffset]_Heightmap1 ("Heightmap01", 2D) = "white" {}
		_WaterMasksVals("Water Masks(X:hgt1Scale,Y:hgt1SubScale,Z:hgt2Scale,W:formScale)", Vector) = (1,1,1,0)
		
		[Toggle(FIRST_WAVE)] _FirstWave("Small Wave", float) = 1
		[Toggle(WAVE_CROSS)] _WaveCROSS("Double Wave CROSS", float) = 0
		[NoScaleOffset][Normal]_HeightNormalmap1("Height Normal Map01", 2D) = "bump" {}
		_Heigt1VectVals("Hgt1(X:dirX,Y:dirY,Z:offset,W:norStr)", Vector) = (1,1,0,1)
		_Heigt1SubVectVals("Hgt1Sub(X:dirX,Y:dirY,Z:offset,W:norStr)", Vector) = (1,1,0,1)

		[Toggle(SECOND_WAVE)] _SecondWave("Big Wave", float) = 0
		[NoScaleOffset][Normal]_HeightNormalmap2("Height Normal Map02", 2D) = "bump" {}
		_Heigt2VectVals ("Hgt2(X:dirX,Y:dirY,Z:offset,W:norStr)", Vector) = (1,1,0,1)

		[Header(Channel Mask)]
		_ChannelMask("Channel Mask(R:Shore,G:Frenel,B:DetailNormal,A:Normal)", 2D) = "white"{}

		[Header(Debug)]
		[Toggle(FRESNEL_DEBUG)] _FresnelDebug("Fresnel Debug", float) = 0
		[Toggle(PEAKWAVE_DEBUG)] _PeakWaveDebug("Peak Wave Debug", float) = 0
    }

    SubShader 
	{
        Tags 
		{
            "RenderType"="Transparent" "Queue" = "Transparent-1"
        }

        Pass 
		{
            Name "FORWARD"
            Tags 
			{
                "LightMode"="ForwardBase" "Queue" = "Transparent-1" "RenderType" = "Transparent"
            }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
			Cull Back
			Lighting Off
			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
			#pragma shader_feature FIRST_WAVE
			#pragma shader_feature WAVE_CROSS
			#pragma shader_feature SECOND_WAVE
			#pragma shader_feature FRESNEL_DEBUG
            #pragma target 3.0
            
			uniform sampler2D _MainTex;
			uniform fixed _FresnelPow;

			uniform fixed _ExtraHeight;
			uniform sampler2D _Heightmap1;
			uniform sampler2D _HeightNormalmap1;
			uniform vector _WaterMasksVals;

			uniform vector _Heigt1VectVals;
			uniform vector _Heigt1SubVectVals;
			uniform sampler2D _HeightNormalmap2;
			uniform vector _Heigt2VectVals;
			uniform fixed3 _Color;
			uniform fixed3 _CubeColor;
			uniform fixed _ColorLerp;

			uniform sampler2D _ChannelMask;

			struct VertexInput 
			{
                fixed4 vertex : POSITION;
                fixed3 normal : NORMAL;
                fixed4 tangent : TANGENT;
                half2 texcoord0 : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct VertexOutput 
			{
                fixed4 pos : SV_POSITION;
				float2 normalCrossUV : TEXCOORD0;
				float4 HeightUV : TEXCOORD1;
				fixed3 normalDir : TEXCOORD2;
                fixed3 tangentDir : TEXCOORD3;
                fixed3 bitangentDir : TEXCOORD4;
				half4 posWorld : TEXCOORD5;
				fixed3 mainTexVertColor : COLOR0;
				fixed3 maskmapColor : COLOR1;

				UNITY_VERTEX_OUTPUT_STEREO
            };

			/// Vertex Func ///
			float2 WaveUVCalculation(in float2 waveUV, in fixed waterMask, in fixed2 waveSpeed) 
			{
				float2 resultUV = waveUV.xy;
				resultUV.xy *= waterMask;
				resultUV.x += _Time.y * waveSpeed.x;
				resultUV.y += _Time.y * waveSpeed.y;

				return resultUV;
			}
			fixed WaveVertexYOffsetCalculation(in fixed hgtmap, in fixed hgtVal)
			{
				fixed ZVector1 = (hgtmap - 0.5) * hgtVal;
			
				return ZVector1;
			}

			/// Fragment Func ///
			fixed3 WaveNormalCalculation(in sampler2D normalmap, in float2 waveUV, in fixed hgtVal ) 
			{
				fixed4 heightNormal_var = tex2D(normalmap, waveUV.xy);
				fixed3 resultNormal = UnpackNormal(heightNormal_var);
				resultNormal = normalize(fixed3((resultNormal.xy * hgtVal), 1)); //Normal Intensity

				return resultNormal;
			}


            VertexOutput vert (VertexInput v) 
			{
                VertexOutput o = (VertexOutput)0;
                
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(VertexOutput, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				
				fixed2 uv0 = v.texcoord0;
				o.maskmapColor.rgb = tex2Dlod(_ChannelMask, fixed4(uv0, 0, 0)).rgb;
				o.mainTexVertColor.rgb = tex2Dlod(_MainTex, fixed4(uv0, 0, 0)).rgb;

////// Vertex Wave:
				fixed waveHeightOffset = 0;
#if defined( FIRST_WAVE)
				o.HeightUV.xy = WaveUVCalculation(v.texcoord0.xy, _WaterMasksVals.x, _Heigt1VectVals.xy);
				fixed _PD_V1 = tex2Dlod(_Heightmap1, float4(o.HeightUV.xy, 0, 0)).r;
				waveHeightOffset += WaveVertexYOffsetCalculation(_PD_V1, _Heigt1VectVals.z);
	#if defined(WAVE_CROSS)
				o.normalCrossUV.xy = WaveUVCalculation(v.texcoord0.xy, _WaterMasksVals.y, _Heigt1SubVectVals.yx); //_Heigt1SubVectVals.yz => switched x and y
				fixed _PD_V1Cross = tex2Dlod(_Heightmap1, float4(o.normalCrossUV.xy, 0, 0)).g;
				waveHeightOffset += WaveVertexYOffsetCalculation(_PD_V1Cross, _Heigt1SubVectVals.z);
	#endif	
#endif
#if defined (SECOND_WAVE)
				o.HeightUV.zw = WaveUVCalculation(v.texcoord0.xy, _WaterMasksVals.z, _Heigt2VectVals.xy);				
				fixed _PD_V2 = tex2Dlod(_Heightmap1, float4(o.HeightUV.zw, 0, 0)).b;
				waveHeightOffset += WaveVertexYOffsetCalculation(_PD_V2, _Heigt2VectVals.z);
#endif
				waveHeightOffset *= o.maskmapColor.b;
				v.vertex.z += waveHeightOffset + _ExtraHeight; // Overall Height Control
////// Vertex Wave Ends //////
				
				o.normalDir = UnityObjectToWorldNormal(v.normal);
				o.tangentDir = normalize(mul(unity_ObjectToWorld, fixed4(v.tangent.xyz, 0)).xyz);
				o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
				//o.mainTexVertColor.a = v.vertex.z;
				o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex);
                
                return o;
            }

            fixed4 frag(VertexOutput i) : COLOR 
			{
				fixed3 maskmap = i.maskmapColor.rgb;
				//fixed vertexZColor = i.mainTexVertColor.a;
				i.normalDir = normalize(i.normalDir);
                fixed3x3 tangentTransform = fixed3x3(i.tangentDir, i.bitangentDir, i.normalDir);                
				fixed3 blendNormal = 0; //Normal Blend Start
#if defined(FIRST_WAVE)		
				blendNormal = WaveNormalCalculation(_HeightNormalmap1, i.HeightUV.xy, _Heigt1VectVals.w);

	#if defined(WAVE_CROSS)
				fixed3 normalCross1Sub = WaveNormalCalculation(_HeightNormalmap1, i.normalCrossUV.xy, _Heigt1SubVectVals.w);
				blendNormal = normalize(fixed3(blendNormal.xy + normalCross1Sub.xy, blendNormal.z * normalCross1Sub.z));
	#endif

#else /// No Small Waves ///
				blendNormal = fixed3(0, 0, 1); // i.normalDir;
#endif

#if defined( SECOND_WAVE)
				fixed3 normalBig2 = WaveNormalCalculation(_HeightNormalmap2, i.HeightUV.zw, _Heigt2VectVals.w);
				blendNormal = normalize(fixed3(blendNormal.xy + normalBig2.xy, blendNormal.z * normalBig2.z));
#endif

				blendNormal = normalize(fixed3((blendNormal.xy * maskmap.b), 1)); // Outer Masking						
				fixed3 normalDirection = normalize(mul(blendNormal, tangentTransform)); // Final Wave normals
////// Cubemap:
				fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
				fixed3 viewReflectDirection = reflect(-viewDir, normalDirection);
				fixed4 sampleCube = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, viewReflectDirection);
////// Fresnel:
				fixed NdotV = saturate(dot(normalDirection, viewReflectDirection));
				fixed k2 = 1-NdotV;
				k2 = k2 * k2;
				fixed fresnel = k2 * _FresnelPow;
////// Alpha: 
				fixed alpha = (fresnel * maskmap.g) * 0.3 + maskmap.r; // maskmap.g for near, maskmap.r for far mask
////// Specular:
				fixed3 specular = lerp(sampleCube.rgb * _CubeColor.rgb, _Color, (_ColorLerp * maskmap.b)) * fresnel;
				specular *= i.mainTexVertColor.rgb; // Texture Color from vertex
////// Final Color:
				fixed3 finalColor = specular; // +peakFinal;
#if defined( FRESNEL_DEBUG) 
				fixed4 finalRGBA = fixed4(fixed3(fresnel, fresnel, fresnel), 1);
#else				
				fixed4 finalRGBA = fixed4(finalColor.rgb, alpha );
#endif          
				return finalRGBA;
            }
        ENDCG
        }
    }
    FallBack "Diffuse"
}
