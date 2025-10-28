using DeepWoods.Game;
using DeepWoods.Players;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace DeepWoods.UI
{
    public class DialogueManager
    {
        private Dictionary<Player, Dialogue> openDialogues = new();

        public void OpenDialogue(Player player, Dialogue dialogue)
        {
            openDialogues[player] = dialogue;
        }

        public void CloseDialogue(Player player)
        {
            openDialogues.Remove(player);
        }

        public void DrawUI(SpriteBatch spriteBatch, Player player)
        {
            if (openDialogues.TryGetValue(player, out Dialogue dialogue))
            {
                dialogue.DrawUI(spriteBatch);
            }
        }

        internal bool HasOpenDialogue(Player player)
        {
            return openDialogues.ContainsKey(player);
        }
    }
}
