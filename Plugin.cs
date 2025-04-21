using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using BepInEx.Bootstrap;

namespace SnowyHolidayDropship
{
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInDependency(GUID_LOBBY_COMPATIBILITY, BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        internal const string PLUGIN_GUID = "butterystancakes.lethalcompany.snowyholidaydropship", PLUGIN_NAME = "Snowy Holiday Dropship", PLUGIN_VERSION = "1.1.4";
        internal static new ManualLogSource Logger;

        const string GUID_LOBBY_COMPATIBILITY = "BMX.LobbyCompatibility";

        public static ConfigEntry<float> configSnowyChance, configNormalChance, configLegacyChance;

        void Awake()
        {
            Logger = base.Logger;

            if (Chainloader.PluginInfos.ContainsKey(GUID_LOBBY_COMPATIBILITY))
            {
                Logger.LogInfo("CROSS-COMPATIBILITY - Lobby Compatibility detected");
                LobbyCompatibility.Init();
            }

            AcceptableValueRange<float> percentage = new(0f, 1f);
            string chanceHint = " (0 = never, 1 = guaranteed, or anything in between - 0.5 = 50% chance)";

            configSnowyChance = Config.Bind(
                "Random",
                "SnowyChance",
                1f,
                new ConfigDescription("The percentage chance for the dropship to be holiday-themed on snowy moons." + chanceHint, percentage));

            configNormalChance = Config.Bind(
                "Random",
                "NormalChance",
                0f,
                new ConfigDescription("The percentage chance for the dropship to be holiday-themed on normal moons." + chanceHint, percentage));

            configLegacyChance = Config.Bind(
                "Random",
                "LegacyChance",
                0f,
                new ConfigDescription("The percentage chance for the normal dropship to use the old music, showcased in an early teaser video." + chanceHint, percentage));

            new Harmony(PLUGIN_GUID).PatchAll();

            Logger.LogInfo($"{PLUGIN_NAME} v{PLUGIN_VERSION} loaded");
        }
    }
}