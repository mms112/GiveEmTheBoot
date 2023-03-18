using System.Linq;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;

namespace GiveEmTheBoot.Util;

public class Functions
{
    public static void AddYeetText(Vector3 pos)
    {
        if (GiveEmTheBootPlugin.dialogSelection.Value.Length > 0)
        {
            string[] selection = GiveEmTheBootPlugin.dialogSelection.Value.Split('|');
            string text = new string(selection[Random.Range(0, selection.Length - 1)].ToCharArray());
            DamageText.WorldTextInstance worldTextInstance = new()
            {
                m_worldPos = pos,
                m_gui = Object.Instantiate(DamageText.instance.m_worldTextBase,
                    DamageText.instance.GetComponent<Transform>())
            };
            worldTextInstance.m_textField = worldTextInstance.m_gui.GetComponent<Text>();
            DamageText.instance.m_worldTexts.Add(worldTextInstance);
            Color white = new Color(1f, 0.75f, 0f, 1f);
            worldTextInstance.m_textField.color = white;
            worldTextInstance.m_textField.fontSize = DamageText.instance.m_largeFontSize + 4;

            worldTextInstance.m_textField.text = text;
            worldTextInstance.m_timer = 0f;
        }
    }
}

public static class KeyboardExtensions
{ 
    // thank you to 'Margmas' for giving me this snippet from VNEI https://github.com/MSchmoecker/VNEI/blob/master/VNEI/Logic/BepInExExtensions.cs#L21
    // since KeyboardShortcut.IsPressed and KeyboardShortcut.IsDown behave unintuitively
    public static bool IsKeyDown(this KeyboardShortcut shortcut)
    {
        return shortcut.MainKey != KeyCode.None && Input.GetKeyDown(shortcut.MainKey) && shortcut.Modifiers.All(Input.GetKey);
    }

    public static bool IsKeyHeld(this KeyboardShortcut shortcut)
    {
        return shortcut.MainKey != KeyCode.None && Input.GetKey(shortcut.MainKey) && shortcut.Modifiers.All(Input.GetKey);
    }
}

public static class PrefabGetter
{
    private static Transform pingVisual;
    private static GameObject pingAudio;

    public static Transform getPingVisual()
    {
        if (pingVisual == null)
        {
            GameObject fetch = ZNetScene.instance.GetPrefab("vfx_sledge_hit");
            Transform fetch2 = fetch.transform.Find("waves");
            pingVisual = Object.Instantiate(fetch2);
            ParticleSystem.MainModule mainModule = pingVisual.GetComponent<ParticleSystem>().main;
            mainModule.simulationSpeed = 0.2F;
            mainModule.startSize = 0.1F;
            mainModule.startSizeMultiplier = 60F;


            /*
            GameObject fetch = ZNetScene.instance.GetPrefab("vfx_blocked");
            Transform fetch2 = fetch.transform.Find("waves");
            pingVisual = UnityEngine.Object.Instantiate<Transform>(fetch2);
            MainModule mainModule = pingVisual.GetComponent<ParticleSystem>().main;
            mainModule.simulationSpeed = 0.2F;
            mainModule.startSize = 0.1F;
            mainModule.startSizeMultiplier = 6F;
            */
        }


        return pingVisual;
    }

    public static GameObject getPingAudio()
    {
        if (pingAudio == null)
        {
            GameObject fetch = ZNetScene.instance.GetPrefab("sfx_lootspawn");

            if (fetch != null)
            {
                pingAudio = fetch;
                Debug.Log("Loaded " + pingAudio.ToString() + " prefab.");
                return fetch;
            }

            Debug.Log("Failed to load the audio prefab.");
            return null;
        }

        return pingAudio;
    }
}