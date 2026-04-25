Shader "Trainamari/PS1Unlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _VertexSnap ("Vertex Snap", Range(0, 256)) = 120
        _VertexJitter ("Vertex Jitter", Range(0, 0.01)) = 0.003
        _AffineCorrection ("Affine Correction", Range(0, 1)) = 0
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _VertexSnap;
            float _VertexJitter;
            float _AffineCorrection;
            
            v2f vert (appdata v)
            {
                v2f o;
                
                // Transform to clip space
                float4 clipPos = UnityObjectToClipPos(v.vertex);
                
                // PS1 vertex snapping - snap to pixel grid
                // This gives that characteristic "swimming" vertex effect
                float2 snapRes = float2(_VertexSnap, _VertexSnap);
                clipPos.xy = floor(clipPos.xy * snapRes) / snapRes;
                
                // Vertex jitter on camera rotation
                // Adds wobble when camera moves
                float2 jitter = float2(
                    sin(_Time.y * 17.3 + v.vertex.x * 4.7) * _VertexJitter,
                    cos(_Time.y * 23.1 + v.vertex.z * 3.2) * _VertexJitter
                );
                clipPos.xy += jitter;
                
                // Affine texture mapping (PS1 style - no perspective correction)
                // When _AffineCorrection = 0, full affine (PS1 look)
                // When _AffineCorrection = 1, corrected (modern look)
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                o.vertex = clipPos;
                UNITY_TRANSFER_FOG(o, o.vertex);
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Sample texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                
                // Apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                return col;
            }
            ENDCG
        }
    }
}