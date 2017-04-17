Shader "Unlit/ColorSelector"
{
	Properties
	{
		_Alpha ("Alpha", float) = 0.8
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert alpha

	    float _Alpha;

		struct Input {
			fixed4 color : COLOR;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			o.Albedo = IN.color.rgb;
			o.Emission = IN.color.rgb * _Alpha;
			o.Alpha = _Alpha;
		}
		ENDCG
	}
}
