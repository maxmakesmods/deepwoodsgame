using DeepWoods.Game;
using Microsoft.Xna.Framework;

namespace DeepWoods.Players
{
    public class RemotePlayer : Player
    {
        public RemotePlayer(DeepWoodsGame game, PlayerId id, Vector2 startPos)
            : base(game, id, startPos)
        {

        }

        public override void Update(float timeDelta)
        {
            base.Update(timeDelta);
        }
    }
}
