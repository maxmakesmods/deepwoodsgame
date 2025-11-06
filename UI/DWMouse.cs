using DeepWoods.Game;
using DeepWoods.Main;
using DeepWoods.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace DeepWoods.UI
{
    internal static class DWMouse
    {
        public static MouseState GetState(LocalPlayer player)
        {
            if (OperatingSystem.IsWindows() && DeepWoodsMain.Instance.IsLocalCoop)
            {
                return RawInput.GetMouseState(player.PlayerIndex, player.PlayerViewport);
            }
            else if (player.PlayerIndex == PlayerIndex.One)
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
