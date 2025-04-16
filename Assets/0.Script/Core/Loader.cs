using UnityEngine;

namespace Jamcat.Core
{
    public class Loader
    {
        public enum ResourceType        //이게 맞나?
        {
            Tackles,
            Avatars,
            Props,
            Materials,
            Houses,
            Maps,
            Fishes
        }
        
        private const string MasterDataFilePath = "";
        private const string TackleResourceFilePath = "Prefabs/Tackles";
        private const string AvatarResourceFilePath = "Prefabs/Avatars";
        private const string PropResourceFilePath = "Prefabs/Props";
        private const string FishResourceFilePath = "Prefabs/Fishes";
        private const string MapResourceFilePath = "Prefabs/Environments";
        private const string MaterialResourceFilePath = "Materials";
        
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
                ResourceType.Tackles => $"{TackleResourceFilePath}/{name}",
                ResourceType.Avatars => $"{AvatarResourceFilePath}/{name}",
                ResourceType.Props => $"{PropResourceFilePath}/{name}",
                ResourceType.Fishes => $"{FishResourceFilePath}/{name}",
                ResourceType.Maps => $"{MapResourceFilePath}/{name}",
                ResourceType.Materials => $"{MaterialResourceFilePath}/{name}",
                _ => string.Empty
            };
        }
    }
}