using UnityEngine;

namespace Jamcat.Core
{
    public class Loader
    {
        public enum ResourceType        //이게 맞나?
        {
            Avatars,
            Items,
            Maps,
        }
        
        private const string AvatarResourceFilePath = "Prefabs/Avatars";
        private const string ItemResourceFilePath = "Prefabs/Items";
        private const string MapResourceFilePath = "Prefabs/Environments";
        
        public static T Load<T>(ResourceType resourceType, string name) where T : MonoBehaviour
        {
            var path = GetResourcePath(resourceType, name);
            var prefab = Resources.Load<GameObject>(path);

            if (prefab == null)
            {
                return null;
            }

            var instance = Object.Instantiate(prefab);
            var component = instance.GetComponent<T>();

            if (component != null) return component;
            
            Object.Destroy(instance); 
            return null;
        }
        
        public static T LoadPrefab<T>(ResourceType resourceType, string name) where T : MonoBehaviour
        {
            var path = GetResourcePath(resourceType, name);
            var prefab = Resources.Load<GameObject>(path);

            if (prefab == null)
            {
                return null;
            }
            
            var component = prefab.GetComponent<T>();
            return component;
        }
        
        public static T LoadResource<T>(ResourceType resourceType, string name) where T : UnityEngine.Object
        {
            var path = GetResourcePath(resourceType, name);
            var resource = Resources.Load<T>(path);

            if (resource == null)
            {
                Debug.LogWarning($"Failed to load {typeof(T).Name} at path: {path}");
                return null;
            }
    
            return resource;
        }
        
        private static string GetResourcePath(ResourceType resourceType, string name)
        {
            return resourceType switch
            {
                ResourceType.Avatars => $"{AvatarResourceFilePath}/{name}",
                ResourceType.Items => $"{ItemResourceFilePath}/{name}",
                ResourceType.Maps => $"{MapResourceFilePath}/{name}",
                _ => string.Empty
            };
        }
    }
}