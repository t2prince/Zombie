Shader "Custom/EnvUnlit" {
	Properties{
		[NoScaleOffset]_MainTex("Base (RGB)", 2D) = "white" {}
	}

	SubShader{
		Tags{ "Queue" = "Geometry-4" "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#include "UnityCG.cginc"

			struct appdata_t 
			{
				fixed4 vertex : POSITION;
				fixed2 texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f 
			{
				fixed4 vertex : SV_POSITION;
				fixed2 texcoord : TEXCOORD0;
				UNITY_FOG_COORDS(1)

				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;

			v2f vert(appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.texcoord = v.texcoord;
				o.vertex = UnityObjectToClipPos(v.vertex);
				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = fixed4(tex2D(_MainTex, i.texcoord).rgb, 1);
				UNITY_APPLY_FOG(i.fogCoord, col);
				
				return col;
			}
			ENDCG
		}
	}

}
