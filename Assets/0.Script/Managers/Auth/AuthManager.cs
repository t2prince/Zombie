using UnityEngine;

namespace Jamcat.Script.Managers.Auth
{
    public static class AuthManager
    {
        public static string username { get; private set; }
        public static string userId { get; private set; }
        
        public static string platform { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            
        }
         
    }
}