using System;
using ChampionFeats.Config;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;

namespace ChampionFeats.Components
{
    [ComponentName("Feat/Bomb Damage Per Die Bonus")]
    [AllowedOn(typeof(Kingmaker.Blueprints.Facts.BlueprintUnitFact), false)]
    [TypeId("d34d51c516644ed284e8d87599153f0a")] 
    public class AddBombDamagePerDie : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, IInitiatorRulebookSubscriber
    {
        public void OnEventAboutToTrigger(RuleCalculateDamage evt)
        {
            if (evt.Reason.Weapon == null || evt.Reason.Weapon.Blueprint.Category != WeaponCategory.Bomb)
            {
                return; // 如果不是武器伤害，或者武器不是炸弹，直接跳过
            }
            if (Blueprints.HasNPCImmortalityBuff(base.Fact.Owner))
      			{
      				return;
      			}
            
            int bonusPerDie = Main.settings.BombDamageBonusPerDie; 
            if (bonusPerDie <= 0) return; // 如果玩家在菜单里设为0，则不触发

            foreach (BaseDamage baseDamage in evt.DamageBundle)
            {
                // 如果这个伤害包没有掷骰子（比如单纯的固定属性伤害），跳过
                if (baseDamage.Dice.BaseFormula.Dice == Kingmaker.RuleSystem.DiceType.Zero)
                {
                    continue;
                }

                int rolls = baseDamage.Dice.BaseFormula.m_Rolls;
                
                int totalBonus = rolls * bonusPerDie;

                baseDamage.AddModifier(totalBonus, Fact);
            }
        }

        public void OnEventDidTrigger(RuleCalculateDamage evt)
        {
        }

        public const string BLUEPRINTNAME = "RMChampionFeatBombDM";
    }
}
