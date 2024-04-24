using System.IO;
using System.Reflection;
using System.Collections;
using UnityEngine;

namespace SnowyHolidayDropship
{
    internal static class DropshipDecorator
    {
        static ItemDropship shipScript;

        static AudioClip music, musicFar, musicJolly, musicFarJolly, musicOld;
        static Mesh ship, shipJolly;

        static GameObject star, shipObject;
        static MeshFilter shipComponent;
        static AudioSource musicComponent, musicFarComponent;

        static System.Random rand;
        static bool landed;

        internal static void Init(ItemDropship itemDropship)
        {
            shipScript = itemDropship;

            Transform shipParent = shipScript.transform;
            star = shipParent.Find("Star").gameObject;
            shipObject = shipParent.Find("ItemShip").gameObject;
            shipComponent = shipObject.GetComponent<MeshFilter>();
            Transform musicParent = shipParent.Find("Music");
            musicComponent = musicParent.GetComponent<AudioSource>();
            musicFarComponent = musicParent.Find("Music (1)").GetComponent<AudioSource>();
            Plugin.Logger.LogInfo("Successfully cached all dropship object references");

            if (shipJolly == null || music == null || musicFar == null)
            {
                shipJolly = shipComponent.sharedMesh;
                music = musicComponent.clip;
                musicFar = musicFarComponent.clip;
                Plugin.Logger.LogInfo("Successfully cached all pre-existing asset references");
            }

            if (musicJolly == null || musicFarJolly == null || ship == null || musicOld == null)
            {
                AssetBundle holidayBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "snowyholidaydropship"));
                musicJolly = holidayBundle.LoadAsset<AudioClip>("IcecreamTruckV2Christmas");
                musicFarJolly = holidayBundle.LoadAsset<AudioClip>("IcecreamTruckV2ChristmasFar");
                musicOld = holidayBundle.LoadAsset<AudioClip>("IcecreamTruck");
                ship = holidayBundle.LoadAsset<Mesh>("MainShipPart");
                holidayBundle.Unload(false);
                Plugin.Logger.LogInfo("Successfully cached all asset bundle references");
            }

            rand = new(StartOfRound.Instance.randomMapSeed);
            Plugin.Logger.LogInfo($"RNG initialized (Seed: {StartOfRound.Instance.randomMapSeed})");

            landed = false;
        }

        internal static void RedecorateDropship()
        {
            if (landed)
                return;
            landed = true;

            Plugin.Logger.LogInfo("Roll chance for holiday");
            bool jolly = RandomChance(StartOfRound.Instance.currentLevel.levelIncludesSnowFootprints ? (double)Plugin.configSnowyChance.Value : (double)Plugin.configNormalChance.Value);
            Plugin.Logger.LogInfo("Roll chance for old music");
            bool classic = RandomChance((double)Plugin.configLegacyChance.Value);
            if (jolly)
            {
                // jolly!
                shipComponent.mesh = shipJolly;
                musicComponent.clip = musicJolly;
                musicFarComponent.clip = musicFarJolly;
                musicFarComponent.mute = false;
                star.SetActive(true);
                Plugin.Logger.LogInfo("Dropship: Holiday");
            }
            else
            {
                shipComponent.mesh = ship;
                if (classic)
                {
                    // classic music
                    musicComponent.clip = musicOld;
                    musicFarComponent.mute = true;
                    Plugin.Logger.LogInfo("Dropship: Normal (old music)");
                }
                else
                {
                    // normal music
                    musicComponent.clip = music;
                    musicFarComponent.clip = musicFar;
                    musicFarComponent.mute = false;
                    Plugin.Logger.LogInfo("Dropship: Normal");
                }
                star.SetActive(false);
            }
        }

        internal static void DropshipLeave()
        {
            landed = false;
            if (star.activeSelf)
                shipScript.StartCoroutine(DisableStarInSky());
        }

        internal static IEnumerator DisableStarInSky()
        {
            float start = Time.time;
            Plugin.Logger.LogInfo("Holiday: Upon reaching orbit, disable star");
            while (shipObject.activeSelf)
                yield return null;
            star.SetActive(false);
            Plugin.Logger.LogInfo($"Holiday: Star disabled after {Time.time - start}s");
        }

        static bool RandomChance(double chance)
        {
            double rng = rand.NextDouble();
            Plugin.Logger.LogInfo($"RNG: {rng} < {chance}");
            return chance >= 1f || (chance > 0f && rng < chance);
        }
    }
}
