using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;
using BepInEx;
using AI;
using GameData;
using System.Linq;
using System.Reflection.Emit;
using YanLib;

namespace RuneEquipment
{
    [BepInPlugin("Slimoon.RuneEquipment", "RuneEquipment","2.0.1")]
    public class Main : BaseUnityPlugin
    {
        void Start()
		{
            new Harmony("Slimoon.RuneEquipment").PatchAll();
		}

        public static bool IsEquipped(int id, string rune)//判断指定角色是否装备了指定符文
		{
            List<string> zhuanke = new List<string>
            {
                DateFile.instance.GetItemDate(int.Parse(DateFile.instance.GetActorDate(id,308,true)),504,true,-1),
                DateFile.instance.GetItemDate(int.Parse(DateFile.instance.GetActorDate(id,309,true)),504,true,-1),
                DateFile.instance.GetItemDate(int.Parse(DateFile.instance.GetActorDate(id,310,true)),504,true,-1),
            };
            return zhuanke.Contains(rune);
		}

        public static bool IsActorWeapon(int id,int weaponid)
		{
            List<int> weaponids = new List<int>
            {
                int.Parse(DateFile.instance.GetActorDate(id,301,true)),
                int.Parse(DateFile.instance.GetActorDate(id,302,true)),
                int.Parse(DateFile.instance.GetActorDate(id,303,true))
            };
            return weaponids.Contains(weaponid);
		}

        [HarmonyPatch(typeof(DateFile), "ChangeNewItemSPower")]
        public static class ChangeNewItemSPower_Patch//为玉石宝物生成随机符文
        {
            public static void Postfix(DateFile __instance, Dictionary<int, string> itemData, int baseItemId, int powerId)
			{
                //    DateFile.instance.actorSkills[id][]修习度[]心法等级
                bool YixiangMaster = DateFile.instance.actorSkills.ContainsKey(107);//&&DateFile.instance.actorSkills[107][0]==100&& DateFile.instance.actorSkills[107][1] == 10;
				if (DateFile.instance.presetitemDate[baseItemId][1]=="3" && powerId==407&&YixiangMaster)
				{
                    List<string> RuneLib = new List<string>
                    {
                        "2001",
                        "2002",
                        "2003",
                        "2004",
                        "2005",
                        "2006",
                        "2007",
                        "2008",
                        "2009",
                        "2010",
                        "2011",
                        "2012",
                        "2013",
                        "2014",
                        "2015",
                        "2016",
                        "2017",
                        "2018",
                        "2019",
                        "2020"
                    };
                    itemData[504] = RuneLib[UnityEngine.Random.Range(0, RuneLib.Count)];
                    //itemData[504] = "2001";
				}
			}
		}

        [HarmonyPatch(typeof(BattleVaule), "GetMoveRange")]//致远,2001
        public static class GetMoveRange_Patch
		{
            public static void Postfix(ref int __result, bool isActor)
			{
				if (BattleSystem.Exists)
				{
                    //Debug.Log("开始打架");
                    int ID = isActor ? DateFile.instance.MianActorID() : BattleSystem.instance.ActorId(false, true);
                    //Debug.Log(ID.ToString());
                    if (isActor && IsEquipped(ID, "2001"))
                    {
                        //Debug.Log(ID.ToString());
                        __result = (int)(__result * 1.2f);
                        //BattleSystem.instance.ShowBattleState(50001, isActor);
                    }
                }
			}
		}

        [HarmonyPatch(typeof(BattleVaule), "GetWeaponMaxRange")]//远眺，2002
        public static class GetWeaponMaxRange_Patch
		{
            public static void Postfix(DateFile __instance, ref int __result, bool isActor)
			{
                int ID = isActor ? DateFile.instance.MianActorID() : BattleSystem.instance.ActorId(false, true);
                if (ID > 0&&IsEquipped(ID, "2002"))
                {
                    __result += 10;
                    __result = Mathf.Min(__result, 90);
                    //BattleSystem.instance.ShowBattleState(50002, isActor,0);
                }
            }
		}

