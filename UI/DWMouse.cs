using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DeepWoods.UI
{
    internal static class DWMouse
    {
        public static MouseState GetState(PlayerIndex playerIndex)
        {
            return RawInput.GetMouseState(playerIndex);
        }
    }
}
