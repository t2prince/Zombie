using UnityEngine;
using UnityEngine.Networking;
using Jamcat.Script.Managers.Auth;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jamcat.Script.Network
{
    public class Request
    {
        public enum ReqType
        {
            Get,
            Post,
        }

        public string userId => AuthManager.userId;
        public string version => Application.version;
        
        public string platform => AuthManager.platform;

        protected virtual ReqType RequestType => ReqType.Get;
        public virtual string Uri => string.Empty;

        public UnityWebRequest GetRequest(string uri)
        {
            return RequestType == ReqType.Get ? CreateGetRequest(uri) : CreatePostRequest(uri);
        }
        
        private UnityWebRequest CreateGetRequest(string uri)
        {
            var request = UnityWebRequest.Get($"{uri}?{GetParameters()}");
            return request;

        }
        private UnityWebRequest CreatePostRequest(string uri)
        {
            var request = UnityWebRequest.Post(uri, GetPostForm());
            if (request != null) SetHeader(ref request);

            return request;
        }

        private static void SetHeader(ref UnityWebRequest request)
        {
            request.SetRequestHeader("Access-Control-Allow-Credentials", "true");
            request.SetRequestHeader("Access-Control-Allow-Headers", "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
            request.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            request.SetRequestHeader("Access-Control-Allow-Origin", "*");
        }
        private WWWForm GetPostForm()
        {
            var form = new WWWForm();
            var request = JsonConvert.SerializeObject(this);
            var jsonObject = JObject.Parse(request);

            foreach (var property in jsonObject.Properties())
            {
                form.AddField(property.Name, property.Value.ToString());
            }

            return form;
        }

        protected virtual string GetParameters()
        {
            return $"{nameof(userId)}={userId}&{nameof(platform)}={platform}&{nameof(version)}={version}";
        }
    }
}