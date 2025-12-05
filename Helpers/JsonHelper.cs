
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeepWoods.Helpers
{
    public class JsonHelper
    {
        private static JsonSerializerOptions _options;
        private static JsonSerializerOptions _optionsWithEnumNames;

        public static JsonSerializerOptions Options
        {
            get
            {
                if (_options == null)
                {
                    _options = new()
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                }
                return _options;
            }
        }

        public static JsonSerializerOptions OptionsWithEnumNames
        {
            get
            {
                if (_optionsWithEnumNames == null)
                {
                    _optionsWithEnumNames = new()
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                    _optionsWithEnumNames.Converters.Add(new JsonStringEnumConverter());
                }
                return _optionsWithEnumNames;
            }
        }
    }
}
