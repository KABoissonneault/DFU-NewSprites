using System.Linq;

using UnityEngine;
using UnityEditor;

using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.Questing;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Items;

namespace NewSpritesMod
{
    public class NewSprites : MonoBehaviour
    {
        private static Mod mod;

        public const int DruidCareerIndex = 147;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;

            var go = new GameObject(mod.Title);
            go.AddComponent<NewSprites>();

            mod.IsReady = true;
        }

        private void Awake()
        {
            QuestMachine.Instance.FoesTable.AddIntoTable(new string[] { "147, Druid" });

            ArrayUtility.Add(ref EnemyBasics.Enemies, new MobileEnemy()
            {
                ID = DruidCareerIndex,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 1000,
                FemaleTexture = 1001,
                CorpseTexture = EnemyBasics.CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = false,
                MapChance = 3,
                LootTableKey = "U",
                Weight = 350,
                CastsMagic = true,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 3, 2, 1, 0, -1, 5, 4, 0 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 0, 1, -1, 3, 2, 1, 0 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 0, -1, 5, 4, 0 },
                HasSpellAnimation = true,
                SpellAnimFrames = new int[] { 0, 1, 2, 3, 3 },
                Team = MobileTeams.Bears,
            });

            DaggerfallEntity.CustomCareerTemplates.Add(DruidCareerIndex, new DFCareer()
            {
                Name = "Druid",
                HitPointsPerLevel = 6,
                Strength = 50,
                Intelligence = 55,
                Willpower = 60,
                Agility = 40,
                Endurance = 60,
                Personality = 40,
                Speed = 40,
                Luck = 55,
                PrimarySkill1 = DFCareer.Skills­.Mysticism,
                PrimarySkill2 = DFCareer.Skills.Restoration,
                PrimarySkill3 = DFCareer.Skills.Destruction,
                MajorSkill1 = DFCareer.Skills.BluntWeapon,
                MajorSkill2 = DFCareer.Skills.Centaurian,
                MajorSkill3 = DFCareer.Skills.Spriggan,
                MinorSkill1 = DFCareer.Skills.Nymph,
                MinorSkill2 = DFCareer.Skills.Medical,
                MinorSkill3 = DFCareer.Skills.ShortBlade,
                MinorSkill4 = DFCareer.Skills.Stealth,
                MinorSkill5 = DFCareer.Skills.Thaumaturgy,
                LongBlades = DFCareer.Proficiency.Forbidden,
                ForbiddenMaterials = DFCareer.MaterialFlags.Steel | DFCareer.MaterialFlags.Elven | DFCareer.MaterialFlags.Dwarven | DFCareer.MaterialFlags.Orcish,
                ForbiddenShields = DFCareer.ShieldFlags.TowerShield | DFCareer.ShieldFlags.KiteShield | DFCareer.ShieldFlags.RoundShield,
                ForbiddenArmors = DFCareer.ArmorFlags.Plate | DFCareer.ArmorFlags.Chain,
                ForbiddenProficiencies = DFCareer.ProficiencyFlags.LongBlades,
                SpellPointMultiplier = DFCareer.SpellPointMultipliers.Times_2_00,
                SpellPointMultiplierValue = 2.0f,
                AcuteHearing = true,
            });

