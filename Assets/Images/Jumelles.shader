Shader "AR/Jumelles" {
	Properties{ _MainTex("", any) = "" {} }

	CGINCLUDE

	sampler2D _MainTex;
	float _Delta_x, _Delta_y;

	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 border : TEXCOORD1;
	};

	v2f vert(float4 vertex : POSITION, float2 uv : TEXCOORD0)
	{
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, vertex);
		o.uv = uv;
		o.border = (mul(UNITY_MATRIX_MV, vertex) - 0.5) * 3 + 0.5;
		o.border.x += _Delta_x;
		o.border.x += _Delta_y;
		return o;
	}

	fixed3 frag(v2f i) : SV_Target
	{
		fixed3 pixel = tex2D(_MainTex, (i.uv - 0.5) * 1 + 0.5);
	    float num = i.border.x * (1 - i.border.x);
		num = max(num, 0);
	    pixel *= num * i.border.y * (1 - i.border.y) * 16;
	    return pixel;
	}

	ENDCG

	SubShader{
		Pass{
			ZTest Always Cull Off ZWrite Off

			CGPROGRAM
#pragma vertex vert
#pragma fragment frag
			ENDCG
		}
	}
	Fallback off
}