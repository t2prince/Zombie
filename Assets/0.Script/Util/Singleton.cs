using UnityEngine;

namespace Jjamcat.Util
{

	public class SingletonGameObject<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static WeakReference<T> _instance;
		private static bool quit = false;

		void OnApplicationQuit ()
		{
			quit = true;
		}

		public static bool HasInstance ()
		{
			if (_instance == null || _instance.Target == null) {
				return false;
			}
			
			return true;
		}

		public static void ResetInstance()
		{
			UnloadInstance();
			SetInstance();
		}

		public static void UnloadInstance()
		{
			_instance.Target = null;
		}

		public static T Instance {
			get {
				if (quit) {
					return null;
				}
				
				if (_instance != null && _instance.Target != null) {
					return _instance.Target;
				}

				SetInstance();
				
				return _instance.Target;
			}
		}

		private static void SetInstance()
		{
			T instance = FindAnyObjectByType<T> ();

			if (instance == null) {
				GameObject container = new GameObject ();
				container.name = "_" + typeof(T).Name;
				instance = container.AddComponent (typeof(T)) as T;	
			} 

			_instance = new WeakReference<T> (instance);
		}
	}

	/// <summary>
	/// Be aware this will not prevent a non singleton constructor
	///   such as `T myT = new T();`
	/// To prevent that, add `protected T () {}` to your singleton class.
	/// 
	/// As a note, this is made as MonoBehaviour because we need Coroutines.
	/// </summary>
	public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
	{
		private static T _instance;
		
		private static object _lock = new ();

		public static T Instance {
			get {
				if (applicationIsQuitting) {
					return null;
				}

				lock (_lock) {
					if (_instance == null) {
						_instance = FindAnyObjectByType<T>();

						if (FindObjectsByType<T>(FindObjectsSortMode.InstanceID).Length > 1) {
							return _instance;
						}

						if (_instance == null) {
							var singleton = new GameObject($"_{typeof(T)}");
							_instance = singleton.AddComponent<T>();
						}

						DontDestroyOnLoad(_instance);
					}

					return _instance;
				}
			}
		}

		private static bool applicationIsQuitting = false;

		/// <summary>
		/// When Unity quits, it destroys objects in a random order.
		/// In principle, a Singleton is only destroyed when application quits.
		/// If any script calls Instance after it have been destroyed, 
		///   it will create a buggy ghost object that will stay on the Editor scene
		///   even after stopping playing the Application. Really bad!
		/// So, this was made to be sure we're not creating that buggy ghost object.
		/// </summary>
		public virtual void OnDestroy ()
		{
			applicationIsQuitting = true;
		}

		public static bool HasInstance ()
		{
			return (bool)_instance;
		}
	}

}
