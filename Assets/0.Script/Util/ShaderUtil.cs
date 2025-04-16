using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jjamcat
{
	public class ShaderType
	{
		public static string DEFAULT_SHADER = "Unlit/Texture";
		public static string CLIPPING_SHADER = "Custom/ClippingGroup";
		public static string VERTEXKIT_SHADER = "Mobile/VertexLit";		
	}
	
	public class ShaderUtil : MonoBehaviour {

		public static Shader GetShader(string shaderPath) {
			return Shader.Find(shaderPath);
		}
	}	
}

