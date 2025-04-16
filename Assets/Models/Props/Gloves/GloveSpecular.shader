Shader "Custom/SS/GloveSpecular"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
        _Alpha ("Alpha Map", 2D) = "white" {}
        _MainTex ("Albedo", 2D) = "white" {}
        _SpecGlossMap ("Specular/Gloss Map", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        [Enum(UV0,0,UV1,1,UV2,2)] _UVSec ("UV Set", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="AlphaTest" "Queue"="AlphaTest-1"}
        LOD 200

        CGPROGRAM
        #pragma surface surf StandardSpecular
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _SpecGlossMap;
        sampler2D _BumpMap;
        sampler2D _Alpha;
        fixed4 _Color;
        float _Cutoff;
        float _Glossiness;
        float _UVSec;
        struct Input
        {
            float2 uv_MainTex;
            float2 uv2_MainTex;
            float2 uv3_MainTex;
        };

        void surf (Input IN, inout SurfaceOutputStandardSpecular o)
        {
            // UV 선택 (모든 텍스처가 같은 UV 좌표 사용)
            float2 uv = _UVSec == 0 ? IN.uv_MainTex : (_UVSec == 1 ? IN.uv2_MainTex : IN.uv3_MainTex);

            // Albedo
            fixed4 albedo = tex2D(_MainTex, uv) * _Color;
            o.Albedo = albedo.rgb;

            // Alpha
            fixed alpha = tex2D(_Alpha, uv).r;
            clip(alpha - _Cutoff);
            o.Alpha = alpha * albedo.a;

            // Specular & Gloss
            fixed4 specGloss = tex2D(_SpecGlossMap, uv);
            o.Specular = specGloss.rgb;
            o.Smoothness = specGloss.a * _Glossiness;

            // Normal Map
            o.Normal = UnpackNormal(tex2D(_BumpMap, uv));
        }
        ENDCG
    }

    FallBack "Diffuse"
}
