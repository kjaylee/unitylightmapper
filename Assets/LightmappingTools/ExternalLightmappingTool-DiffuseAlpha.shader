Shader "ExternalLightmappingTool/LightmappedDiffuseAlpha" {
	Properties {
		_LightmapModifier ("Lightmap Modifier", Color) = (0.5,0.5,0.5,1)
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_LightMap ("Lightmap (RGB)", 2D) = "lightmap" { LightmapMode } 
	}
	SubShader {
		Tags {"Queue"="Transparent"}
		Blend One One
		ColorMask RGB
		
		Zwrite Off
		// Alpha mask pass
		Pass {
			Name "BASE"
			Blend Zero OneMinusSrcColor
			ColorMask RGBA
			Tags {"LightMode" = "Always"}
			
			SetTexture [_MainTex]
			{
				constantColor (0,0,0,0)
				combine constant, texture alpha
			}
		}
		
		// Ambient pass
		Pass {
			Name "BASE"
			Blend SrcAlpha OneMinusSrcAlpha
			Tags {"LightMode" = "PixelOrNone"}
			Color [_PPLAmbient]
			BindChannels {
				Bind "Vertex", vertex
				Bind "normal", normal
				Bind "texcoord1", texcoord0 // lightmap uses 2nd uv
				Bind "texcoord", texcoord1 // main uses 1st uv
			}
			SetTexture [_LightMap] {
				constantColor [_LightmapModifier]
				combine texture * constant DOUBLE
			}
			SetTexture [_MainTex] {
				constantColor [_Color]
				combine texture * previous , texture * constant
			}
		}
	
		// Vertex lights
		Pass {
			Name "BASE"
			Blend SrcAlpha OneMinusSrcAlpha
			Tags {"LightMode" = "Vertex"}
			Material {
				Diffuse [_Color]
				Emission [_PPLAmbient]
			}

			Lighting On

			BindChannels {
				Bind "Vertex", vertex
				Bind "normal", normal
				Bind "texcoord1", texcoord0 // lightmap uses 2nd uv
				Bind "texcoord1", texcoord1 // lightmap uses 2nd uv
				Bind "texcoord", texcoord2 // main uses 1st uv
			}
			
			SetTexture [_LightMap] {
				constantColor [_LightmapModifier]
				combine texture * constant
			}
			SetTexture [_LightMap] {
				combine previous + primary
			}
			SetTexture [_MainTex] {
				combine texture * previous DOUBLE, texture * primary
			}
		}
		UsePass "Transparent/Diffuse/PPL"
	}
	//FallBack "ExternalLightmappingTool/LightmappedVertexLitAlpha", 1
}