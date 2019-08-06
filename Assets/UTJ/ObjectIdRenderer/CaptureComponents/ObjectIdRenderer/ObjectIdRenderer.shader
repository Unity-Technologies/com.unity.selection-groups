Shader "Utj/ObjectIdRenderer"
{
	Properties
    {
        [PerRendererData] _IdColor  ("ID Color", Color)  = (0.4, 0.4, 0.8, 0)
	}

	SubShader
    {
		Tags { "RenderType" = "Opaque" }

		CGPROGRAM

		#pragma surface surf Lambert

        struct Input
        {
            float2 uv_MainTex;
        };

		sampler2D _MainTex;
        float3 _IdColor;

        void surf (Input IN, inout SurfaceOutput o)
        {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = half3(0, 0, 0);
            o.Alpha = c.a;
//			o.Emission = half3(0, 0.5, 0);
			o.Emission = _IdColor.rgb;
        }

		ENDCG
    }

	Fallback "Diffuse"
}