            EnemyEntity.OnLootSpawned += OnEnemySpawned;
        }

        void OnEnemySpawned(object sender, EnemyLootSpawnedEventArgs args)
        {
            var enemyEntity = sender as EnemyEntity;
            if (enemyEntity == null)
                return;

            if (enemyEntity.CareerIndex == DruidCareerIndex)
            {
                SetupDruid(enemyEntity, args);
            }            
        }

        static byte[] OrcShamanSpells = { 0x06, 0x07, 0x16, 0x19, 0x1F };
        static byte[] FrostDaedraSpells = { 0x10, 0x14 };
        static byte[] DaedrothSpells = { 0x16, 0x17, 0x1F };
        static byte[] VampireAncientSpells = { 0x08, 0x32 };
        static byte[] DaedraLordSpells = { 0x08, 0x0A, 0x0E, 0x3C, 0x43 };
        static byte[] LichSpells = { 0x08, 0x0A, 0x0E, 0x22, 0x3C };
        static byte[] AncientLichSpells = { 0x08, 0x0A, 0x0E, 0x1D, 0x1F, 0x22, 0x3C };
        static byte[][] EnemyClassSpells = { FrostDaedraSpells, DaedrothSpells, OrcShamanSpells, VampireAncientSpells, DaedraLordSpells, LichSpells, AncientLichSpells };

        void SetupDruid(EnemyEntity enemyEntity, EnemyLootSpawnedEventArgs args)
        {
            var career = args.EnemyCareer;

            var player = GameManager.Instance.PlayerEntity;
            var level = player.Level;

            enemyEntity.Level = level;
            enemyEntity.MaxHealth = FormulaHelper.RollEnemyClassMaxHealth(level, career.HitPointsPerLevel);

            var equipRoll = Random.Range(0, 100);

            int spellTable;

            if(equipRoll < 50)
            {
                DaggerfallUnityItem weapon = ItemBuilder.CreateWeapon(Weapons.Staff, RandomDruidWeaponMaterial(level));
                enemyEntity.ItemEquipTable.EquipItem(weapon, alwaysEquip: true, playEquipSounds: false);
                enemyEntity.Items.AddItem(weapon);

                spellTable = Mathf.Clamp(level / 3 + 2, 0, 6);
            }
            else if(equipRoll < 80)
            {
                DaggerfallUnityItem weapon = ItemBuilder.CreateWeapon(Weapons.Dagger, RandomDruidWeaponMaterial(level));
                enemyEntity.ItemEquipTable.EquipItem(weapon, alwaysEquip: true, playEquipSounds: false);
                enemyEntity.Items.AddItem(weapon);

                AddDruidArmor(enemyEntity, Armor.Cuirass);
                AddDruidArmor(enemyEntity, Armor.Boots);

                spellTable = Mathf.Clamp(level / 3 + 1, 0, 6);
            }
            else
            {
                DaggerfallUnityItem weapon = ItemBuilder.CreateWeapon(Weapons.Battle_Axe, RandomDruidWeaponMaterial(level));
                enemyEntity.ItemEquipTable.EquipItem(weapon, alwaysEquip: true, playEquipSounds: false);
                enemyEntity.Items.AddItem(weapon);

                DaggerfallUnityItem shield = ItemBuilder.CreateArmor(player.Gender, player.Race, Armor.Buckler, RandomDruidShieldMaterial(level));
                enemyEntity.ItemEquipTable.EquipItem(shield, alwaysEquip: true, playEquipSounds: false);
                enemyEntity.Items.AddItem(shield);
                enemyEntity.UpdateEquippedArmorValues(shield, true);

                AddDruidArmor(enemyEntity, Armor.Cuirass);
                AddDruidArmor(enemyEntity, Armor.Greaves);
                AddDruidArmor(enemyEntity, Armor.Boots);

                spellTable = Mathf.Clamp(level / 3, 0, 6);
            }

            enemyEntity.SetEnemySpells(EnemyClassSpells[spellTable]);
        }

        WeaponMaterialTypes RandomDruidWeaponMaterial(int level)
        {
            WeaponMaterialTypes type = FormulaHelper.RandomMaterial(level);
            switch(type)
            {
                case WeaponMaterialTypes.Steel:
                    return WeaponMaterialTypes.Iron;
                case WeaponMaterialTypes.Elven:
                case WeaponMaterialTypes.Dwarven:
                    return WeaponMaterialTypes.Silver;
                case WeaponMaterialTypes.Orcish:
                    return WeaponMaterialTypes.Ebony;
                default:
                    return type;
            }
        }

        ArmorMaterialTypes RandomDruidShieldMaterial(int level)
        {
            ArmorMaterialTypes type = FormulaHelper.RandomArmorMaterial(level);
            switch (type)
            {
                case ArmorMaterialTypes.Steel:
                    return ArmorMaterialTypes.Iron;
                case ArmorMaterialTypes.Elven:
                case ArmorMaterialTypes.Dwarven:
                    return ArmorMaterialTypes.Silver;
                case ArmorMaterialTypes.Orcish:
                    return ArmorMaterialTypes.Ebony;
                default:
                    return type;
            }
        }

        void AddDruidArmor(EnemyEntity enemyEntity, Armor armor)
        {
            var player = GameManager.Instance.PlayerEntity;

            DaggerfallUnityItem item = ItemBuilder.CreateArmor(player.Gender, player.Race, armor, ArmorMaterialTypes.Leather);
            enemyEntity.ItemEquipTable.EquipItem(item, alwaysEquip: true, playEquipSounds: false);
            enemyEntity.Items.AddItem(item);
            enemyEntity.UpdateEquippedArmorValues(item, equipping: true);
        }
    }
}