        [HarmonyPatch(typeof(BattleVaule), "GetWeaponMinRange")]//近观，2003
        public static class GetWeaponMinRange_Patch
        {
            public static void Postfix(DateFile __instance, ref int __result, bool isActor)
            {
                int ID = isActor ? DateFile.instance.MianActorID() : BattleSystem.instance.ActorId(false, true);
                if (ID > 0 && IsEquipped(ID, "2003"))
                {
                    __result -= 10;
                    __result = Mathf.Max(__result, 20);
                    //BattleSystem.instance.ShowBattleState(50002, isActor,0);
                }
            }
        }

        [HarmonyPatch(typeof(BattleVaule), "SetAttackMagic")]
        public static class SetAttackMagic_Patch
		{
            public static void Postfix(DateFile __instance, ref int __result, int characterId)
			{
                int ID = DateFile.instance.MianActorID();
                bool isActor = ID == characterId;
				if (isActor && IsEquipped(ID, "2004")){//内谐，2004
                    __result = 50;
                    //BattleSystem.instance.ShowBattleState(50004, true, 0);
                }
                else if(isActor && IsEquipped(ID, "2006"))//阴韵，2006
				{
                    __result = 100;
				}
                else if(isActor && IsEquipped(ID, "2007"))//阳韵，2007
				{
                    __result = 0;
                }

                if(!isActor&& IsEquipped(ID, "2005"))//外谐，2005
				{
                    __result = 50;
                    //BattleSystem.instance.ShowBattleState(50005, true, 0);
                }
            }
		}

        [HarmonyPatch(typeof(BattleVaule), "GetAttackNeedTime")]
        public static class GetAttackNeedTime_Patch//若轻，2008
		{
            public static void Postfix(DateFile __instance, ref int __result, int weaponId)
			{
                int ID = DateFile.instance.MianActorID();
                
                if (IsActorWeapon(ID,weaponId) && IsEquipped(ID, "2008"))
				{
                    __result= 3000 + int.Parse(DateFile.instance.GetItemDate(weaponId, 501, true, -1));
                }
            }
		}

        [HarmonyPatch(typeof(BattleVaule), "GetWeaponCd")]
        public static class GetWeaponCd_Patch
		{
            public static void Postfix(DateFile __instance, ref int __result, bool isActor)//巧手，2009
			{
                int ID = DateFile.instance.MianActorID();
                if (isActor && IsEquipped(ID, "2009"))
                {
                    __result = (int)(__result * 0.7f);
                }
            }
		}

        [HarmonyPatch(typeof(BattleVaule), "GetMaxDp")]
        public static class GetMaxDp_Patch
		{
            public static void Postfix(BattleSystem __instance, ref int __result, bool isActor)//厚颜，2010
			{
                int ID = DateFile.instance.MianActorID();
                if (isActor && IsEquipped(ID, "2010"))
                {
                    float charmBouns = (int.Parse(DateFile.instance.GetActorDate(ID, 15, true)) + 900) / 900;
                    __result = (int)(__result * charmBouns);
                    BattleSystem.instance.ShowBattleState(50010, true, 0);
                 //   YanLib.DataManipulator.Actor.DumpActorData(ID, false, false, @"C:\Users\75283\Desktop\MianActor");
                }
				
            }
		}

        [HarmonyPatch(typeof(BattleVaule), "GetMoveGongFaRund")]
        public static class GetMoveGongFaRund_Patch
		{
            public static void Postfix(DateFile __instance, ref int __result, int gongFaId)//翻腾，2011
			{
                int ID = DateFile.instance.MianActorID();
                int Index = DateFile.instance.mianActorEquipGongFaIndex;
                if(DateFile.instance.mianActorEquipGongFas[Index][2]!=null)
                if (DateFile.instance.mianActorEquipGongFas[Index][2].Contains(gongFaId)&&IsEquipped(ID,"2011"))
				{
                    __result++;
                    BattleSystem.instance.ShowBattleState(50011, true, 0);
                }
			}
		}

