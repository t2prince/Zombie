using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Jjamcat.Util
{
    public static class ListHelper
    {
        public static void BuildInitList<T> (List<T> originList, int count) where T : MonoBehaviour
        {
            var addCount = count - originList.Count;
            var origin = originList.First();
            for (var i = 0; i < addCount; i++)
            {
                var newOne = GameObject.Instantiate(origin, origin.transform.parent);
                originList.Add(newOne);
            }

            foreach (var prob in originList)
            {
                prob.gameObject.SetActive(false);
            }
        }
    }
}