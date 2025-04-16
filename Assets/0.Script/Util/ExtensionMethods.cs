using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace Util
{
    public static class ExtensionMethods
    {
        private static Random rng = new Random();

        public static void AddOrAssign<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if (dict.ContainsKey(key))
                dict[key] = value;
            else
                dict.Add(key, value);
        }
        
        public static void AddOrPlus<TKey>(this Dictionary<TKey, int> dict, TKey key, int value)
        {
            if (dict.ContainsKey(key))
                dict[key] = dict[key] + value;
            else
                dict.Add(key, value);
        }

        public static void DecreaseSafe<TKey>(this Dictionary<TKey, int> dict, TKey key, int value)
        {
            if (dict.ContainsKey(key))
                dict[key] = dict[key] - value;            
        }

        
        public static void RemoveSafe<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            if (dict.ContainsKey(key))
                dict.Remove(key);
        }

        public static TValue GetSafe<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }
            return default(TValue);
        }


        public static T RandomElement<T>(this IList<T> list)
        {
            return list[rng.Next(list.Count)];
        }

        public static T RandomElement<T>(this T[] array)
        {
            return array[rng.Next(array.Length)];
        }

        public static float GetCurrentAnimationLength(this Animator anim) {
            anim.Update(Mathf.Epsilon);

            return anim.GetCurrentAnimatorStateInfo(0).length;
        }

        public static void AddCount<T>(this Dictionary<T, int> dic, T type, int count) {
            if (dic.ContainsKey(type)) {
                dic[type] += count;
            }
            else {
                dic.Add(type, count);
            }
        }
        
        public static T GetRandomEnumValue<T>() where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            var random = new Random();
            return (T)values.GetValue(random.Next(values.Length));
        }
    }           
}