using Newtonsoft.Json;

namespace Jamcat.Script.Network
{
    public class Response
    {
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}