using BepInEx.Configuration;
using HarmonyLib;
using System;
using GiveEmTheBoot.Util;
using UnityEngine;

namespace GiveEmTheBoot.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.PlayerAttackInput))]
    public static class PlayerAttackInputPatch
    {
        //Loaded config values for adjusting the mod as you see fit.
        public static bool invalidKey = false;

        //In this method we check when pushback is being applied to an enemy, and if we're kicking them, we add force.
        public static void Postfix(Player __instance, float dt)
        {
            if (__instance.InPlaceMode())
            {
                return;
            }

            if (!__instance.TakeInput()) return;

            //Check the kick hotkey first.
            bool kickInput = false;

            try
            {
                if (GiveEmTheBootPlugin.kickHotkey.Value.IsKeyHeld())
                {
                    kickInput = true;
                }
            }
            catch (Exception e)
            {
                if (!invalidKey)
                {
                    GiveEmTheBootPlugin.GiveEmTheBootLogger.LogError(
                        "You bound an invalid key code that Unity cannot recognize. Check the config file.");
                    invalidKey = true;
                }
            }

            //Go ahead with the kick
            if (kickInput)
            {
                __instance.ClearActionQueue();
                if ((__instance.InAttack() && !__instance.HaveQueuedChain()) || __instance.InDodge() ||
                    !__instance.CanMove() || __instance.IsKnockedBack() || __instance.IsStaggering() ||
                    __instance.InMinorAction())
                {
                    return;
                }

                if (__instance.m_currentAttack != null)
                {
                    __instance.m_currentAttack.Stop();
                    __instance.m_previousAttack = __instance.m_currentAttack;
                    __instance.m_currentAttack = null;
                }

                ItemDrop.ItemData currentWeapon = __instance.m_unarmedWeapon.m_itemData;
                Attack attack = currentWeapon.m_shared.m_secondaryAttack.Clone();

                if (attack.Start(__instance, __instance.m_body, __instance.m_zanim, __instance.m_animEvent,
                        __instance.m_visEquipment, currentWeapon, __instance.m_previousAttack,
                        __instance.m_timeSinceLastAttack, 0F))
                {
                    __instance.m_currentAttack = attack;
                    __instance.m_lastCombatTimer = 0f;
                }
            }
        }
    }
}