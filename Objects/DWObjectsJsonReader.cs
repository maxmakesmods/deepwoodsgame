
using DeepWoods.Helpers;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.Text.Json;

namespace DeepWoods.Objects
{
    internal class DWObjectsJsonReader : ContentTypeReader<List<DWObjectDefinition>>
    {
        protected override List<DWObjectDefinition> Read(ContentReader reader, List<DWObjectDefinition> existingInstance)
        {
            return JsonSerializer.Deserialize<List<DWObjectDefinition>>(reader.ReadString(), JsonHelper.Options);
        }
    }
}
