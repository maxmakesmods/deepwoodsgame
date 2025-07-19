using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DeepWoods.UI
{
    internal class DWKeyboard
    {
        public static KeyboardState GetState(PlayerIndex playerIndex)
        {
            return RawInput.GetKeyboardState(playerIndex);
        }
    }
}
