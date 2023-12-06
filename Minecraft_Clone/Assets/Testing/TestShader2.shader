// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Test/Coverge"
{
	Properties
	{
		_MainTex("Base Map", 2D) = "white" {}
		_OverlayTex("Overlay Texture", 2D) = "black" {}
		_Direction("Coverage Direction", Vector) = (0,1,0)
		_Intensity("Intensity", Range(0,1)) = 1

	}

	SubShader
	{
		Tags {"RenderPipeline"="UniversalPipeline"}
		LOD 100

		Pass
		{
			CGPROGRAM
				#pragma vertex vertFunc
				#pragma fragment fragFunc

				#include "UnityCG.cginc"

				//struct appdata
				//{
				//	float4 vertex : POSITION;
				//	float3 normal : NORMAL;
				//	float2 uv : TEXCOORD0;
				//	float uv_overlay : TEXCOORD1;
				//};

				struct v2f
				{
					float4 position :POSITION;
					float3 normal : NORMAL;
					float2 uv : TEXCOORD0;
					float uv_overlay : TEXCOORD1;
				};

				
				sampler2D _MainTex;
				float4 _MainTex_ST;

				sampler2D _OverlayTex;
				float4 _OverlayTex_ST;

				float3 _Direction;
				fixed _Intensity;


				v2f vertFunc(appdata_full i)
				{
					v2f o;
					o.position = UnityObjectToClipPos(i.vertex);
					o.uv = TRANSFORM_TEX(i.texcoord, _MainTex);
					o.uv_overlay = TRANSFORM_TEX(i.texcoord, _OverlayTex);
					o.normal = mul(unity_ObjectToWorld, i.normal);
					return o;
				}

				fixed4 fragFunc(v2f i) : COLOR
				{

					fixed dir = dot(normalize(i.normal), _Direction);

					if(dir < (1 - _Intensity))
					{
						dir = 0;
					}

					fixed4 col1 = tex2D(_MainTex , i.uv);
					fixed4 col2 = tex2D(_OverlayTex , i.uv_overlay);

					return lerp(col1, col2, dir);
				}

			ENDCG
		}
		
	}

	
}