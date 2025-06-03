
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DeepWoods
{
    internal class Camera
    {
        private Vector3 position;
        private float angle = 20f;
        private float aspectRatio = 16f / 9f;
        private float fov = 45f;
        private float cameraSpeed = 0.5f;
        private float cameraZoomSpeed = 10f;
        private int lastMouseWheel = 0;

        public Matrix View => Matrix.Invert(Matrix.CreateRotationX(MathHelper.ToRadians(angle)) * Matrix.CreateTranslation(position));
        public Matrix Projection => Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), aspectRatio, 1f, 1000f);

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

            position.Z += mouseWheelDelta * cameraZoomSpeed;
            if (position.Z < 1)
            {
                position.Z = 1;
            }

            // TODO: Update fov and aspectRatio
        }
    }
}
