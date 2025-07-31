Shader "Custom/BulletTimeShaderAffect"
{
        Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}
        _TintColor ("Tint Color", Color) = (0.4, 0.2, 0.8, 0.3)
        _EdgeIntensity ("Edge Intensity", Range(0, 2)) = 1.0
        _DistortionStrength ("Distortion", Range(0, 0.1)) = 0.02
        _TimeScale ("Time Scale", Range(0.1, 5.0)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _TintColor;
            float _EdgeIntensity, _DistortionStrength, _TimeScale;

            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            v2f vert (float4 vertex : POSITION, float2 uv : TEXCOORD0)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(vertex);
                o.uv = uv;
                return o;
            }

            float edgeMask(float2 uv)
            {
                float2 d = abs(uv - 0.5) * 2.0;
                return smoothstep(0.6, 1.0, max(d.x, d.y));
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float time = _Time.y * _TimeScale;
                float2 uv = i.uv + sin(i.uv.y * 40.0 + time * 5.0) * _DistortionStrength;
                fixed4 col = tex2D(_MainTex, uv);
                float edge = edgeMask(i.uv);
                col.rgb += edge * _EdgeIntensity;
                col.rgb = lerp(col.rgb, _TintColor.rgb, _TintColor.a);
                return col;
            }
            ENDCG
        }
    }

}
