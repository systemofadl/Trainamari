Shader "Trainamari/PS1Lit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _VertexSnap ("Vertex Snap", Range(0, 256)) = 120
        _VertexJitter ("Vertex Jitter", Range(0, 0.01)) = 0.003
        _Ambient ("Ambient", Range(0, 1)) = 0.3
        _Diffuse ("Diffuse", Range(0, 1)) = 0.7
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100
        
        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                UNITY_FOG_COORDS(3)
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _VertexSnap;
            float _VertexJitter;
            float _Ambient;
            float _Diffuse;
            
            v2f vert (appdata v)
            {
                v2f o;
                
                // PS1 vertex snapping
                float4 clipPos = UnityObjectToClipPos(v.vertex);
                float2 snapRes = float2(_VertexSnap, _VertexSnap);
                clipPos.xy = floor(clipPos.xy * snapRes) / snapRes;
                
                // Vertex jitter
                float2 jitter = float2(
                    sin(_Time.y * 17.3 + v.vertex.x * 4.7) * _VertexJitter,
                    cos(_Time.y * 23.1 + v.vertex.z * 3.2) * _VertexJitter
                );
                clipPos.xy += jitter;
                
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.vertex = clipPos;
                UNITY_TRANSFER_FOG(o, o.vertex);
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Simple directional light + ambient (PS1 style)
                float3 lightDir = _WorldSpaceLightPos0.xyz;
                float ndotl = dot(normalize(i.worldNormal), lightDir);
                float lighting = _Ambient + _Diffuse * saturate(ndotl);
                
                // Quantize lighting to 4-8 steps (PS1 had limited lighting precision)
                lighting = floor(lighting * 4) / 4;
                
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                col.rgb *= lighting;
                
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                return col;
            }
            ENDCG
        }
    }
}