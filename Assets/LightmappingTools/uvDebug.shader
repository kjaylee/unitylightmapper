Shader "ExternalLightmappingTool/uvDebug" {
     SubShader {
          Pass {
               Cull Off Blend One One ZWrite Off Fog { Mode Off }
               CGPROGRAM
               #pragma vertex vert

               struct appdata {
                    float4 vertex : POSITION;
                    float4 texcoord : TEXCOORD0;
                    float4 texcoord1 : TEXCOORD1;
               };

               struct v2f {
                    float4 pos : POSITION;
                    float dummy : TEXCOORD0;
                    float4 color : COLOR0;
               };

               v2f vert (appdata v)
               {
                    v2f o;                    
                    o.pos.xy = v.texcoord1 * 2 - 1;
                    o.pos.z = 0.1;
                    o.pos.w = 1.0;
                    
                    o.color = 0.5;
                    // use vertex position in a dummy way, otherwise Unity will complain
                    o.dummy = v.vertex.x;
                    return o;
               }
               ENDCG
          }
     }
}
