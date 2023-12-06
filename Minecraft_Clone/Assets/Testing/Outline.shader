Shader "Test/Sprite Outline"
{
	Properties
	{
		[MainTexture]
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", COLOR) = (1,1,1,1)
		_OutlineColor("Outline Color", COLOR) = (1,1,1,1)
	}

	SubShader
	{
		Tags{"RenderType"="Opaque"}
		LOD 100
		//Cull Off

		Pass
		{
			CGPROGRAM

			#pragma vertex vertFunc
			#pragma fragment fragFunc

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			fixed4 _Color;
			float4 _MainTex_TexelSize;
			float4 _OutlineColor;

			struct v2f
			{
				float4 position : SV_POSITION;
				float2 texcoord : TEXCOORD0;
			};

			v2f vertFunc(appdata_img v)
			{
				v2f o;
				v.texcoord.x += fmod(_Time.y, 1);
				v.texcoord.y += _SinTime.w;
				o.position = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.texcoord;
				return o;
			}

			fixed4 fragFunc(v2f i) : COLOR
			{
				fixed4 col = tex2D(_MainTex, i.texcoord) * _Color;
				clip(col.a - 0.2);

				fixed up_a = tex2D(_MainTex, i.texcoord + fixed2(0, _MainTex_TexelSize.y)).a;
				fixed down_a = tex2D(_MainTex, i.texcoord + fixed2(0, -_MainTex_TexelSize.y)).a;
				fixed left_a = tex2D(_MainTex, i.texcoord + fixed2(-_MainTex_TexelSize.x, 0)).a;
				fixed right_a = tex2D(_MainTex, i.texcoord + fixed2(_MainTex_TexelSize.x, 0)).a;

				return lerp(_OutlineColor, col, ceil(up_a * down_a * left_a * right_a));
			}



			ENDCG
		}
	}
}