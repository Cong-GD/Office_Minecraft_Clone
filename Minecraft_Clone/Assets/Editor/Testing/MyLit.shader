Shader "Test/MyLit"
{
	Properties
	{
		_MainTex ("Base Map", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_AnimationSpeed("Animation Speed", Range(0,3)) = 0
		_OffsetSize("Offset size", Range(0, 10)) = 0
	}

	SubShader
	{
		Tags{"RenderPipeline"="UniversalPipeline"}
		LOD 100

		Pass
		{
			CGPROGRAM
				#pragma vertex vertexFunc
				#pragma fragment fragmentFunc
				
				#include "UnityCG.cginc"

				sampler2D _MainTex;
				fixed4 _Color;
				float _AnimationSpeed;
				float _OffsetSize;

				struct appdata
				{
					float4 vertex : POSITION; 
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float4 position : SV_POSITION;
					float2 uv : TEXCOORD0;
				};

				v2f vertexFunc(appdata i)
				{
					v2f o;

					i.vertex.x += sin(_Time.y * _AnimationSpeed + i.vertex.y * _OffsetSize);

					o.position = UnityObjectToClipPos(i.vertex);
					o.uv = i.uv;
					return o;
				}

				fixed4 fragmentFunc(v2f i) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.uv) * _Color;
					return col;
				}


			ENDCG
		}
	}

	
}