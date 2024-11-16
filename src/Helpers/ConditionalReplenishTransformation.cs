using RoR2;
using System;

namespace itsschwer.Items.Helpers
{
    public class ConditionalReplenishTransformation : IReplenishTransformation
    {
        private readonly ItemDef consumed;
        private readonly ItemDef original;
        private readonly ItemDef conditional;
        private readonly Func<Inventory, bool> condition;
        public ItemDef Consumed => consumed;

        public ConditionalReplenishTransformation(ItemDef consumed, ItemDef original, ItemDef conditional, Func<Inventory, bool> condition)
        {
            this.consumed = consumed;
            this.original = original;
            this.conditional = conditional;
            this.condition = condition;
        }

        public ItemDef GetTransformation(Inventory inventory)
        {
            return condition(inventory) ? conditional : original;
        }
    }
}
