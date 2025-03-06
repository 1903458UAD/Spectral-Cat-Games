Shader "Custom/InteractableObjectShader"
{
    Properties
    {
        _HighlightObject ("Highlight Object", Float) = 0
        _TransparentObject ("Transparant Object", Float) = 0
        _HighlightStrength ("Highlight Strength", Float) = 0
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _MetallicGlossMap ("Metallic + Smoothness Map", 2D) = "white" {} 
        _BumpMap ("Normal Map", 2D) = "bump" {} 
        _OcclusionMap ("Ambient Occlusion Map", 2D) = "white" {} 
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _OcclusionStrength ("AO Strength", Range(0,1)) = 1.0
    }

    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull back 
        LOD 100

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _MetallicGlossMap;
        sampler2D _BumpMap;
        sampler2D _OcclusionMap;

        struct Input
        {
            float2 uv_MainTex;
            float2 _MetallicGlossMap;
            float2 _BumpMap;
            float2 _OcclusionMap;
        };

        half _Glossiness;
        half _Metallic;
        half _OcclusionStrength;
        fixed4 _Color;
        float _HighlightObject;
        float _HighlightStrength;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 albedoColor = tex2D(_MainTex, IN.uv_MainTex);

            if (_HighlightObject == 1)
            {
                albedoColor.rgb = lerp(albedoColor.rgb, _Color.rgb, _HighlightStrength);
            }

            o.Albedo = albedoColor.rgb;

            fixed4 metallicGloss = tex2D(_MetallicGlossMap, IN.uv_MainTex);
            o.Metallic = metallicGloss.r * _Metallic;  
            o.Smoothness = metallicGloss.a * _Glossiness;  

            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));

            fixed4 aoTex = tex2D(_OcclusionMap, IN.uv_MainTex);
            o.Occlusion = lerp(1, aoTex.r, _OcclusionStrength);

            o.Alpha = albedoColor.a;
        }
        ENDCG
    }

    FallBack "Standard"
}
