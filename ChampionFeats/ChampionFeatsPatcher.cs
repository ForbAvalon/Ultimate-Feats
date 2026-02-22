using ChampionFeats.Components;
using ChampionFeats.Extensions;
using ChampionFeats.Utilities;
using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Designers.Mechanics.Recommendations;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using System;

namespace ChampionFeats
{
    class ChampionFeatsPatcher
    {
        [HarmonyPatch(typeof(BlueprintsCache), "Init")]
        public static class BlueprintPatcher
        {
            static bool Initialized;

            [HarmonyPriority(Priority.LowerThanNormal)]
            static void Postfix()
            {
                if (Initialized) return;
                Initialized = true;
                Main.Log("Adding Champion Feats");

                AddChampionDefences(); //int wis and cha
                AddChampionOffences();
                AddChampionMagics();
            }

            private static string getStepString(int step)
            {
                string stepString = "";
                switch (step)
                {
                    case 1:
                        stepString = "此后每1";
                        break;
                    case 2:
                        stepString = "在第2级以及此后每2";
                        break;
                    case 3:
                        stepString = "在第3级以及此后每3";
                        break;
                    default:
                        stepString = String.Format("在第{0}级以及此后每{0}", step);
                        break;
                }
                return stepString;
            }

            private static string getStepStringWOffset(int step)
            {
                string stepString = "";
                switch (step)
                {
                    case 1:
                        stepString = "此后每1";
                        break;
                    case 2:
                        stepString = "在第3级以及此后每2";
                        break;
                    case 3:
                        stepString = "在第4级以及此后每3";
                        break;
                    default:
                        stepString = String.Format("在第{0}级以及此后每{1}", step, step - 1);
                        break;
                }
                return stepString;
            }

            public static ContextRankConfig.CustomProgressionItem[] makeCustomProgression(int levelsPerStep, int bonusPerStep, int levelStepOffset = 0)
            {
                // levelsPerStep must be within [1,40]
                if (levelsPerStep < 1)
                {
                    levelsPerStep = 1;
                }
                if (levelsPerStep > 40)
                {
                    levelsPerStep = 40;
                }
                // max level 40
                int steps = (40 / levelsPerStep) + 1;

                ContextRankConfig.CustomProgressionItem[] items = new ContextRankConfig.CustomProgressionItem[steps];
                int bonus = bonusPerStep;
                int level = levelsPerStep + levelStepOffset;

                for (int step = 0; step < steps; step++)
                {
                    items[step] = new ContextRankConfig.CustomProgressionItem()
                    {
                        ProgressionValue = bonus,
                        BaseValue = level
                    };

                    bonus += bonusPerStep;
                    level += levelsPerStep;
                }

                items[steps - 1].BaseValue = 99;

                return items;
            }

            static void AddChampionDefences()
            {
                var ChampionDefenceAC = Helpers.CreateBlueprint<BlueprintFeature>(AddACFromArmor.BLUEPRINTNAME, bp =>
                {
                    bp.IsClassFeature = true;
                    bp.ReapplyOnLevelUp = true;
                    if (!Main.settings.FeatsAreMythic)
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.CombatFeat, FeatureGroup.Feat };
                    }
                    else
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.MythicFeat };
                    }
                    bp.Ranks = 1;
                    bp.SetName("终极防御");
                    string stepString = getStepString(Main.settings.ScalingACLevelsPerStep);
                    bp.SetDescription(String.Format("无论源于苦练还是受教，你身着护甲时的防御能力皆超乎常人。穿戴轻型/中型/重型护甲时，你获得+{0}/+{1}/+{2}点防御等级。{3}级，该加值按相同幅度增加。",
                        Main.settings.ScalingACArmorBonusLightPerStep, Main.settings.ScalingACArmorBonusMediumPerStep, Main.settings.ScalingACArmorBonusHeavyPerStep, stepString));
                    bp.m_DescriptionShort = bp.m_Description;
                    bp.AddComponent(Helpers.Create<AddACFromArmor>(c =>
                    {
                        c.LightBonus = Main.settings.ScalingACArmorBonusLightPerStep;
                        c.MediumBonus = Main.settings.ScalingACArmorBonusMediumPerStep;
                        c.HeavyBonus = Main.settings.ScalingACArmorBonusHeavyPerStep;
                        c.StepLevel = Main.settings.ScalingACLevelsPerStep;
                    }));