        [HarmonyPatch(typeof(BattleSystem), "AddBattleInjury")]
        public static class AddBattleInjury_Patch//怀柔，2012；留悯，2013；劝学，2014；好学，2015
		{
            public static void Postfix(BattleSystem __instance, bool isActor, int actorId, int attackerId, int injuryId, int injuryPower)
			{
                int ID = DateFile.instance.MianActorID();
                bool isNei = injuryId % 6 == 4 || injuryId % 6 == 5 || injuryId % 6 == 0;
                if (!isActor && IsEquipped(ID, "2012") &&isNei)//怀柔，2012
				{
                    BattleSystem.instance.DoHeal(isActor, 50, false, true, 1);
                    BattleSystem.instance.ShowBattleState(50012, false, 0);
				}
                if (!isActor && IsEquipped(ID, "2013")&& !isNei)//留悯，2013
                {
                    BattleSystem.instance.DoHeal(isActor, 50, false, true, 0);
                    BattleSystem.instance.ShowBattleState(50013, false, 0);
                }
				if (!isActor)//劝学，2014，好学，2015
				{

                    if (IsEquipped(ID,"2014")&&UnityEngine.Random.Range(0, 100) < 25)
					{
                        Traverse.Create(__instance).Method("RandAddCostIcon", false, 1).GetValue();
                        BattleSystem.instance.ShowBattleState(50014, false, 0);
                    }
                    if (IsEquipped(ID, "2015") && UnityEngine.Random.Range(0, 100) < 10)
                    {
                        Traverse.Create(__instance).Method("RandAddCostIcon", true, 1).GetValue();
                        BattleSystem.instance.ShowBattleState(50015, true, 0);
                    }

                }
            }
		}

        [HarmonyPatch(typeof(BattleSystem), "BattlerIsDanger")]
        public static class BattlerIsDanger_Patch//无畏，2016，懦弱，2017
		{
            public static void Postfix(ref bool __result)
			{
                int ID1 = DateFile.instance.MianActorID();
                int ID2 = BattleSystem.instance.ActorId(false, true);
                if(ID2 > 0)
				{
                    if (IsEquipped(ID1, "2016") || IsEquipped((ID2), "2016"))
                    {
                        __result = false;
                    }
                    else if (IsEquipped(ID1, "2017") || IsEquipped(ID2, "2017"))
                    {
                        __result = true;
                    }
                }
				
			}
		}

        [HarmonyPatch(typeof(DateFile), "GetGongFaBasePower")]//欺世，2018，功法威力部分
        public static class GetGongFaBasePower_Patch
		{
            public static void Postfix(ref int __result, int actorId)
			{
                var equipG = DateFile.instance.GetActorEquipGongFa(actorId);
                bool nv = equipG[0].Contains(809);
                bool nan = equipG[0].Contains(109);
                List<int> actorFeature = DateFile.instance.GetActorFeature(actorId, false);
                bool za = !actorFeature.Contains(4001);
                if (IsEquipped(actorId, "2018") && (nv||nan) && za)
				{
                    __result += (nan ? 30 : 0) + (nv ? 30 : 0);
				}
			}
		}

        [HarmonyPatch(typeof(DateFile), "GetActorFaceAge")]//欺世，2018，相貌部分
        public static class GetActorFaceAge_Patch
		{
            public static void Postfix(ref int __result,int id)
			{
                int actorId = id;
                var equipG = DateFile.instance.GetActorEquipGongFa(actorId);
                bool nv = equipG[0].Contains(809);
                bool nan = equipG[0].Contains(109);
                List<int> actorFeature = DateFile.instance.GetActorFeature(actorId, false);
                bool za = !actorFeature.Contains(4001);
                if (IsEquipped(actorId, "2018") && (nv || nan) && za)
                {
                    __result = 16;
                }
            }
		}

        [HarmonyPatch(typeof(BattleSystem), "DoHeal")]
        public static class DoHeal_Patch
		{
            public static void Postfix(bool isActor)
			{
                int ID = DateFile.instance.MianActorID();
                if (isActor && IsEquipped(ID, "2019"))//祝福,2019
                {
                    int Fame = DateFile.instance.GetActorFame(ID);
                    if (Fame >= 0)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            BattleSystem.AddSp(true, i, Fame / 20);
                        }
                    }
                }
            }
		}

        [HarmonyPatch(typeof(BattleSystem), "DoRemovePoison")]
        public static class DoRemovePoison_Patch
        {
            public static void Postfix(bool isActor)
            {
                int ID = DateFile.instance.MianActorID();
                if (isActor && IsEquipped(ID, "2020"))//诅咒,2020
                {
                    int Fame = DateFile.instance.GetActorFame(ID);
                    if (Fame <= 0)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            BattleSystem.AddSp(false, i, -Fame / 20);
                        }
                    }
                }
            }
        }

    }
}
