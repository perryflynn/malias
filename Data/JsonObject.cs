using Newtonsoft.Json;

namespace maliasmgr.Data
{
    /// <summary>
    /// Helper class to easy serialize an DTO
    /// </summary>
    public abstract class JsonObject
    {
        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
