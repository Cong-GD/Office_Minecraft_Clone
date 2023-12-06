Shader "Test/WaterRender"
{
	Properties
	{
		_NoiseMap("Noise Map", 2D) = "white" {}
		_Color("Color", COLOR) = (1,1,1,1)
		_Scale("Scale", Range(0, 1)) = 1
		_Amplitude("Amplitude", Range(0,100)) = 1
		_RepeatTime("Repeat Time", Range(0, 10)) = 1
	}

	SubShader
	{
		Tags
		{ 
			"RenderType"= "Transparent"
		}
		LOD 100
		Cull Off

		Pass
		{	
			CGPROGRAM

			#pragma vertex vertFunc
			#pragma fragment fragFunc
			
			#include "UnityCG.cginc"

			sampler2D _NoiseMap;
			float2 _NoiseMap_TexelSize;
			float4 _Color;
			float _Scale;
			float _Amplitude;
			float _RepeatTime;

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
			};

			float genNoise(float x, float y, float2 offset)
			{
				x = (x + offset.x) * _Scale;
				y = (y + offset.y) * _Scale;
				float2 uv = (x * _NoiseMap_TexelSize.x, y * _NoiseMap_TexelSize.y);
				uv = fmod(uv, float2(1, 1));
				fixed4 col = tex2D(_NoiseMap, float2(1,1));
				return 1 - col.x * col.y * col.z;
			}

			v2f vertFunc(appdata v)
			{
				v2f o;
				float4 vert = v.vertex;
				//float time = fmod(_Time.y, _RepeatTime);
				//float noiseValue = genNoise(vert.x, vert.z, float2(time, time));
				//vert.y += noiseValue * _Amplitude;
				vert.y += _SinTime.w * cos(vert.x) * sin(vert.z);

				o.position = UnityObjectToClipPos(vert);
				return o;
			}

			fixed4 fragFunc(v2f i) : COLOR
			{
				//fixed4 col = tex2D(_NoiseMap, float2(1,1));
				return _Color;
			}

			ENDCG
		}

	}
}