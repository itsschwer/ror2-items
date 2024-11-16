using itsschwer.Items.Helpers;
using RoR2;
using System.Collections.Generic;
using BaseItems = RoR2.RoR2Content.Items;
using DLC1Items = RoR2.DLC1Content.Items;
using DLC2Items = RoR2.DLC2Content.Items;

namespace itsschwer.Items
{
    public static class MendConsumedTransformations
    {
        private static List<IReplenishTransformation> transformations = new List<IReplenishTransformation>();

        internal static void Init()
        {
            List<IReplenishTransformation> _transformations = [
                new ReplenishTransformation(BaseItems.ExtraLifeConsumed, BaseItems.ExtraLife),
                new ConditionalReplenishTransformation(DLC1Items.ExtraLifeVoidConsumed, DLC1Items.ExtraLifeVoid, BaseItems.ExtraLife, (inventory) => inventory.GetItemCount(BaseItems.ExtraLifeConsumed) > 0),
                new ReplenishTransformation(DLC1Items.FragileDamageBonusConsumed, DLC1Items.FragileDamageBonus),
                new ReplenishTransformation(DLC1Items.HealingPotionConsumed, DLC1Items.HealingPotion),
                new ReplenishTransformation(BaseItems.TonicAffliction, null)
            ];

            // Mods that register additional transformations with the same consumed items won't be removed!
            _transformations.AddRange(transformations);
            transformations = _transformations;
        }

        public static (ItemDef consumed, ItemDef original)[] GetTransformations(Inventory inventory)
        {
            (ItemDef consumed, ItemDef original)[] result = new (ItemDef consumed, ItemDef original)[transformations.Count];
            for (int i = 0; i < transformations.Count; i++)
                result[i] = (transformations[i].Consumed, transformations[i].GetTransformation(inventory));
            return result;
        }

        public static bool Register(IReplenishTransformation transformation)
        {
            if (transformation == null || transformation.Consumed == null) return false;

            for (int i = 0; i < transformations.Count; i++) {
                if (transformations[i].Consumed == transformation.Consumed) {
                    Plugin.Logger.LogWarning($"{transformation.Consumed.nameToken} already has an {nameof(IReplenishTransformation)}, skipping transformation from {GetExecutingMethod()}");
                    return false;
                }
            }

            transformations.Add(transformation);
            return true;
        }

        private static string GetExecutingMethod(int index = 0)
        {
            // +2 ∵ this method + method to check
            var caller = new System.Diagnostics.StackTrace().GetFrame(index + 3).GetMethod();
            return $"{caller.DeclaringType}::{caller.Name}";
        }
    }
}
