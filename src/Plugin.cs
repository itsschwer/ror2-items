using BepInEx;

namespace itsschwer.Items
{
    [R2API.Utils.NetworkCompatibility(R2API.Utils.CompatibilityLevel.EveryoneMustHaveMod, R2API.Utils.VersionStrictness.EveryoneNeedSameModVersion)]
    [BepInDependency(R2API.LanguageAPI.PluginGUID)]
    [BepInDependency(R2API.ItemAPI.PluginGUID)]
    [BepInPlugin(GUID, GUID, Version)]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string GUID = Author + ".Items";
        public const string Author = "itsschwer";
        public const string Name = "items_from_itsschwer"; // only to satisfy release workflow
        public const string Version = "1.1.0";

        internal static new BepInEx.Logging.ManualLogSource Logger { get; private set; }

        private void Awake()
        {
            // This plugin's Plugin.Name is the same as Plugin.GUID
            Logger = base.Logger;

            MendConsumed.Init();
            RoR2.RoR2Application.onLoad += MendConsumedTransformations.Init;

            Logger.LogMessage("~awake.");
        }
    }
}
