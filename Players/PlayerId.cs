using System;

namespace DeepWoods.Players
{
    public readonly struct PlayerId
    {
        public static readonly PlayerId HostId = new(Guid.Empty);

        public readonly Guid id;

        public PlayerId()
        {
            id = Guid.NewGuid();
        }

        public PlayerId(Guid id)
        {
            this.id = id;
        }
    }
}
