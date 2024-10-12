using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using R2API;
using RoR2;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace itsschwer.Items
{
    internal static class MendConsumed
    {
        public static readonly EquipmentDef equipmentDef;

        internal static void Init()
        {
            Plugin.Logger.LogDebug($"Added equipment {equipmentDef.name} ({Language.GetString(equipmentDef.nameToken)})");
        }

        static MendConsumed() {
            // Base on Trophy Hunter's Tricorn (even though most fields are being overwritten...)
            equipmentDef = ScriptableObject.Instantiate(Addressables.LoadAssetAsync<EquipmentDef>("RoR2/DLC1/BossHunter/BossHunter.asset").WaitForCompletion());
            equipmentDef.name = $"{Plugin.Author}_{nameof(MendConsumed)}";

            string token = $"{Plugin.Author.ToUpperInvariant()}_{nameof(MendConsumed).ToUpperInvariant()}";
            equipmentDef.nameToken = token + "_NAME";
            equipmentDef.pickupToken = token + "_PICKUP";
            equipmentDef.descriptionToken = token + "_DESC";
            equipmentDef.loreToken = "- no lore -"; // token + "_LORE";

            equipmentDef.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/DLC1/LunarWings/texLunarWingsIcon.png").WaitForCompletion();
            equipmentDef.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/LunarWings/PickupLunarWings.prefab").WaitForCompletion();

            equipmentDef.equipmentIndex = EquipmentIndex.None;
            equipmentDef.requiredExpansion = null;
            equipmentDef.unlockableDef = null;

            equipmentDef.cooldown = 15;

            ItemAPI.Add(new CustomEquipment(equipmentDef, (ItemDisplayRule[])null));
            LanguageAPI.Add(equipmentDef.nameToken, "Replenisher");
            LanguageAPI.Add(equipmentDef.pickupToken, "Restores broken, consumed, and empty items. Transforms on use.");
            LanguageAPI.Add(equipmentDef.descriptionToken, "<style=cIsUtility>Restores</style> broken, consumed, and empty items back into their <style=cIsDamage>original forms</style>. Does not affect items that can regenerate. Equipment <style=cIsUtility>transforms</style> into a <style=cIsDamage>random equipment</style> when depleted.");
            ApplyIL();
        }

        private static void ApplyIL() {
            ILHook hook = new ILHook(typeof(EquipmentSlot).GetMethod(nameof(EquipmentSlot.PerformEquipmentAction), BindingFlags.Instance | BindingFlags.NonPublic), (il) => {
                ILCursor c = new ILCursor(il);

                ILLabel funcInvoke = null;
                ILLabel nextElseIf = null;
                MethodReference ctorFunc = null;

                Func<Instruction, bool>[] elseIfEquipmentDef = {
                    x => x.MatchBr(out funcInvoke),
                    x => x.MatchLdarg(1),
                    x => x.MatchLdsfld(out _),
                    x => x.MatchCallOrCallvirt<UnityEngine.Object>("op_Equality"),
                    x => x.MatchBrfalse(out nextElseIf),

                    x => x.MatchLdarg(0),
                    x => x.MatchLdftn(out _),
                    x => x.MatchNewobj(out ctorFunc),
                    x => x.MatchStloc(0)
                };

                if (c.TryGotoNext(MoveType.After, elseIfEquipmentDef)) {
#if DEBUG
                    Plugin.Logger.LogWarning(c.ToString());
#endif
                    Instruction nextElseIfTarget = nextElseIf.Target;

                    try {
                        c.Emit(OpCodes.Br, funcInvoke.Target);
                        c.Emit(OpCodes.Ldarg_1);
                        nextElseIf.Target = c.Previous;
                        c.Emit(OpCodes.Ldsfld, typeof(MendConsumed).GetField(nameof(equipmentDef), BindingFlags.Static | BindingFlags.Public));
                        c.Emit(OpCodes.Call, typeof(UnityEngine.Object).GetMethod("op_Equality"));
                        c.Emit(OpCodes.Brfalse, nextElseIfTarget);
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Ldftn, typeof(MendConsumed).GetMethod(nameof(MendConsumed.Execute), BindingFlags.Static | BindingFlags.NonPublic));
                        c.Emit(OpCodes.Newobj, ctorFunc);
                        c.Emit(OpCodes.Stloc_0);
#if DEBUG
                        Plugin.Logger.LogDebug(il.ToString());
#endif
                    }
                    catch (Exception e) { Plugin.Logger.LogError(e); }
                }
                else Plugin.Logger.LogError($"{nameof(MendConsumed)}> Cannot hook: failed to match IL instructions.");
            });
        }

        private static bool Execute(this EquipmentSlot equipmentSlot) {
            Inventory inventory = equipmentSlot.characterBody?.inventory;
            if (!inventory) return false;

            // Transform into a random equipment when fully depleted
            if (equipmentSlot.stock <= 1) {
                EquipmentIndex randomEquipment = PickupCatalog.GetPickupDef(Run.instance.availableEquipmentDropList[UnityEngine.Random.Range(0, Run.instance.availableEquipmentDropList.Count)])?.equipmentIndex ?? EquipmentIndex.None;
                CharacterMasterNotificationQueue.SendTransformNotification(equipmentSlot.characterBody.master, equipmentSlot.characterBody.inventory.currentEquipmentIndex, randomEquipment, CharacterMasterNotificationQueue.TransformationType.Default);
                equipmentSlot.characterBody.inventory.SetEquipmentIndex(randomEquipment);
            }

            RestoreConsumedItems(inventory, equipmentSlot.characterBody.master);

            EffectData effectData = new EffectData { origin = equipmentSlot.characterBody.transform.position };
            effectData.SetNetworkedObjectReference(equipmentSlot.characterBody.gameObject);
            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/HealingPotionEffect"), effectData, transmit: true);
            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/DelicateWatchProcEffect"), effectData, transmit: true);
            // Probably should cache effects, but why does HealthComponent.AssetReferences have a field for Delicate Watch but not Power Elixir??

            return true;
        }

        private static void RestoreConsumedItems(Inventory inventory, CharacterMaster notificationTarget)
        {
            (ItemDef consumed, ItemDef original)[] items = {
                (RoR2Content.Items.ExtraLifeConsumed, RoR2Content.Items.ExtraLife),
                (DLC1Content.Items.ExtraLifeVoidConsumed, (inventory.GetItemCount(RoR2Content.Items.ExtraLifeConsumed) > 0 ? RoR2Content.Items.ExtraLife : DLC1Content.Items.ExtraLifeVoid)),
                (DLC1Content.Items.FragileDamageBonusConsumed, DLC1Content.Items.FragileDamageBonus),
                (DLC1Content.Items.HealingPotionConsumed, DLC1Content.Items.HealingPotion),
                (RoR2Content.Items.TonicAffliction, null)
            };

            for (int i = 0; i < items.Length; i++) {
                int count = inventory.GetItemCount(items[i].consumed);
                inventory.RemoveItem(items[i].consumed, count);
                if (items[i].original) {
                    inventory.GiveItem(items[i].original, count);
                    if (count > 0) CharacterMasterNotificationQueue.SendTransformNotification(notificationTarget, items[i].consumed.itemIndex, items[i].original.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                }
            }
        }
    }
}
