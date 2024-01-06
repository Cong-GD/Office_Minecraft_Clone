Shader "Minecraft/Blocks"
{
    Properties {
        _MainTex ("Block Texture Atlas", 2D) = "white" {}
        _TintColor("Tint Color", Color) = (1,1,1,1)
        _Transparency("Transparent", Range(0.0,0.5)) = 0.25
        _CutoutThresh("Cutout Threshold", Range(0.0,1.0)) = 0.2
        _Distance("Distance", Float) = 1.0
        _Amplitude("Amplitude", Float) = 1.0
        _Amount("Amount", Float) = 1.0
    }

    SubShader {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
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
                    float4 vertex : SV_POSITION;
                    float2 uv : TEXCOORD0;
                };

                sampler2D _MainTex;
                float4 _TintColor;
                float _Transparency;
                float _CutoutThresh;
                float _Distance;
                float _Amplitude;
                float _Speed;
                float _Amount;

                v2f vert(appdata v)
                {
                    v2f o;
                    //v.vertex.x += sin(_Time.y * _Speed + v.vertex.y * _Amplitude) * _Distance * _Amount;
                    v.vertex.x += sin(_Time.y * _Amplitude) * _Distance;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 col = tex2D(_MainTex, i.uv) * _TintColor;
                    col.a = _Transparency;
                    //col.a = sin(_Time.y);
                    clip(col.r - _CutoutThresh);
                    return col;
                }

            ENDCG

        }
    }
}