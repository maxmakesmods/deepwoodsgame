
using System;
using System.Text.Json;

namespace DeepWoods.Network.Data
{
    internal class Message
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public MessageType Type {  get; set; }
        public string Json { get; set; }

        public static Message Create<T>(MessageType type, T data)
        {
            return new()
            {
                Type = type,
                Json = JsonSerializer.Serialize<T>(data, Options)
            };
        }

        public T GetPayload<T>()
        {
            return JsonSerializer.Deserialize<T>(Json, Options);
        }
    }
}
