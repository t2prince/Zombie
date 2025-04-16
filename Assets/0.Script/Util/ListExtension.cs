using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public static class ListExtension {

	// http://stackoverflow.com/questions/273313/randomize-a-listt-in-c-sharp
	public static void Shuffle<T>(this IList<T> list, System.Random rnd)
	{
		for(var i=0; i < list.Count; i++)
			list.Swap(i, rnd.Next(i, list.Count));
	}
	
	public static void Shuffle<T>(this IList<T> list)
	{
		var rnd = new System.Random();
		list.Shuffle(rnd);
	}
	
	public static T GetRandomItem<T>(this IList<T> list)
	{
		var rndIdx = Random.Range(0,list.Count - 1);
		return list[rndIdx];
	}
	
	public static void Swap<T>(this IList<T> list, int i, int j)
	{
		T temp = list[i];
		list[i] = list[j];
		list[j] = temp;
	}

	public static List<T> SwapRef<T>(this List<T> list, int index1, int index2)
    {
    	if (index1 != index2 && index2 > -1 && index2 < list.Count) {
			T temp = list[index1];
	        list[index1] = list[index2];
	        list[index2] = temp;
    	}

		return list;
    }
}
