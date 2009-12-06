Shader "ExternalLightmappingTool/LightmappedVertexLitAlpha" {
Properties {
	_LightmapModifier ("Lightmap Modifier", Color) = (0.5,0.5,0.5,1)
	_Color ("Main Color", Color) = (1,1,1,1)
	_SpecColor ("Spec Color", Color) = (1,1,1,1)
	_Shininess ("Shininess", Range (0.01, 1)) = 0.7
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_LightMap ("Lightmap (RGB)", 2D) = "lightmap" { LightmapMode }
}

// ------------------------------------------------------------------
// Three texture cards (Radeons, GeForce3/4Ti and up)

SubShader {
	Tags {"Queue"="Transparent"}
	Fog { Color [_AddFog] }
	ColorMask RGB
	
	ZWrite Off
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
			Shininess [_Shininess]
			Specular [_SpecColor]
		}

		Lighting On
		SeparateSpecular On

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
}

// ------------------------------------------------------------------
// Dual texture cards - draw in two passes

SubShader {
	Tags {"Queue"="Transparent"}
	Fog { Color [_AddFog] }
	ColorMask RGB

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
	// Always drawn base pass: texture * lightmap
	Pass {
		Name "BASE"
		Tags {"LightMode" = "Always"}
		Color [_PPLAmbient]
		Blend SrcAlpha OneMinusSrcAlpha
		BindChannels {
			Bind "Vertex", vertex
			Bind "normal", normal
			Bind "texcoord1", texcoord0 // lightmap uses 2nd uv
			Bind "texcoord", texcoord1 // main uses 1st uv
		}
		SetTexture [_LightMap] {
			constantColor [_Color]
			combine texture * constant
		}
		SetTexture [_MainTex] {
			combine texture * previous, texture * primary
		}
	}
	
	// Vertex lights: add lighting on top of base pass
	Pass {
		Name "BASE"
		Tags {"LightMode" = "Vertex"}
		Blend SrcAlpha OneMinusSrcAlpha
		Material {
			Diffuse [_Color]
			Shininess [_Shininess]
			Specular [_SpecColor]
		}

		Lighting On
		SeparateSpecular On
		
		ColorMask RGB

		SetTexture [_MainTex] {
			combine texture * primary DOUBLE, texture
		}
	}
}

Fallback "Transparent/VertexLit", 1

}
