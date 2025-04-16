Shader "Custom/TransparentColoredSimple" {
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
	}

	SubShader{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

		ZWrite Off
		Lighting Off
		Fog{ Mode Off }

		Blend SrcAlpha OneMinusSrcAlpha
	
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#pragma multi_compile_instancing 

			struct appdata
			{
				half4 vertex : POSITION;
				half3 normal : NORMAL;
				fixed2 uv : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				half4 vertex : SV_POSITION;
				half3 worldPos : TEXCOORD1;
				half2 uv0 : TEXCOORD0;

				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform fixed3 _Color;
			uniform sampler2D _MainTex;

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.uv0 = v.uv.xy;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 mainTex = tex2D(_MainTex, i.uv0.xy);
				fixed3 col = mainTex * _Color;

				fixed4 colFinal = fixed4(col.rgb, mainTex.a);

				return colFinal;
			}		
			ENDCG
		}
	}
}