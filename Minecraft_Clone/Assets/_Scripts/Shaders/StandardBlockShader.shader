Shader "Minecraft/Blocks"
{
    Properties {
        _MainTex ("Block Texture Atlas", 2D) = "white" {}
    }

    SubShader {
        Tags {"Queue"="Alphatest" "IgnoreProjector" ="True" "RenderType"="TransparentCutout"}
        LOD 100
        Lighting Off

        Pass {
            CGPROGRAM
                #pragma vertex vertFunction
                #pragma fragment fragFunction
                #pragma target 2.0
              
                #include "UnityCG.cginc"
                
struct appdata
{
    float4 vertex : POSITION;

    float2 uv : TEXCOORD0;
};

struct v2f
{
    float4 vertex : SV_Position;
    float2 uv : TEXCOORD0;
};

sampler2D _MainTex;

v2f vertFunction(appdata v)
{
    v2f o;
    
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    
    return o;
}

fixed4 fragFunction(v2f i) : SV_Target
{
    fixed4 col = tex2D(_MainTex, i.uv);
    clip(col.a - 1);
    col = lerp(col, float4(0, 0, 0, 1), 0);
    return col;
}

ENDCG

        }
    }
}