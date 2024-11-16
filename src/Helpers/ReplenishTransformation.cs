using RoR2;

namespace itsschwer.Items.Helpers
{
    public class ReplenishTransformation : IReplenishTransformation
    {
        private readonly ItemDef consumed;
        private readonly ItemDef original;
        public ItemDef Consumed => consumed;

        public ReplenishTransformation(ItemDef consumed, ItemDef original)
        {
            this.consumed = consumed;
            this.original = original;
        }

        public ItemDef GetTransformation(Inventory inventory)
        {
            return original;
        }
    }
}
