
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace DeepWoods.Helpers
{
    internal static class DataHelper
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public static byte[] ToBytes<T>(this T o)
        {
            try
            {
                string json = JsonSerializer.Serialize(o, Options);
                Debug.WriteLine($"ToBytes.json: {json}");
                return Encoding.UTF8.GetBytes(json);
            }
            catch (System.Exception e)
            {
                Debug.WriteLine(e);
                // TODO: log error
                return null;
            }
        }

        public static T FromBytes<T>(this byte[] data, int dataOffset, int dataSize)
        {
            try
            {
                string json = Encoding.UTF8.GetString(data, dataOffset, dataSize);
                Debug.WriteLine($"FromBytes.json: {json}");
                T result = JsonSerializer.Deserialize<T>(json, Options);
                return result;
            }
            catch (System.Exception e)
            {
                Debug.WriteLine(e);
                // TODO: log error
                return default;
            }
        }
    }
}
