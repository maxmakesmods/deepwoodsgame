using DeepWoods.Graphics;
using DeepWoods.Helpers;
using DeepWoods.Loaders;
using DeepWoods.Main;
using DeepWoods.Objects;
using DeepWoods.Players;
using DeepWoods.UI;
using DeepWoods.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace DeepWoods.Game
{
    public class DeepWoodsGame
    {
        private readonly DeepWoodsMain parent;
        private readonly Random rng = new();


        public GameRenderer Renderer { get; private set; }
        public GameWorld World { get; private set; }
        public InGameClock Clock { get; private set; }
        public PlayerManager PlayerManager { get; private set; }
        public DialogueManager DialogueManager { get; private set; }

        private bool wasESCPressed = false;
        public bool isGamePaused = false;

        public DeepWoodsGame(DeepWoodsMain parent, SaveLoadHelper.SaveData saveData)
        {
            this.parent = parent;
            rng = new(saveData.Seed);

            Clock = new InGameClock();
            PlayerManager = new PlayerManager(this);
            DialogueManager = new DialogueManager();
            Renderer = new GameRenderer(this);

            int worldSeed = rng.Next();
            //int worldSeed = 382081431;
            World = new GameWorld(this, worldSeed, saveData.GridSize, saveData.GridSize);


            Clock.TimeScale = 60;
            Clock.SetTime(1, 10, 0);

            PlayerManager.SpawnLocalPlayer();


            // TODO
            GameState.IsMultiplayerGame = true;
        }

        public void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                if (!wasESCPressed)
                {
                    isGamePaused = !isGamePaused;
                    if (isGamePaused)
                    {
                        MouseState ms = DWMouse.GetState(PlayerManager.LocalPlayers[0]);
                        Mouse.SetPosition(ms.X, ms.Y);
                    }
                }
                wasESCPressed = true;
            }
            else
            {
                wasESCPressed = false;
            }

            if (parent.IsActive && !isGamePaused)
            {
                parent.IsMouseVisible = false;
                Mouse.SetPosition(parent.Window.ClientBounds.Width / 2, parent.Window.ClientBounds.Height / 2);
            }
            else
            {
                parent.IsMouseVisible = true;
            }

            double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;

            EffectLoader.SpriteEffect.Parameters["GlobalTime"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);

            PlayerManager.Update((float)deltaTime);
            Clock.Update(deltaTime);
        }

        public void Draw(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            World.Update(Clock.DayDelta, deltaTime);

            string debugstring = $"Seed: {World.Seed}," +
                $" Time: {Clock.Day:D2}:{Clock.Hour:D2}:{Clock.Minute:D2}," +
                $" FPS: {parent.FPS.FPS}, ms/f: {parent.FPS.SPF}";

            Renderer.Draw(debugstring, isGamePaused);
        }
    }
}
