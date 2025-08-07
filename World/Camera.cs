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
        private static readonly float FarPlane = 10000f;
        private static readonly float MinimumCameraZ = 2f;
        private static readonly float MaximumCameraZ = 6400f;

        public Vector3 position;
        private float angle = 20f;
        private float fov = 45f;
        private float cameraZoomSpeed = 1.2f;
        private int lastMouseWheel = 0;
        private readonly GraphicsDevice graphicsDevice;

        private Viewport Viewport { get; set; }

        public Rectangle ShadowRectangle { get; private set; }

        public Matrix View => Matrix.Invert(Matrix.CreateRotationX(MathHelper.ToRadians(angle)) * Matrix.CreateTranslation(position));
        public Matrix Projection => Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), Viewport.AspectRatio, NearPlane, FarPlane);

        public Matrix ShadowView => Matrix.Invert(Matrix.CreateTranslation(ShadowRectangle.CenterV3(10f)));
        public Matrix ShadowProjection => Matrix.CreateOrthographic(ShadowRectangle.Width, ShadowRectangle.Height, NearPlane, FarPlane);

        public Camera(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            position.Z = 16;
        }

        public void Update(Vector2 playerPos, Rectangle viewport, MouseState mouseState, float timeDelta)
        {
            Viewport = new(viewport);

            position.X = playerPos.X + 0.5f;
            position.Y = playerPos.Y + 0.5f - position.Z / 2;

            int mouseWheel = mouseState.ScrollWheelValue;
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
            if (position.Z < MinimumCameraZ)
            {
                position.Z = MinimumCameraZ;
            }
            if (position.Z > MaximumCameraZ)
            {
                position.Z = MaximumCameraZ;
            }

            RecalculateShadowRectangle();
        }

        private void RecalculateShadowRectangle()
        {
            int margin = 2;

            Point topleft = GetTileAtScreenPos(new Point(0, 0));
            Point topright = GetTileAtScreenPos(new Point(Viewport.Width, 0));
            Point bottomleft = GetTileAtScreenPos(new Point(0, Viewport.Height));

            ShadowRectangle = new Rectangle(
                topleft.X - margin,
                bottomleft.Y - margin,
                topright.X - topleft.X + margin * 2,
                topleft.Y - bottomleft.Y + margin * 2);
        }

        public Point GetTileAtScreenPos(Point screenPos)
        {
            var worldPosNear = Viewport.Unproject(new(screenPos.X, screenPos.Y, NearPlane), Projection, View, Matrix.Identity);
            var worldPosFar = Viewport.Unproject(new(screenPos.X, screenPos.Y, FarPlane), Projection, View, Matrix.Identity);

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
