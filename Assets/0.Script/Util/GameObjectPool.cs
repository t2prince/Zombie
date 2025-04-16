using UnityEngine;
using System.Collections.Generic;

public class GameObjectPool 
{
	public delegate GameObject Func ();
	
	private readonly int _count;

	private readonly Func _createFunc;

	private readonly Queue<GameObject> _objects;

	public GameObjectPool (int count, Func fn)
	{  
		_count = count;  
		_createFunc = fn;  

		_objects = new Queue<GameObject> (_count);  
		Allocate ();  
	}

	private void Allocate ()
	{  
		for (var i = 0; i < _count; ++i)
		{
			var obj = _createFunc.Invoke();
			obj.gameObject.SetActive(false);
			_objects.Enqueue(obj);  
		}  
	}

	public GameObject Pop ()
	{  
		if (_objects.Count <= 0) {  
			Allocate ();  				
		}

		var obj = _objects.Dequeue();
		obj.gameObject.SetActive(true);
		return obj;  
	}

	public void Push (GameObject obj)
	{  			
		_objects.Enqueue(obj);
		obj.gameObject.SetActive(false);
	}
}

public class GameObjectPool<T> where T : MonoBehaviour
{
	public delegate T Func ();
	
	private readonly int _count;

	private readonly Func _createFunc;

	private readonly Queue<T> _objects;

	public GameObjectPool (int count, Func fn)
	{  
		_count = count;  
		_createFunc = fn;  

		_objects = new Queue<T> (_count);  
		Allocate ();  
	}

	private void Allocate ()
	{  
		for (var i = 0; i < _count; ++i)
		{
			var obj = _createFunc.Invoke();
			obj.gameObject.SetActive(false);
			_objects.Enqueue(obj);  
		}  
	}

	public T Pop ()
	{  
		if (_objects.Count <= 0) {  
			Allocate ();  				
		}

		var obj = _objects.Dequeue();
		obj.gameObject.SetActive(true);
		return obj;  
	}

	public void Push (T obj)
	{  			
		_objects.Enqueue(obj);
		obj.gameObject.SetActive(false);
	}

	public void Reset()
	{
		foreach (var obj in _objects)
		{
			GameObject.Destroy(obj);
		}
		_objects.Clear();
		Allocate();
	}
}
