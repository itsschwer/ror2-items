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
            equipmentDef = ScriptableObject.Instantiate(Addressables.LoadAssetAsync<EquipmentDef>("RoR2/DLC1/BossHunter/BossHunter.asset").WaitForCompletion());
            equipmentDef.name = $"{Plugin.Author}_{nameof(MendConsumed)}";
            equipmentDef.nameToken = "Replenisher";
            equipmentDef.pickupToken = "Restores broke, consumed, and empty items.";

            equipmentDef.descriptionToken = "";
            equipmentDef.loreToken = "";

            equipmentDef.pickupIconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Junk/SkullCounter/texBanditCoinIcon.png").WaitForCompletion(); // "RoR2/Base/Common/MiscIcons/texMysteryIcon.png"
            equipmentDef.pickupModelPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/LunarWings/PickupLunarWings.prefab").WaitForCompletion(); //"RoR2/Base/Mystery/PickupMystery.prefab"

            equipmentDef.equipmentIndex = EquipmentIndex.None;
            equipmentDef.requiredExpansion = null;
            equipmentDef.unlockableDef = null;

            equipmentDef.cooldown = 15;

            ItemAPI.Add(new CustomEquipment(equipmentDef, (ItemDisplayRule[])null));
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
            if (equipmentSlot.characterBody?.inventory && equipmentSlot.stock <= 1) {
                CharacterMasterNotificationQueue.SendTransformNotification(equipmentSlot.characterBody.master, equipmentSlot.characterBody.inventory.currentEquipmentIndex, DLC1Content.Equipment.BossHunterConsumed.equipmentIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                equipmentSlot.characterBody.inventory.SetEquipmentIndex(DLC1Content.Equipment.BossHunterConsumed.equipmentIndex);
            }

            Chat.SendBroadcastChat(new Chat.SimpleChatMessage {
                baseToken = nameof(MendConsumed)
            });
            return true;
        }
    }
}
