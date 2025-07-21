using DeepWoods.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace DeepWoods.UI
{
    internal static class DWMouse
    {
        public static MouseState GetState(PlayerIndex playerIndex)
        {
            if (OperatingSystem.IsWindows() && GameState.IsMultiplayerGame)
            {
                return RawInput.GetMouseState(playerIndex);
            }
            else if (playerIndex == PlayerIndex.One)
            {
                return Mouse.GetState();
            }
            else
            {
                return default;
            }
        }
    }
}
