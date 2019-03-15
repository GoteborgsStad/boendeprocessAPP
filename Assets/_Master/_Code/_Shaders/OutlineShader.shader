Shader "Custom/Silhouette Only" 
{
	Properties
	{
		_Color("Outline Color", Color) = (0,0,0,1)
		_Outline("Outline width", Range(0.0, 0.5)) = .005
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	struct appdata 
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	struct v2f 
	{
		float4 pos : POSITION;
		float4 color : COLOR;
	};

	uniform float _Outline;
	uniform float4 _Color;

	v2f vert(appdata v) 
	{
		v2f o;

		o.pos = UnityObjectToClipPos(v.vertex);
		
		float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
		float2 offset = TransformViewToProjection(norm.xy);
		offset = normalize(offset);
		o.pos.xy += offset * _Outline;

#if UNITY_REVERSED_Z
		o.pos.z -= 0.01;
#else
		o.pos.z += 0.01;
#endif

		o.color.rgb = _Color.rgb * _Color.a;
		o.color.a = _Color.a;
		return o;
	}
	ENDCG

	SubShader
	{
		Tags { "Queue" = "Transparent" }

		Pass
		{
			Name "BASE"
			Cull Off
			Blend Zero One
			Offset -8, -8

			SetTexture[_Color]
			{
				ConstantColor(0,0,0,0)
				Combine constant
			}
		}

		// note that a vertex shader is specified here but its using the one above
		Pass
		{
			Name "OUTLINE"
			Tags { "LightMode" = "Always" }
			Cull Off
			Blend One OneMinusDstColor

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			half4 frag(v2f i) : COLOR
			{
				return i.color;
			}
			ENDCG
		}
	}

	Fallback "Diffuse"
}