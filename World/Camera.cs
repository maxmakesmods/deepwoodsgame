using DeepWoods.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace DeepWoods.World
{
    public class Camera
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

        public Viewport Viewport { get; set; }

        public Rectangle ShadowRectangle { get; private set; }

        public Matrix View => Matrix.Invert(Matrix.CreateRotationX(MathHelper.ToRadians(angle)) * Matrix.CreateTranslation(position));
        public Matrix Projection => Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), Viewport.AspectRatio, NearPlane, FarPlane);

        public Matrix ShadowView => Matrix.Invert(Matrix.CreateTranslation(ShadowRectangle.CenterV3(10f)));
        public Matrix ShadowProjection => Matrix.CreateOrthographic(ShadowRectangle.Width, ShadowRectangle.Height, NearPlane, FarPlane);

        public Camera(MouseState mouseState)
        {
            lastMouseWheel = mouseState.ScrollWheelValue;
            position.Z = 16;
        }

        public void Update(Vector2 playerPos, Rectangle viewport, MouseState mouseState, bool isGamePaused)
        {
            Viewport = new(viewport);

            int mouseWheel = mouseState.ScrollWheelValue;
            int mouseWheelDelta = mouseWheel - lastMouseWheel;
            lastMouseWheel = mouseWheel;

            if (Math.Abs(mouseWheelDelta) >= 120)
            {
                mouseWheelDelta /= 120;
            }

            if (!isGamePaused)
            {
                if (mouseWheelDelta > 0)
                {
                    position.Z /= mouseWheelDelta * cameraZoomSpeed;
                }
                else if (mouseWheelDelta < 0)
                {
                    position.Z *= -mouseWheelDelta * cameraZoomSpeed;
                }
            }

            if (position.Z < MinimumCameraZ)
            {
                position.Z = MinimumCameraZ;
            }
            if (position.Z > MaximumCameraZ)
            {
                position.Z = MaximumCameraZ;
            }

            position.X = playerPos.X + 0.5f;
            position.Y = playerPos.Y + 0.5f - position.Z / 2;

            RecalculateShadowRectangle();
        }

        private void RecalculateShadowRectangle()
        {
            int margin = 2;

            Point topleft = GetTileAtScreenPos(new Point(Viewport.X, Viewport.Y));
            Point topright = GetTileAtScreenPos(new Point(Viewport.X + Viewport.Width, Viewport.Y));
            Point bottomleft = GetTileAtScreenPos(new Point(Viewport.X, Viewport.Y + Viewport.Height));

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

        public Vector2 GetScreenPosAtTile(Point tilePos)
        {
            var screenPos = Viewport.Project(new(tilePos.X, tilePos.Y, 0), Projection, View, Matrix.Identity);
            return screenPos.ToVector2();
        }
    }
}
