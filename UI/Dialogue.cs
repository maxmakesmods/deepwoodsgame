
using DeepWoods.Game;
using DeepWoods.Helpers;
using DeepWoods.Main;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace DeepWoods.UI
{
    public class Dialogue : UIPanel
    {
        private readonly string ping;
        private readonly List<string> pongs;

        public Dialogue(string ping, List<string> pongs)
        {
            this.ping = ping;
            this.pongs = pongs;
        }

        public void DrawUI(SpriteBatch spriteBatch)
        {
            PanelData panelData = DoThePanelThing(spriteBatch, 10, pongs.Count + 2, 0.1f);

            DeepWoodsMain.Instance.TextHelper.DrawStringOnScreen(spriteBatch, panelData.Position(), ping, Color.RoyalBlue);
            for (int i = 0; i < pongs.Count; i++)
            {
                DeepWoodsMain.Instance.TextHelper.DrawStringOnScreen(spriteBatch, panelData.MakeCell(0, i + 1).Position(), pongs[i], Color.SpringGreen);
            }
        }

    }
}
