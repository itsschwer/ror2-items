using RoR2;

namespace itsschwer.Items.Helpers
{
    public interface IReplenishTransformation
    {
        public ItemDef Consumed { get; }

        public ItemDef GetTransformation(Inventory inventory);
    }
}