                    bp.AddComponent(Helpers.Create<AddMechanicsFeature>(amf =>
                    {
                        amf.m_Feature = AddMechanicsFeature.MechanicsFeatureType.ImmunityToArmorSpeedPenalty;
                    }));


                });


                var ChampionDefenceDR = Helpers.CreateBlueprint<BlueprintFeature>(AddScalingDamageResistance.BLUEPRINTNAME, bp =>
                {
                    bp.IsClassFeature = true;
                    bp.ReapplyOnLevelUp = true;
                    if (!Main.settings.FeatsAreMythic)
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.CombatFeat, FeatureGroup.Feat };
                    }
                    else
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.MythicFeat };
                    }
                    bp.Ranks = 1;
                    bp.SetName("终极坚韧");
                    string stepString = getStepString(Main.settings.ScalingDRLevelsPerStep);
                    bp.SetDescription(String.Format("你坚定屹立，抵御一切袭来的物理伤害，无论其形式如何。你获得+{0}点DR/-。{1}级，该数值增加+{0}。",
                        Main.settings.ScalingDRBonusPerStep, stepString));
                    bp.m_DescriptionShort = bp.m_Description;

                    /*
                    bp.AddComponent(Helpers.Create<AddScalingDamageResistance.AddDamageResistanceAllScaling>(c =>
                    {
                        c.name = "RMChampionGuardBuff";
                    }));
                    */

                    bp.AddComponent(Helpers.Create<AddDamageResistancePhysical>(c =>
                    {
                        c.name = "RMChampionGuardBuff";
                        c.MinEnhancementBonus = 5;
                        // I apologize for the brute force
                        c.Value = new ContextValueDR()
                        {
                            ValueType = ContextValueType.Simple,
                            // not sure about this
                            //Value = Main.settings.ScalingDRBonusPerStep
                            Value = 1
                        };
                    }));

                });


                var ChampionDefenceSaves = Helpers.CreateBlueprint<BlueprintFeature>(AddScalingSavingThrows.BLUEPRINTNAME, bp =>
                {
                    bp.IsClassFeature = true;
                    bp.ReapplyOnLevelUp = true;
                    if (!Main.settings.FeatsAreMythic)
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.CombatFeat, FeatureGroup.Feat };
                    }
                    else
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.MythicFeat };
                    }
                    bp.Ranks = 1;
                    bp.SetName("终极豁免");
                    string stepString = getStepString(Main.settings.ScalingSaveLevelsPerStep);
                    bp.SetDescription(string.Format("你规避危险的本能护你免受伤害。你的所有豁免检定获得+{0}加值。{1}级，该数值增加+{0}。",
                        Main.settings.ScalingSaveBonusPerLevel, stepString));
                    bp.m_DescriptionShort = bp.m_Description;

                    bp.AddComponent(Helpers.Create<AddScalingSavingThrows>());
                });

                if (!Main.settings.FeatsAreMythic)
                {
                    FeatTools.AddAsFeat(ChampionDefenceAC);
                    FeatTools.AddAsFeat(ChampionDefenceDR);
                    FeatTools.AddAsFeat(ChampionDefenceSaves);
                }
                else
                {
                    FeatTools.AddAsMythicFeats(ChampionDefenceAC);
                    FeatTools.AddAsMythicFeats(ChampionDefenceDR);
                    FeatTools.AddAsMythicFeats(ChampionDefenceSaves);
                }

            }

            static void AddChampionOffences()
            {
                var ChampionSkills = Helpers.CreateBlueprint<BlueprintFeature>(AddScalingSkillBonus.BLUEPRINTNAME, bp =>
                {
                    bp.IsClassFeature = true;
                    bp.ReapplyOnLevelUp = true;
                    if (!Main.settings.FeatsAreMythic)
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.CombatFeat, FeatureGroup.Feat };
                    }
                    else
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.MythicFeat };
                    }
                    bp.Ranks = 1;
                    bp.SetName("终极技艺");
                    string stepString = getStepString(Main.settings.ScalingSkillsLevelsPerStep);
                    string descString = string.Format("你的造诣无可限量，能以匪夷所思的速度掌握新技能。你的所有技能获得+{0}加值。{1}级，该数值增加+{0}。",
                        Main.settings.ScalingSkillsBonusPerLevel, stepString);
                    if (AddScalingSkillBonus.AddToBaseValue)
                    {
                        descString += " 此专长提供的加值将直接计入技能等级，而不会在进行技能检定时作为加值单独显示。";
                    }
                    bp.SetDescription(descString);
                    bp.m_DescriptionShort = bp.m_Description;

                    bp.AddComponent(Helpers.Create<AddScalingSkillBonus>());
                    bp.AddComponent(Helpers.Create<AddIdentifyBonus>(ib =>
                    {
                        ib.AllowUsingUntrainedSkill = true;
                    }));
                    bp.AddComponent(Helpers.Create<AddMechanicsFeature>(mf =>
                    {
                        mf.m_Feature = AddMechanicsFeature.MechanicsFeatureType.MakeKnowledgeCheckUntrained;
                    }));
                });

                var ChampionOffenceAB = Helpers.CreateBlueprint<BlueprintFeature>(AddScalingAttackBonus.BLUEPRINTNAME, bp =>
                {
                    bp.IsClassFeature = true;
                    bp.ReapplyOnLevelUp = true;
                    if (!Main.settings.FeatsAreMythic)
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.CombatFeat, FeatureGroup.Feat };
                    }
                    else
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.MythicFeat };
                    }
                    bp.Ranks = 1;
                    bp.SetName("终极精准");
                    String stepString = getStepStringWOffset(Main.settings.ScalingABLevelsPerStep);
                    bp.SetDescription(String.Format("无论源于苦练还是受教，你命中目标的能力皆超乎常人。你获得+{0}点攻击加值。{1}级，该数值增加+{0}。",
                        Main.settings.ScalingABBonusPerStep, stepString));
                    bp.m_DescriptionShort = bp.m_Description;
                    bp.AddComponent(Helpers.Create<AddScalingAttackBonus>(c => { }));

                });


                var ChampionOffenceDam = Helpers.CreateBlueprint<BlueprintFeature>(AddScalingDamageBonus.BLUEPRINTNAME, bp =>
                {
                    bp.IsClassFeature = true;
                    bp.ReapplyOnLevelUp = true;
                    if (!Main.settings.FeatsAreMythic)
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.CombatFeat, FeatureGroup.Feat };

                    }
                    else
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.MythicFeat };
                    }
                    bp.Ranks = 1;
                    bp.SetName("终极猛击");
                    String stepString = getStepStringWOffset(Main.settings.ScalingDamageLevelsPerStep);
                    bp.SetDescription(String.Format("无论敌人多么顽强，你的武器攻击皆能重创对手。你的攻击获得+{0}点伤害。{1}级，该数值增加+{0}。",
                        Main.settings.ScalingDamageBonusPerStep, stepString));
                    bp.m_DescriptionShort = bp.m_Description;
                    bp.AddComponent(Helpers.Create<AddScalingDamageBonus>(c => { }));
                });

                if (!Main.settings.FeatsAreMythic)
                {
                    FeatTools.AddAsFeat(ChampionSkills);
                    FeatTools.AddAsFeat(ChampionOffenceAB);
                    FeatTools.AddAsFeat(ChampionOffenceDam);
                }
                else
                {
                    FeatTools.AddAsMythicFeats(ChampionSkills);
                    FeatTools.AddAsMythicFeats(ChampionOffenceAB);
                    FeatTools.AddAsMythicFeats(ChampionOffenceDam);
                }
            }
            static void AddChampionMagics()
            {
                var SpellPen = Resources.GetBlueprint<BlueprintFeature>("ee7dc126939e4d9438357fbd5980d459");

                var ChampionOffenceSpellDam = Helpers.CreateBlueprint<BlueprintFeature>(AddScalingSpellDamage.BLUEPRINTNAME, bp =>
                {
                    bp.IsClassFeature = true;
                    bp.ReapplyOnLevelUp = true;
                    if (!Main.settings.FeatsAreMythic)
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.WizardFeat, FeatureGroup.Feat };
                    }
                    else
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.MythicFeat };
                    }
                    bp.Ranks = 1;
                    bp.SetName("终极魔爆");
                    String stepString = getStepStringWOffset(Main.settings.ScalingSpellDamageLevelsPerStep);
                    bp.SetDescription(String.Format("无论敌人多么顽强，你的魔法技艺皆能重创对手。你的法术攻击每个伤害骰获得+{0}点伤害。{1}级，该数值增加+{0}。",
                        Main.settings.ScalingSpellDamageBonusPerStep, stepString));
                    bp.m_DescriptionShort = bp.m_Description;
                    bp.AddComponent(Helpers.Create<AddScalingSpellDamage>(c => { }));
                    bp.AddComponent(Helpers.Create<RecommendationRequiresSpellbook>());
                    bp.AddComponent(Helpers.Create<FeatureTagsComponent>(c =>
                    {
                        c.FeatureTags = FeatureTag.Magic;
                    }));
                });

                var ChampionOffenceSpellDC = Helpers.CreateBlueprint<BlueprintFeature>(AddScalingSpellDC.BLUEPRINTNAME, bp =>
                {
                    bp.IsClassFeature = true;
                    bp.ReapplyOnLevelUp = true;
                    if (!Main.settings.FeatsAreMythic)
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.WizardFeat, FeatureGroup.Feat };
                    }
                    else
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.MythicFeat };
                    }
                    bp.Ranks = 1;
                    bp.SetName("终极咒力");
                    String stepString = getStepStringWOffset(Main.settings.ScalingSpellDCLevelsPerStep);
                    bp.SetDescription(String.Format("你的魔法技艺令敌人难以招架。你的法术豁免难度获得+{0}加值。{1}级，该数值增加+{0}。",
                        Main.settings.ScalingSpellDCBonusPerStep, stepString));
                    bp.m_DescriptionShort = bp.m_Description;
                    bp.AddComponent(Helpers.Create<AddScalingSpellDC>(c => { }));
                    bp.AddComponent(Helpers.Create<RecommendationRequiresSpellbook>());
                    bp.AddComponent(Helpers.Create<FeatureTagsComponent>(c =>
                    {
                        c.FeatureTags = FeatureTag.Magic;
                    }));
                });

                var ChampionOffenceSpellPen = Helpers.CreateBlueprint<BlueprintFeature>(AddScalingSpellPenetration.BLUEPRINTNAME, bp =>
                {
                    bp.IsClassFeature = true;
                    bp.ReapplyOnLevelUp = true;
                    if (!Main.settings.FeatsAreMythic)
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.WizardFeat, FeatureGroup.Feat };
                    }
                    else
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.MythicFeat };
                    }
                    bp.Ranks = 1;
                    bp.SetName("终极法穿");
                    bp.SetDescription(String.Format("你的魔法技艺经过磨练，足以穿透最严密的防护。你获得法术穿透加值，数值相当于每2个角色等级+{0}（至少+1）。若你拥有“法术穿透”专长，则改为每角色等级+{0}。",
                        Main.settings.ScalingSpellPenBonusPerLevel));
                    bp.m_DescriptionShort = bp.m_Description;
                    bp.AddComponent(Helpers.Create<AddScalingSpellPenetration>(c =>
                    {
                        c.m_SpellPen = SpellPen.ToReference<BlueprintUnitFactReference>();
                    }));
                    bp.AddComponent(Helpers.Create<RecommendationRequiresSpellbook>());
                    bp.AddComponent(Helpers.Create<FeatureTagsComponent>(c =>
                    {
                        c.FeatureTags = FeatureTag.Magic;
                    }));
                });

                var ChampionBombMastery = Helpers.CreateBlueprint<BlueprintFeature>(AddBombDamagePerDie.BLUEPRINTNAME, bp =>
                {
                    bp.IsClassFeature = true;
                    bp.ReapplyOnLevelUp = true;
                    if (!Main.settings.FeatsAreMythic)
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.CombatFeat, FeatureGroup.Feat };
                    }
                    else
                    {
                        bp.Groups = new FeatureGroup[] { FeatureGroup.MythicFeat };
                    }
                    bp.Ranks = 1;
                    bp.SetName("终极爆弹");
                    bp.SetDescription(String.Format("你的炼金炸弹极具破坏力，炸弹的每一个伤害骰都会造成额外的 {0} 点伤害。",
                        Main.settings.BombDamageBonusPerDie));
                    bp.m_DescriptionShort = bp.m_Description;
                    bp.AddComponent(Helpers.Create<AddBombDamagePerDie>());
                    bp.AddComponent(Helpers.Create<FeatureTagsComponent>(c =>
                    {
                        c.FeatureTags = FeatureTag.Damage | FeatureTag.ClassSpecific;
                    }));
                });

                if (!Main.settings.FeatsAreMythic)
                {
                    FeatTools.AddAsFeat(ChampionOffenceSpellPen);
                    FeatTools.AddAsFeat(ChampionOffenceSpellDC);
                    FeatTools.AddAsFeat(ChampionOffenceSpellDam);
                    FeatTools.AddAsFeat(ChampionBombMastery);
                }
                else
                {
                    FeatTools.AddAsMythicFeats(ChampionOffenceSpellPen);
                    FeatTools.AddAsMythicFeats(ChampionOffenceSpellDC);
                    FeatTools.AddAsMythicFeats(ChampionOffenceSpellDam);
                    FeatTools.AddAsMythicFeats(ChampionBombMastery);
                }

            }
        }
    }
}
