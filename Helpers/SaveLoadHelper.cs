using DeepWoods.Game;
using DeepWoods.Objects;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text.Json;

namespace DeepWoods.Helpers
{
    public class SaveLoadHelper
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public class SaveData
        {
            public int Seed { get; set; }
            public int NumPlayers { get; set; }
            public int GridSize { get; set; }
            public List<Vector2> Players { get; set; }
            // TODO: Objects
            // TODO: Players
            // TODO: Game state
        }

        public static SaveLoadHelper Instance { get; } = new();

        private readonly string savePath;

        private SaveLoadHelper()
        {
            savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DeepWoods");
            Directory.CreateDirectory(savePath);
        }

        public void Save(string id, SaveData saveData)
        {
            string saveFile = Path.Combine(savePath, id + ".json");
            string json = JsonSerializer.Serialize(saveData, Options);
            File.WriteAllText(saveFile, json);
        }

        public SaveData Load(string id)
        {
            string saveFile = Path.Combine(savePath, id + ".json");
            string json = File.ReadAllText(saveFile);
            return JsonSerializer.Deserialize<SaveData>(json, Options);
        }
    }
}
