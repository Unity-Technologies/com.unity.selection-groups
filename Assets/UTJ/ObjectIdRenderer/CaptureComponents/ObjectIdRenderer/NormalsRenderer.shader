Shader "Utj/NormalsRenderer"
{
	SubShader {
        Tags { "RenderType"="Opaque" }
        Pass {
        CGPROGRAM

        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"

        struct v2f {
            float4 pos : SV_POSITION;
            float depth : TEXCOORD0;
            float3 worldNormal : TEXCOORD1;
        };

        v2f vert (appdata_base v) {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            float3 worldPos = mul (unity_ObjectToWorld, v.vertex);
            float3 localPos = worldPos - _WorldSpaceCameraPos;
            o.depth = (localPos.z - _ProjectionParams.y)/(_ProjectionParams.z - _ProjectionParams.y);
            o.worldNormal = UnityObjectToWorldNormal(v.normal);
            return o;
        }

        half4 frag(v2f i) : SV_Target {
            return float4(normalize((i.worldNormal+1)*0.5),1);
        }
        ENDCG
        }
    }
}
