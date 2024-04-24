using HarmonyLib;

namespace SnowyHolidayDropship.Patches
{
    [HarmonyPatch]
    class SnowyHolidayDropshipPatches
    {
        [HarmonyPatch(typeof(ItemDropship), "Start")]
        [HarmonyPostfix]
        public static void ItemDropshipPostStart(ItemDropship __instance)
        {
            DropshipDecorator.Init(__instance);
        }

        [HarmonyPatch(typeof(ItemDropship), "LandShipClientRpc")]
        [HarmonyPostfix]
        public static void PostLandShipClientRpc()
        {
            DropshipDecorator.RedecorateDropship();
        }

        [HarmonyPatch(typeof(ItemDropship), "ShipLeave")]
        [HarmonyPostfix]
        public static void ItemDropshipPostShipLeave()
        {
            DropshipDecorator.DropshipLeave();
        }
    }
}