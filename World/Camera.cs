using DeepWoods.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace DeepWoods.World
{
    internal class Camera
    {
        private static readonly float NearPlane = 1f;
        private static readonly float FarPlane = 1000f;

        public Vector3 position;
        private float angle = 20f;
        private float fov = 45f;
        private float cameraSpeed = 0.5f;
        private float cameraZoomSpeed = 1.2f;
        private int lastMouseWheel = 0;
        private readonly GraphicsDevice graphicsDevice;

        public Rectangle ShadowRectangle { get; private set; }

        public Matrix View => Matrix.Invert(Matrix.CreateRotationX(MathHelper.ToRadians(angle)) * Matrix.CreateTranslation(position));
        public Matrix Projection => Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), graphicsDevice.Viewport.AspectRatio, NearPlane, FarPlane);

        public Matrix ShadowView => Matrix.Invert(Matrix.CreateTranslation(ShadowRectangle.CenterV3(10f)));
        public Matrix ShadowProjection => Matrix.CreateOrthographic(ShadowRectangle.Width, ShadowRectangle.Height, NearPlane, FarPlane);

        public Camera(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        public void Update(float timeDelta)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.W)) position.Y += timeDelta * cameraSpeed * position.Z;
            if (Keyboard.GetState().IsKeyDown(Keys.S)) position.Y -= timeDelta * cameraSpeed * position.Z;
            if (Keyboard.GetState().IsKeyDown(Keys.A)) position.X -= timeDelta * cameraSpeed * position.Z;
            if (Keyboard.GetState().IsKeyDown(Keys.D)) position.X += timeDelta * cameraSpeed * position.Z;

            int mouseWheel = Mouse.GetState().ScrollWheelValue;
            int mouseWheelDelta = mouseWheel - lastMouseWheel;
            lastMouseWheel = mouseWheel;

            if (System.Math.Abs(mouseWheelDelta) >= 120)
            {
                mouseWheelDelta /= 120;
            }

            if (mouseWheelDelta > 0)
            {
                position.Z /= mouseWheelDelta * cameraZoomSpeed;
            }
            else if (mouseWheelDelta < 0)
            {
                position.Z *= -mouseWheelDelta * cameraZoomSpeed;
            }
            if (position.Z < 1)
            {
                position.Z = 1;
            }

            RecalculateShadowRectangle();
        }

        private void RecalculateShadowRectangle()
        {
            Point topleft = GetTileAtScreenPos(new Point(0, 0));
            Point topright = GetTileAtScreenPos(new Point(graphicsDevice.Viewport.Width, 0));
            Point bottomleft = GetTileAtScreenPos(new Point(0, graphicsDevice.Viewport.Height));
            ShadowRectangle = new Rectangle(topleft.X, bottomleft.Y, topright.X - topleft.X, topleft.Y - bottomleft.Y);
        }

        public Point GetTileAtScreenPos(Point screenPos)
        {
            var worldPosNear = graphicsDevice.Viewport.Unproject(new(screenPos.X, screenPos.Y, NearPlane), Projection, View, Matrix.Identity);
            var worldPosFar = graphicsDevice.Viewport.Unproject(new(screenPos.X, screenPos.Y, FarPlane), Projection, View, Matrix.Identity);

            var direction = worldPosFar - worldPosNear;
            direction.Normalize();

            var groundNormal = new Vector3(0, 0, 1);

            float dot = Vector3.Dot(direction, groundNormal);
            float distance = -Vector3.Dot(groundNormal, worldPosNear) / dot;
            var worldPosGround = worldPosNear + direction * distance;

            return new((int)Math.Floor(worldPosGround.X), (int)Math.Floor(worldPosGround.Y));
        }
    }
}
