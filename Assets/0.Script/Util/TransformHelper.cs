
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace _Scripts.Util
{
    
    
    

	public static class TransformHelper
	{
		public static void ResetTransformLocal(this Transform trans)
		{
			trans.localPosition = Vector3.zero;
			trans.localRotation = Quaternion.identity;
			trans.localScale = Vector3.one;
		}

		public static void SetLocalPositionX(this Transform trans, float x)
		{
			Vector3 vec = trans.localPosition;
			vec.x = x;
			trans.localPosition = vec;
		}

		public static void SetLocalPositionY(this Transform trans, float y)
		{
			Vector3 vec = trans.localPosition;
			vec.y = y;
			trans.localPosition = vec;
		}

		public static void SetLocalPositionZ(this Transform trans, float z)
		{
			Vector3 vec = trans.localPosition;
			vec.z = z;
			trans.localPosition = vec;
		}

		public static void AddLocalPositionX(this Transform trans, float x)
		{
			Vector3 vec = trans.localPosition;
			vec.x += x;
			trans.localPosition = vec;
		}

		public static void AddLocalPositionY(this Transform trans, float y)
		{
			Vector3 vec = trans.localPosition;
			vec.y += y;
			trans.localPosition = vec;
		}

		public static void AddLocalPositionZ(this Transform trans, float z)
		{
			Vector3 vec = trans.localPosition;
			vec.z += z;
			trans.localPosition = vec;
		}

		public static void SetWorldPositionX(this Transform trans, float x)
		{
			Vector3 vec = trans.position;
			vec.x = x;
			trans.position = vec;
		}
		
		public static void SetWorldPositionY(this Transform trans, float y)
		{
			Vector3 vec = trans.position;
			vec.y = y;
			trans.position = vec;
		}
		
		public static void SetWorldPositionZ(this Transform trans, float z)
		{
			Vector3 vec = trans.position;
			vec.z = z;
			trans.position = vec;
		}

		public static void DestroyChildren(this GameObject go)
		{
			
			List<GameObject> children = new List<GameObject>();
			foreach (Transform tran in go.transform)
			{      
				children.Add(tran.gameObject); 
			}
			children.ForEach(child => GameObject.Destroy(child));  
		}

		public static void SetLayer(this Transform trans, string layerName)
		{
			if (trans == null)
				return;
			
			int layer = LayerMask.NameToLayer(layerName);
			trans.SetLayer(layer, false);
		}

		public static void SetLayer(this Transform trans, string layerName, bool recursive)
		{
			if (trans == null)
				return;

			int layer = LayerMask.NameToLayer(layerName);
			trans.SetLayer(layer, recursive);
		}

		public static void SetLayer(this Transform trans, int layer)
		{
			if (trans == null)
				return;

			trans.SetLayer(layer, false);
		}
		
		public static void SetLayer(this Transform trans, int layer, bool recursive)
		{
			if (trans == null)
				return;
			
			trans.gameObject.layer = layer;
			if (recursive)
			{
				for (int i = 0; i < trans.childCount; i++)
				{
					trans.GetChild(i).SetLayer(layer, true);
				}
			}
		}

		public static Transform FindRecursively(this Transform target, string name)
		{
			if (target == null)
				return null;

			if (target.name == name)
				return target;
			
			for (int i = 0; i < target.childCount; ++i) {
				var result = target.GetChild(i).FindRecursively(name);
				
				if (result != null)
					return result;
			}
			
			return null;
		}
	}

}