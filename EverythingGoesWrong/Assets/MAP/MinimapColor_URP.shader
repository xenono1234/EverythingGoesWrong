Shader "Custom/MinimapColorURP"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        
        Pass
        {
            Name "MinimapColorPass"
            Tags { "LightMode"="UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            uniform float4 _BaseColor;
            
            struct Attributes
            {
                float4 positionOS : POSITION;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float4 pos = float4(IN.positionOS.xyz, 1.0);
                OUT.positionCS = TransformObjectToHClip(pos);
                return OUT;
            }
            
            half4 frag(Varyings IN) : SV_Target
            {
                // Simply output the override color.
                return half4(_BaseColor.rgb, 1);
            }
            ENDHLSL
        }
    }
}
