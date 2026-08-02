using System.Collections.Generic;
using System.Reflection;
using Alta.Intelligence;
using MelonLoader;
using HarmonyLib;
using UnityEngine;


[assembly: MelonInfo(typeof(NiceLilSpriggul.Core), "NiceLilSpriggul", "1.0", "Feien")]
[assembly: MelonGame("Alta", "A Township Tale")]

namespace NiceLilSpriggul
{
    public class Core: MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Spriggul... play nice now...");
            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("nice-lil-spriggul.feien.dev");
            harmony.PatchAll(); 
        }
    }
}

[HarmonyPatch(typeof(Spriggull), "InitializeAi")]
class SpriggulDamagePatch
{
    [HarmonyPostfix]
    static void Postfix(Spriggull __instance)
    {
        ActionComponentSet actionSet = AccessTools.Field(typeof(Spriggull), "passiveActions").GetValue(__instance) as ActionComponentSet;
        if (actionSet != null)
        {
            Transform spriggullAi = actionSet.transform.parent;
            AgentRegainControlSettings flailControl = spriggullAi.Find("Flail").GetComponent<AgentRegainControlSettings>();
            if(false){ /*If configured to disable completely*/
                //AgentThreatenSettings stareControl = spriggullAi.Find("Passive Set/Stare").GetComponent<AgentThreatenSettings>();
                List<Transform> unwantedActions = new List<Transform>();
                
                unwantedActions.Add(spriggullAi.Find("Aggressive Set/Jump Attack"));
                unwantedActions.Add(spriggullAi.Find("Aggressive Set/Chase"));
                unwantedActions.Add(spriggullAi.Find("Aggressive Set/Reposition"));
                unwantedActions.Add(spriggullAi.Find("Passive Set/Stare"));
                
                for (int i = 0; i < unwantedActions.Count; i++)
                {
                    Object.Destroy(unwantedActions[i].gameObject);
                }
                
                AccessTools.Field(typeof(AgentRegainControlSettings), "exitTo").SetValue(flailControl, actionSet);
                //AccessTools.Field(typeof(AgentThreatenSettings), "changeTo").SetValue(stareControl, actionSet);
            }
            else
            {
                List<Transform> reConfigureThis = new List<Transform>();
                
                reConfigureThis.Add(spriggullAi.Find("Aggressive Set/Jump Attack"));
                reConfigureThis.Add(spriggullAi.Find("Aggressive Set/Chase"));
                reConfigureThis.Add(spriggullAi.Find("Aggressive Set/Reposition"));
                
                for (int i = 0; i < reConfigureThis.Count; i++)
                {
                    Transform attack = reConfigureThis[i];
                    
                    StatOperation origAggroOperation = attack.Find("Target Preferred/Aggro 0 -> 10 (x1)").GetComponent<StatOperation>();
                    
                    GameObject newGameObject = new GameObject();
                    newGameObject.transform.SetParent(attack.Find("Target Required"), false);
                    newGameObject.name = "Aggro 0 -> 10 (x1)";
                    
                    StatOperation aggroOperation = newGameObject.AddComponent<StatOperation>();
                    FieldInfo[] fields = origAggroOperation.GetType().GetFields();
                    
                    for (int j = 0; j<fields.Length;j++ )
                    {
                        fields[i].SetValue(aggroOperation, fields[i].GetValue(origAggroOperation));
                    }
                }
            }
        }
        else
        {
            MelonLogger.BigError("Failed to override spriggul flail", "FAILED FAILED IFALED");
        }
    }
}
