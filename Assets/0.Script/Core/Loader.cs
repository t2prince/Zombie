using System;
using System.IO;
using Jamcat.Managers.Data;
using Rpg.Sys.Secure;
using UnityEngine;
using Object = UnityEngine.Object;

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
        const string MasterDataRoot = "MasterData/";
        
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
        
        public static T LoadMasterData<T>(string name = null) where T : ScriptableObject
        {
            return (T) Resources.Load(Path.Combine(MasterDataRoot, name ?? typeof(T).Name));
        } 
        
        public static void SaveFile(string fileName, string fileString, bool isServerUpdate = false, Action<bool> callback = null)
        {
            try
            {
                var persistentFilePath = Path.Combine(Application.persistentDataPath, $"{fileName}.json");

                var cipheredTable = fileString.ToCipheredTable();
                File.WriteAllBytes(persistentFilePath,cipheredTable);
                callback?.Invoke(true);
            }
            catch (Exception e)
            {
                Debug.LogError($"{fileName}\n"+ e.Message);
                callback?.Invoke(false);
            }
        }

        public static string LoadFile(string fileName)
        {
            try
            {
                var path = Path.Combine(Application.persistentDataPath, $"{fileName}.json");
                return FileToString(path);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{fileName} Load Fail\n{e.Message}" );
                return null;
            }
        }
        
        private static string FileToString(string path)
        {
            var fileBytes = File.ReadAllBytes(path);
            try
            {
                var text = fileBytes.ToPlainTable();
                return text;
            }
            catch
            {
                Debug.LogError($"Table Load Fail : {path}");
                return string.Empty;					
            }
        }
    }
}