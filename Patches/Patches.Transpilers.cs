using Bag;
using CaiJi;
using GUIPackage;
using HarmonyLib;
using script.NewLianDan.LianDan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Tab;
using UnityEngine;
using UnityEngine.Events;
using UnityModularTranslator.Translation;
using YSGame.EquipRandom;
using YSGame.TuJian;

namespace EngTranslatorMod.Patches
{
    public static partial class Patches
    {
        [HarmonyPatch]

        static class Transpiler1_patch
        {
            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(ToolTipsMag), "CreateShuXing", new Type[] { typeof(Bag.BaseSkill) });
                yield return AccessTools.Method(typeof(ToolTipsMag), "CreateShuXing", new Type[] { typeof(Bag.BaseItem) });
                yield return AccessTools.Method(typeof(Bag.BaseSkill), "GetPinJieName");
                yield return AccessTools.Method(typeof(Bag.BaseSkill), "GetPinJie");
                yield return AccessTools.Method(typeof(Skill_UIST), "Show_Tooltip");
                yield return AccessTools.Method(typeof(ShenTongInfoPanel), "RefreshPanelData");
                yield return AccessTools.Method(typeof(KuangShiInfoPanel), "RefreshPanelData");
                yield return AccessTools.Method(typeof(CaoYaoInfoPanel), "RefreshPanelData");
                yield return AccessTools.Method(typeof(YaoShouInfoPanel), "RefreshPanelData");
                yield return AccessTools.Method(typeof(DanYaoInfoPanel), "RefreshPanelData");
                yield return AccessTools.Method(typeof(GongFaInfoPanel), "RefreshPanelData");
                yield return AccessTools.Method(typeof(MiShuInfoPanel), "RefreshPanelData");
                yield return AccessTools.Method(typeof(YaoShouCaiLiaoInfoPanel), "RefreshPanelData");

            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count - 1; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr && Translator.TryGetTranslation(codes[i].operand.ToString(), out string translation))
                    {
                        codes[i].operand = translation;
                    }
                }
                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch]

        static class Transpiler2_patch
        {
            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(item), "StudyTiaoJian", null, null);
                yield return AccessTools.Method(typeof(WuDaoTooltip), "Show", new Type[]
                {
                typeof(Sprite),
                typeof(int),
                typeof(UnityAction)
                }, null);
                yield return AccessTools.Method(typeof(WuDaoCellTooltip), "open", null, null);
                yield return AccessTools.Method(typeof(SiXuData), "Init", null, null);
                yield return AccessTools.Method(typeof(SiXuManager), "initLingGuan", null, null);
                yield return AccessTools.Method(typeof(BiGuanYinfo), "getTaskNextTime", null, null);
                yield return AccessTools.Method(typeof(BiGuanYinfo), "Update", null, null);
                yield return AccessTools.Method(typeof(TaskCell), "getTaskNextTime", null, null);
                yield return AccessTools.Method(typeof(MainUIDataCell), "Init", null, null);
                yield return AccessTools.Method(typeof(Tools), "getStr", null, null);
                yield return AccessTools.Method(typeof(UIBiGuan), "OK", null, null);
                yield return AccessTools.Method(typeof(UIBiGuanXiuLianPanel), "OnStartBiGuanClick", null, null);
                yield return AccessTools.Method(typeof(UIBiGuanPanel), "RefreshKeFangTime", null, null);
                yield return AccessTools.Method(typeof(UIBiGuanXiuLianPanel), "RefreshSpeedUI", null, null);
                yield return AccessTools.Method(typeof(createAvatarChoice), "setValue", null, null);
                yield return AccessTools.Method(typeof(createTianfu), "Awake", null, null);
                yield return AccessTools.Method(typeof(MainUISelectTianFu), "NextPage", null, null);
                yield return AccessTools.Method(typeof(DanLuBag), "GetQualityData", null, null);
                yield return AccessTools.Method(typeof(LianDanPanel), "GetCostTime", null, null);
                yield return AccessTools.Method(typeof(LianDanResultManager), "getCostTime", null, null);
                yield return AccessTools.Method(typeof(LianDanResultManager), "lianDanJieSuan", null, null);
                yield return AccessTools.Method(typeof(LianQiController), "GetItemDesc", null, null);
                yield return AccessTools.Method(typeof(LianQiResultManager), "setEquipNameClick", null, null);
                yield return AccessTools.Method(typeof(Tools), "TimeToShengYuTime", null, null);
                yield return AccessTools.Method(typeof(PaiMaiHang), "JiMai", null, null);
                yield return AccessTools.Method(typeof(UIMiniTaskPanel), "RefreshChuanWen", null, null);
                yield return AccessTools.Method(typeof(XiuLian), "Update", null, null);
                yield return AccessTools.Method(typeof(NPCEx), "AddFavor", null, null);
                yield return AccessTools.Method(typeof(NPCEx), "AddQingFen", null, null);
                yield return AccessTools.Method(typeof(NPCEx), "ZengLiToNPC", null, null);
                yield return AccessTools.Method(typeof(LingHeCaiJiUIMag), "OpenCaiJi", null, null);
                yield return AccessTools.Method(typeof(showCaiLiaoImage), "Click", null, null);
                yield return AccessTools.Method(typeof(JianLingManager), "UnlockXianSuo", null, null);
                yield return AccessTools.Method(typeof(ShowEquipCell), "getEquipDesc", null, null);
                yield return AccessTools.Method(typeof(RandomEquip), "GetEquipQualityDesc", null, null);
                yield return AccessTools.Method(typeof(ShowEquipCell), "setEquipPingJie", null, null);
                yield return AccessTools.Method(typeof(ShowEquipCell), "updateEquipPingJie", null, null);
                //yield return AccessTools.Method(typeof(UIJianLingQingJiaoPanel), "Refresh", null, null); TODO: figure out why this doesn't get recognized by compiler
                yield return AccessTools.Method(typeof(DanGeDanFang_UI), "init", null, null);
                yield return AccessTools.Method(typeof(headUIMag), "showHeadUI", null, null);
                yield return AccessTools.Method(typeof(WuDaoDianPanel), "Show", null, null);
                yield return AccessTools.Method(typeof(Tools), "getSkillText", null, null);
                yield return AccessTools.Method(typeof(LingGuangMag), "ClickLingGuangCell", null, null);
                yield return AccessTools.Method(typeof(LingGuangMag), "GanWu", null, null);
                yield return AccessTools.Method(typeof(LingGuangMag), "GetShengYuShiJian", null, null);
                yield return AccessTools.Method(typeof(UIBiGuanTuPoPanel), "RefreshInventory", null, null);
                yield return AccessTools.Method(typeof(UIBiGuanTuPoPanel), "SetNull", null, null);
                yield return AccessTools.Method(typeof(UIBiGuanTuPoPanel), "TuPo", null, null);
                yield break;
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count - 1; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr && Translator.TryGetTranslation(codes[i].operand.ToString(), out string translation))
                    {
                        codes[i].operand = translation;
                    }
                }
                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch]

        static class LoadAndSaveMenus
        {
            static IEnumerable<MethodBase> TargetMethods()
            {
                yield return AccessTools.Method(typeof(MainUIDataCell), "Click");
                yield return AccessTools.Method(typeof(AvatarInfoCell), "click");
                yield return AccessTools.Method(typeof(Tab.TabDataBase), "Load");
                yield return AccessTools.Method(typeof(Tab.TabDataBase), "Save");

            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count - 1; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldstr)
                    {

                    }

                    if (codes[i].opcode == OpCodes.Ldstr && Translator.TryGetTranslation(codes[i].operand.ToString(), out string translation))
                    {
                        codes[i].operand = translation;


                    }

                }
                return codes.AsEnumerable();


            }
        }

    }
}
