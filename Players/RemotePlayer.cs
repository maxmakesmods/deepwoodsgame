using DeepWoods.Game;
using Microsoft.Xna.Framework;

namespace DeepWoods.Players
{
    public class RemotePlayer : Player
    {
        public RemotePlayer(DeepWoodsGame game, Vector2 startPos)
            : base(game, startPos)
        {

        }

        public override void Update(float timeDelta)
        {
            base.Update(timeDelta);
        }
    }
}
