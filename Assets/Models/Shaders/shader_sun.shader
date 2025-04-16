Shader "Custom/sunFlare"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		[HDR]_TintColor("Tint Color", color) = (0.5,0.5,0.5,0.5)
	}
	SubShader
	{
		Tags{ "Queue" = "Geometry-3" "IgnoreProjector" = "True" "RenderType" = "Transparent" "PreviewType" = "Plane" }
		Blend SrcAlpha One
		Cull Off Lighting Off ZWrite Off ZTest Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			struct appdata
			{
				half4 vertex : POSITION;
				half2 uv : TEXCOORD0;
				fixed3 color : COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			struct v2f
			{
				half2 uv : TEXCOORD0;
				half4 vertex : SV_POSITION;
				fixed4 color : COLOR;

				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex; half4 _MainTex_ST;
			fixed4 _TintColor;

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX (v.uv.xy, _MainTex);
				o.color.rgba = fixed4(v.color, 1) * _TintColor.rgba;
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				fixed4 col = tex2D(_MainTex, i.uv.xy) * i.color.rgba;

				return col;
			}
			ENDCG
		}
	}
}