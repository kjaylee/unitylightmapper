Shader "ExternalLightmappingTool/LightmappedDiffuse" {
	Properties {
		_LightmapModifier ("Lightmap Modifier", Color) = (0.5,0.5,0.5,1)
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_LightMap ("Lightmap (RGB)", 2D) = "lightmap" { LightmapMode } 
	}
	SubShader {
		UsePass "ExternalLightmappingTool/LightmappedVertexLit/BASE"
		UsePass "Diffuse/PPL"
	}
	FallBack "ExternalLightmappingTool/LightmappedVertexLit", 1
}