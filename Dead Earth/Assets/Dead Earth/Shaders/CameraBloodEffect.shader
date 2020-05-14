Shader "Hidden/CameraBloodEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_BloodTex ("Blood Texture", 2D) = "white" {}
		_BloodBump ("Blood Normal", 2D) = "bump" {}
		_BloodAmount ("Blood Amount", Range(0, 1)) = 0
		_BloodDistortion ("Blood Distortion", Range(0, 1)) = 1
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _BloodTex;
            sampler2D _BloodBump;
			float _BloodAmount;
			float _BloodDistortion;


            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
				half lum = Luminance(col.xyz);
				return fixed4(lum, lum, lum, 1.0);
            }
            ENDCG
        }
    }
}
