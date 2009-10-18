Shader "ExternalLightmappingTool/LightmappedBumpedDiffuse" {
	Properties {
		_LightmapModifier ("Lightmap Modifier", Color) = (0.5,0.5,0.5,1)
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BumpMap ("Bumpmap (RGB)", 2D) = "bump" {}
		_LightMap ("Lightmap (RGB)", 2D) = "lightmap" { LightmapMode } 
	}
	SubShader {
		UsePass "ExternalLightmappingTool/LightmappedVertexLit/BASE"
		UsePass "Bumped Diffuse/PPL"
	}
	FallBack "ExternalLightmappingTool/LightmappedVertexLit", 1
}