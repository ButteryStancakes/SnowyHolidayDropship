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

        [HarmonyPatch(typeof(ItemDropship), nameof(ItemDropship.LandShipClientRpc))]
        [HarmonyPostfix]
        public static void PostLandShipClientRpc()
        {
            DropshipDecorator.RedecorateDropship();
        }

        [HarmonyPatch(typeof(ItemDropship), nameof(ItemDropship.ShipLeave))]
        [HarmonyPostfix]
        public static void ItemDropshipPostShipLeave()
        {
            DropshipDecorator.DropshipLeave();
        }

        [HarmonyPatch(typeof(ItemDropship), nameof(ItemDropship.DeliverVehicleClientRpc))]
        [HarmonyPostfix]
        public static void PostDeliverVehicleClientRpc()
        {
            DropshipDecorator.RedecorateDropship(true);
        }
    }
}