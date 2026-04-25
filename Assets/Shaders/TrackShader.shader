Shader "Trainamari/TrackShader"
{
    Properties
    {
        _MainTex ("Rail Texture", 2D) = "white" {}
        _RailColor ("Rail Color", Color) = (0.5, 0.5, 0.5, 1)
        _TieColor ("Tie Color", Color) = (0.35, 0.25, 0.15, 1)
        _BallastColor ("Ballast Color", Color) = (0.4, 0.4, 0.4, 1)
        _VertexSnap ("Vertex Snap", Range(0, 256)) = 120
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _RailColor;
            float4 _TieColor;
            float4 _BallastColor;
            float _VertexSnap;
            
            v2f vert (appdata v)
            {
                v2f o;
                
                float4 clipPos = UnityObjectToClipPos(v.vertex);
                
                // PS1 vertex snapping
                float2 snapRes = float2(_VertexSnap, _VertexSnap);
                clipPos.xy = floor(clipPos.xy * snapRes) / snapRes;
                
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.vertex = clipPos;
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Simple lighting
                float3 lightDir = normalize(float3(0.5, 1.0, 0.3));
                float ndotl = dot(normalize(i.worldNormal), lightDir);
                float lighting = 0.3 + 0.7 * saturate(ndotl);
                lighting = floor(lighting * 4) / 4; // PS1 quantize
                
                // Pattern: rails at edges, ties as horizontal stripes, ballast in between
                float2 uv = i.uv;
                
                // Horizontal ties
                float tiePattern = step(0.3, frac(uv.y * 20.0));
                
                // Vertical rails at edges
                float railPattern = step(0.85, uv.x) + step(uv.x, 0.15);
                
                // Combine: ballast base, ties on top, rails on top of everything
                fixed4 col = _BallastColor;
                col = lerp(col, _TieColor, tiePattern * step(uv.x, 0.85) * step(0.15, uv.x));
                col = lerp(col, _RailColor, saturate(railPattern));
                
                col.rgb *= lighting;
                
                return col;
            }
            ENDCG
        }
    }
}