using UnityModManagerNet;

namespace ChampionFeats
{
    public class Settings : UnityModManager.ModSettings
    {

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            UnityModManager.ModSettings.Save<Settings>(this, modEntry);
        }


        public bool FeatsAreMythic;

        // feat customization settings
        // Champion Protection
        public int ScalingACLevelsPerStep = 5;
        public int ScalingACArmorBonusLightPerStep = 1;
        public int ScalingACArmorBonusMediumPerStep = 2;
        public int ScalingACArmorBonusHeavyPerStep = 3;
        // Champion Guard
        public int ScalingDRLevelsPerStep = 5;
        public int ScalingDRBonusPerStep = 5;
        // Champion Saves
        public int ScalingSaveLevelsPerStep = 1;
        public int ScalingSaveBonusPerLevel = 2;
        // Champion Skills
        public bool AddAsSkillRanks = true;
        public int ScalingSkillsLevelsPerStep = 2;
        public int ScalingSkillsBonusPerLevel = 1;
        // Champion Aim
        public int ScalingABLevelsPerStep = 2;
        public int ScalingABBonusPerStep = 1;
        // Champion Strikes
        public int ScalingDamageLevelsPerStep = 2;
        public int ScalingDamageBonusPerStep = 1;
        // Champion Spell Blasts
        public int ScalingSpellDamageLevelsPerStep = 3;
        public int ScalingSpellDamageBonusPerStep = 1;
        // Champion Spell Force
        public int ScalingSpellDCLevelsPerStep = 1;
        public int ScalingSpellDCBonusPerStep = 1;
        // Champion Spell Penetration
        public int ScalingSpellPenBonusPerLevel = 1;
        // Champion Bomb
        public int ScalingBombDamageBonusPerDie = 1;
    }

}
