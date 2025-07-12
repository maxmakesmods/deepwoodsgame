
using DeepWoods.Loaders;
using DeepWoods.Objects;
using DeepWoods.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DeepWoods.Players
{
    internal class Player
    {
        private static readonly float WalkSpeed = 2f;

        public Vector2 position;

        public Camera camera;

        public Player(GraphicsDevice graphicsDevice)
        {
            camera = new Camera(graphicsDevice);
        }



        public void Update(Terrain terrain, float timeDelta)
        {
            var oldPosition = position;

            if (Keyboard.GetState().IsKeyDown(Keys.W)) position.Y += timeDelta * WalkSpeed;
            if (Keyboard.GetState().IsKeyDown(Keys.S)) position.Y -= timeDelta * WalkSpeed;
            if (Keyboard.GetState().IsKeyDown(Keys.A)) position.X -= timeDelta * WalkSpeed;
            if (Keyboard.GetState().IsKeyDown(Keys.D)) position.X += timeDelta * WalkSpeed;

            int x = (int)position.X;
            int y = (int)position.Y;

            if (!terrain.tiles[x, y].isOpen)
            {
                position = oldPosition;
            }

            camera.Update(position, timeDelta);
        }


        public void Draw(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            var screenPos = graphicsDevice.Viewport.Project(Vector3.Zero, camera.Projection, camera.View, Matrix.CreateTranslation(position.X, position.Y, 0f));
            var screenPosOneTileOffset = graphicsDevice.Viewport.Project(Vector3.Zero, camera.Projection, camera.View, Matrix.CreateTranslation(position.X + 1f, position.Y, 0f));

            float width = screenPosOneTileOffset.X - screenPos.X;

            spriteBatch.Draw(TextureLoader.ObjectsTexture, new Rectangle((int)screenPos.X, (int)screenPos.Y, (int)width, (int)width), Color.White);
        }
    }
}
