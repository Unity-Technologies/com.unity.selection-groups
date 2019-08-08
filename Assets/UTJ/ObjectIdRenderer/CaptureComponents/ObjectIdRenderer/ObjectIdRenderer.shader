Shader "Utj/ObjectIdRenderer"
{
	Properties
    {
        [PerRendererData] _IdColor  ("ID Color", Color)  = (0.4, 0.4, 0.8, 0)
	}

	SubShader {
        Tags { "RenderType"="Opaque" }
        Pass {
        CGPROGRAM

        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"
        
        float3 _IdColor;

        struct v2f {
            float4 pos : SV_POSITION;
        };

        v2f vert (appdata_base v) {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            return o;
        }

        half4 frag(v2f i) : SV_Target {
            return half4(_IdColor,1);
        }
        ENDCG
        }
    }

	Fallback "Diffuse"
}
