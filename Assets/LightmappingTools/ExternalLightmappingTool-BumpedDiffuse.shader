Shader "ExternalLightmappingTool/LightmappedBumpedDiffuse" {
	Properties {
		_LightmapModifier ("Lightmap Modifier", Color) = (0.5,0.5,0.5,1)
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BumpMap ("Bumpmap (RGB)", 2D) = "bump" {}
		_LightMap ("Lightmap (RGB)", 2D) = "lightmap" { LightmapMode } 
          _SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
          _Shininess ("Shininess", Range (0.03, 1)) = 0.078125
	}
	SubShader {
		UsePass "ExternalLightmappingTool/LightmappedDiffuse/BASE"
		UsePass "Bumped Diffuse/PPL"
	}
	FallBack "ExternalLightmappingTool/LightmappedVertexLit", 1
}