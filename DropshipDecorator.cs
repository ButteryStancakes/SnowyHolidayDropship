using System.IO;
using System.Reflection;
using System.Collections;
using UnityEngine;
using BepInEx.Bootstrap;

namespace SnowyHolidayDropship
{
    internal static class DropshipDecorator
    {
        static ItemDropship shipScript;

        static AudioClip music, musicFar, musicJolly, musicFarJolly, musicOld, musicFarOld;
        static Mesh ship, shipJolly;

        static GameObject star, shipObject;
        static MeshFilter shipComponent;
        static AudioSource musicComponent, musicFarComponent;

        static System.Random rand = new();
        static bool initialized, landed, seeded;

        static GameObject artificeBlizzard;

        internal static void Init(ItemDropship itemDropship)
        {
            initialized = false;
            shipScript = itemDropship;

            try
            {
                Transform shipParent = shipScript.transform;
                star = shipParent.Find("Star").gameObject;
                shipObject = shipParent.Find("ItemShip").gameObject;
                shipComponent = shipObject.GetComponent<MeshFilter>();
                Transform musicParent = shipParent.Find("Music");
                musicComponent = musicParent.GetComponent<AudioSource>();
                musicFarComponent = musicParent.Find("Music (1)").GetComponent<AudioSource>();
                Plugin.Logger.LogInfo("Successfully cached all dropship object references");

                // actually, for modded moon compatibility, it's better to re-cache the default audio clips each day
                /*if (ship == null || music == null || musicFar == null)
                {*/
                    ship = shipComponent.sharedMesh;
                    music = musicComponent.clip;
                    musicFar = musicFarComponent.clip;
                    Plugin.Logger.LogInfo("Successfully cached all pre-existing asset references");
                //}

                if (musicJolly == null || musicFarJolly == null || shipJolly == null || musicOld == null)
                {
                    AssetBundle holidayBundle = AssetBundle.LoadFromFile(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "snowyholidaydropship"));
                    musicJolly = holidayBundle.LoadAsset<AudioClip>("IcecreamTruckV2Christmas");
                    musicFarJolly = holidayBundle.LoadAsset<AudioClip>("IcecreamTruckV2ChristmasFar");
                    musicOld = holidayBundle.LoadAsset<AudioClip>("IcecreamTruck");
                    musicFarOld = holidayBundle.LoadAsset<AudioClip>("IcecreamTruckV1Far");
                    shipJolly = holidayBundle.LoadAsset<Mesh>("MainShipPart");
                    holidayBundle.Unload(false);
                    Plugin.Logger.LogInfo("Successfully cached all asset bundle references");
                }

                if (StartOfRound.Instance.currentLevel.name == "ArtificeLevel" && Chainloader.PluginInfos.ContainsKey("butterystancakes.lethalcompany.artificeblizzard"))
                {
                    artificeBlizzard = GameObject.Find("/Systems/Audio/BlizzardAmbience");
                    if (artificeBlizzard != null)
                        Plugin.Logger.LogInfo("Artifice Blizzard compatibility success");
                }

                initialized = true;
                landed = false;
                seeded = false;
            }
            catch (System.Exception e)
            {
                Plugin.Logger.LogError("Failed to capture references to all the dropship objects - are you playing on a modded moon?");
                Plugin.Logger.LogError("Please send the information below to the developer, and mention what moon this error occurred on");
                Plugin.Logger.LogError(e);
            }
        }

        internal static void RedecorateDropship(bool vehicle = false)
        {
            if (!initialized || landed)
                return;
            landed = true;

            if (!seeded)
            {
                rand = new(StartOfRound.Instance.randomMapSeed);
                Plugin.Logger.LogInfo($"RNG initialized (Seed: {StartOfRound.Instance.randomMapSeed})");
                seeded = true;
            }

            Plugin.Logger.LogInfo("Roll chance for holiday");
            bool jolly = RandomChance(IsSnowLevel() ? (double)Plugin.configSnowyChance.Value : (double)Plugin.configNormalChance.Value);
            Plugin.Logger.LogInfo("Roll chance for old music");
            bool classic = RandomChance((double)Plugin.configLegacyChance.Value);

            if (vehicle)
            {
                shipComponent.mesh = ship;
                star.SetActive(false);
                Plugin.Logger.LogInfo("Dropship: Normal (delivering vehicle)");
            }
            else if (jolly)
            {
                // jolly!
                shipComponent.mesh = shipJolly;
                musicComponent.clip = musicJolly;
                musicFarComponent.clip = musicFarJolly;
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
                    musicFarComponent.clip = musicFarOld;
                    Plugin.Logger.LogInfo("Dropship: Normal (old music)");
                }
                else
                {
                    // normal music
                    musicComponent.clip = music;
                    musicFarComponent.clip = musicFar;
                    Plugin.Logger.LogInfo("Dropship: Normal");
                }
                star.SetActive(false);
            }
        }

        internal static void DropshipLeave()
        {
            landed = false;
            if (initialized && star.activeSelf)
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

        static bool IsSnowLevel()
        {
            return StartOfRound.Instance.currentLevel.levelIncludesSnowFootprints && (artificeBlizzard == null || artificeBlizzard.activeSelf);
        }
    }
}
