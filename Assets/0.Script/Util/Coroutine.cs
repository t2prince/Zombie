using UnityEngine;
using System.Collections;
using Jjamcat.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Util
{
	internal class Coroutine : SingletonGameObject<Coroutine>
	{
		private static int _randCount;

		private class FloatComparer : IEqualityComparer<float>
		{
			
			bool IEqualityComparer<float>.Equals (float x, float y)
			{
				return x == y;
			}
			int IEqualityComparer<float>.GetHashCode (float obj)
			{
				return obj.GetHashCode();
			}
		}

		private static readonly Dictionary<float, WaitForSeconds> _timeInterval = new Dictionary<float, WaitForSeconds>(new FloatComparer());

		public static WaitForSeconds WaitForSeconds(float seconds)
		{
			WaitForSeconds wfs;
			if (!_timeInterval.TryGetValue(seconds, out wfs))
				_timeInterval.Add(seconds, wfs = new WaitForSeconds(seconds));
			return wfs;
			
		}

		public static void DelayedAction (Action action, float time)
		{
			Instance.StartCoroutine (Delay (action, time));
		}
		
		public static void DelayedActionWithObj (Action<object> action, object obj, float time)
		{
			Instance.StartCoroutine (DelayWithObj (action, obj, time));
		}

		private static IEnumerator Delay (Action action, float time)
		{
			yield return WaitForSeconds(time);

			action ();
		}
		
		private static IEnumerator DelayWithObj (Action<object> action, object obj, float time)
		{
			yield return WaitForSeconds (time);

			action (obj);
		}		

		public static UnityEngine.Coroutine Run (IEnumerator iterationResult)
		{
			return Instance.StartCoroutine (iterationResult);
		}

		public static void Run (IEnumerator iterationResult, Action action)
		{
			Instance.StartCoroutine(RunWithAction(iterationResult, action));
		}

		private static IEnumerator RunWithAction (IEnumerator iterationResult, Action action)
		{
			yield return Instance.StartCoroutine (iterationResult);
			action?.Invoke();
		}

		public static void Stop(IEnumerator iterationResult)
		{
			Instance.StopCoroutine(iterationResult);
		}

		public static void Stop (UnityEngine.Coroutine coroutine)
		{
			if (HasInstance ())
				Instance.StopCoroutine (coroutine);
		}
		
		
		
		public static T GetNewObjectComponent<T> () where T : Component
		{
			var container = new GameObject ();
			container.name = "_" + typeof(T).Name;
			var component = container.AddComponent (typeof(T)) as T;	
			return component;
		}
		
		public static T GetObjectFromPlayerPrefs<T>(string key)
		{
			string base64str = PlayerPrefs.GetString(key);
			T t = default(T);
			if (string.IsNullOrEmpty((base64str)))
				return t;

			try {
				IFormatter formatter = new BinaryFormatter();
				using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(base64str))) {
					t = (T)formatter.Deserialize(stream);
				}
			} catch (Exception e) {
				Debug.LogWarning(e);
			}

			return t;
		}
		
		public static string[] GetSavedRequestorResultFiles()
		{
			string path = GetDeviceRelativePathForRequestor();
			return Directory.GetFiles(path, "*.txt");
		}
		
		public static string GetDeviceRelativePathForRequestor()
		{
			string path = Path.Combine(GetDeviceRelativePath(), "GCache");

			if (!Directory.Exists(path)) {
				Directory.CreateDirectory(path);
			}
			return path;
		}

		public static string GetDeviceRelativePath(bool useFileSchema = false)
		{
			string path = Application.persistentDataPath;
#if UNITY_EDITOR
			path = Environment.CurrentDirectory.Replace("\\", Path.DirectorySeparatorChar.ToString ());
#endif

			if (useFileSchema)
				path = "file://";

			return path;
		}
		
		public static void ChangeLayersRecursively(Transform trans, string name)
		{
			trans.gameObject.layer = LayerMask.NameToLayer(name);
			foreach (Transform child in trans)
			{
				ChangeLayersRecursively(child, name);
			}
		}
	}

	internal class SingletonUtil : SingletonGameObject<SingletonUtil>
	{
		public static T GetSingletonComponent<T> () where T : Component
		{
			T instance = FindAnyObjectByType<T> ();

			if (instance == null) {
				GameObject container = new GameObject ();
				container.name = "_" + typeof(T).Name;
				instance = container.AddComponent (typeof(T)) as T;	
			} 

			return instance;
		}
	}
}
