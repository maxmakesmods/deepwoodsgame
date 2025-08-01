
using DeepWoods.Game;
using DeepWoods.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace DeepWoods.UI
{
    internal class Dialogue : UIPanel
    {
        private readonly string ping;
        private readonly List<string> pongs;

        public Dialogue(string ping, List<string> pongs)
        {
            this.ping = ping;
            this.pongs = pongs;
        }

        public void DrawUI(AllTheThings att, SpriteBatch spriteBatch)
        {
            PanelData panelData = DoThePanelThing(att.GraphicsDevice, spriteBatch, 10, pongs.Count + 2, 0.1f);

            att.TextHelper.DrawStringOnScreen(spriteBatch, panelData.Position(), ping, Color.RoyalBlue);
            for (int i = 0; i < pongs.Count; i++)
            {
                att.TextHelper.DrawStringOnScreen(spriteBatch, panelData.MakeCell(0, i + 1).Position(), pongs[i], Color.SpringGreen);
            }
        }

    }
}
