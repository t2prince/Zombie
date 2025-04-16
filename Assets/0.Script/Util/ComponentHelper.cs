using UnityEngine;

namespace Util
{
    public static class ComponentHelper
    {
        public static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            var type = original.GetType();
            var copy = destination.AddComponent(type);
            var fields = type.GetFields();
            foreach (var field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy as T;
        }
        
        public static void CopyComponent<T>(this T destination, T original) where T : Component
        {
            var type = original.GetType();
            var fields = type.GetFields();
            foreach (var field in fields)
            {
                field.SetValue(destination, field.GetValue(original));
            }
        }
    }

    public static class MonoBehaviourHelper
    {
        public static bool IsFastNull(this MonoBehaviour target)
        {
            return ReferenceEquals(target, null);
        }
        
        public static bool IsFastNull(this Component target)
        {
            return ReferenceEquals(target, null);
        }
        
        public static bool IsFastNull(this GameObject target)
        {
            return ReferenceEquals(target, null);
        }
    }
}