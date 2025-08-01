
using DeepWoods.Graphics;
using DeepWoods.Objects;
using DeepWoods.Players;
using DeepWoods.UI;
using DeepWoods.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace DeepWoods.Game
{
    internal class AllTheThings
    {
        public GraphicsDeviceManager GraphicsDeviceManager { get; set; }
        public GraphicsDevice GraphicsDevice { get; set; }
        public GameWindow GameWindow { get; set; }
        public Terrain Terrain { get; set; }
        public LightManager LightManager { get; set; }
        public ObjectManager ObjectManager { get; set; }
        public InGameClock Clock { get; set; }
        public DWRenderer Renderer { get; set; }
        public PlayerManager PlayerManager { get; set; }
        public DialogueManager DialogueManager { get; set; }
        public FPSCounter FPS { get; set; }
        public ContentManager Content { get; set; }
        public TextHelper TextHelper { get; set; }
    }
}
