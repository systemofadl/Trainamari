Shader "Trainamari/CRTPostProcess"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.15
        _VignetteIntensity ("Vignette", Range(0, 1)) = 0.4
        _ColorBleed ("Color Bleed", Range(0, 0.01)) = 0.002
        _Warp ("Screen Warp", Range(0, 0.02)) = 0.005
    }
    
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always
        
        Pass
        {
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float _ScanlineIntensity;
            float _VignetteIntensity;
            float _ColorBleed;
            float _Warp;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                // CRT barrel distortion
                float2 center = uv - 0.5;
                float r2 = dot(center, center);
                uv = uv + center * r2 * _Warp;
                
                // Clamp to prevent sampling outside
                uv = saturate(uv);
                
                // Sample main texture
                fixed4 col = tex2D(_MainTex, uv);
                
                // Color bleeding (chromatic aberration)
                fixed4 colR = tex2D(_MainTex, uv + float2(_ColorBleed, 0));
                fixed4 colB = tex2D(_MainTex, uv - float2(_ColorBleed, 0));
                col.r = colR.r;
                col.b = colB.b;
                
                // Scanlines
                float scanline = sin(uv.y * _ScreenParams.y * 3.14159) * 0.5 + 0.5;
                scanline = pow(scanline, 0.8);
                col.rgb *= 1.0 - _ScanlineIntensity * (1.0 - scanline);
                
                // Vignette
                float2 vignetteUv = i.uv * (1.0 - i.uv);
                float vignette = vignetteUv.x * vignetteUv.y * 15.0;
                vignette = saturate(pow(vignette, _VignetteIntensity));
                col.rgb *= vignette;
                
                // Slight brightness boost to compensate for darkening
                col.rgb *= 1.1;
                
                return col;
            }
            ENDCG
        }
    }
}