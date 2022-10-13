using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace FlakGunners
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Harmony.CreateAndPatchAll(typeof(Plugin));
        }

        [HarmonyPatch(typeof(Weapon), "GetDistanceToAimPoint")]
        [HarmonyPostfix]
        public static void GetDistanceToAimPoint (Weapon __instance, ref float __result)
        {

            PlayerControl pc = (PlayerControl) Traverse.Create(__instance).Field("pc").GetValue();
            bool isDrone = (bool) Traverse.Create(__instance).Field("isDrone").GetValue();
            Transform muzzleFlashTrans = (Transform) Traverse.Create(__instance).Field("muzzleFlashTrans").GetValue();
            AIControl aic = (AIControl) Traverse.Create(__instance).Field("aic").GetValue();
            ProjectileControl projControl = (ProjectileControl) Traverse.Create(__instance).Field("projControl").GetValue();

            Vector3 aimPoint;
            if ((bool)pc && pc.GetSpaceShip.crew.GetGunner(__instance.weaponSlotIndex) != null && pc.GetSpaceShip.crew.GetGunner(__instance.weaponSlotIndex).control == 0)
            {
                aimPoint = projControl.target.position;
                var gunnerSkill = pc.GetSpaceShip.crew.GetGunner(__instance.weaponSlotIndex).crewMember.GetSkillLevel(CrewPosition.Gunner, pc.GetSpaceShip);
                if (gunnerSkill == 0) gunnerSkill = 1;
                Vector3 translation = new Vector3(Random.Range(-1f, 1f) * (100f / gunnerSkill), 0f, 0);
                float n = Vector3.Distance(aimPoint, muzzleFlashTrans.position) / projControl.speed;
                translation += n * (projControl.target.GetComponent<Rigidbody>().velocity - pc.GetSpaceShip.GetComponent<Rigidbody>().velocity);
                
                aimPoint += translation;



                float num = Vector3.Distance(muzzleFlashTrans.position, aimPoint);
                if (num > (float)__instance.range)
                {
                   num = __instance.range;
                }
                __result = num;

            }
        }
    }
}
