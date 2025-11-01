using System.Linq;
using NLog;
using Stump.Core.Attributes;
using Stump.Core.Reflection;
using Stump.DofusProtocol.Enums;
using Stump.Server.BaseServer.Initialization;
using Stump.Server.WorldServer.Database.Items.Templates;
using Stump.Server.WorldServer.Game.Effects.Instances;
using Stump.Server.WorldServer.Game.Items;
using Stump.Server.WorldServer.Database.Characters;

namespace Stump.Server.WorldServer.Game.Actors.RolePlay.Characters
{
    public class PrestigeManager : Singleton<PrestigeManager>
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public const int ItemForBonus = 14230;

        public static ItemTemplate BonusItem;

        [Variable]
        public static short[] PrestigeTitles =
        {
            527, 528, 529, 530, 531, 532, 533, 534, 535, 536
        };

        private static readonly EffectInteger[][] m_prestigesBonus =
        {
            new[] {new EffectInteger(EffectsEnum.Effect_AddDamageBonus, 2)},
            new[] {new EffectInteger(EffectsEnum.Effect_AddVitality, 25)},
            new[] {
                new EffectInteger(EffectsEnum.Effect_AddChance, 20),
                new EffectInteger(EffectsEnum.Effect_AddIntelligence, 20),
                new EffectInteger(EffectsEnum.Effect_AddWisdom, 20),
                new EffectInteger(EffectsEnum.Effect_AddAgility, 20),
                new EffectInteger(EffectsEnum.Effect_AddStrength, 20)
            },
            new[] {new EffectInteger(EffectsEnum.Effect_AddDamageBonus, 3)},
            new[] {new EffectInteger(EffectsEnum.Effect_AddVitality, 50)},
            new[] {
                new EffectInteger(EffectsEnum.Effect_AddChance, 30),
                new EffectInteger(EffectsEnum.Effect_AddIntelligence, 30),
                new EffectInteger(EffectsEnum.Effect_AddWisdom, 30),
                new EffectInteger(EffectsEnum.Effect_AddAgility, 30),
                new EffectInteger(EffectsEnum.Effect_AddStrength, 30)
            },
            new[] {new EffectInteger(EffectsEnum.Effect_AddDamageBonus, 5)},
            new[] {new EffectInteger(EffectsEnum.Effect_IncreaseDamage_138, 20)},
            new[] {new EffectInteger(EffectsEnum.Effect_AddDamageBonus, 10)},
            new[] {
                new EffectInteger(EffectsEnum.Effect_AddAirElementReduction, 10),
                new EffectInteger(EffectsEnum.Effect_AddEarthElementReduction, 10),
                new EffectInteger(EffectsEnum.Effect_AddFireElementReduction, 10),
                new EffectInteger(EffectsEnum.Effect_AddWaterElementReduction, 10),
                new EffectInteger(EffectsEnum.Effect_AddNeutralElementReduction, 10)
            },
            new[] {new EffectInteger(EffectsEnum.Effect_IncreaseDamage_138, 20)},
            new[] {new EffectInteger(EffectsEnum.Effect_AddVitality, 50)},
            new[] {new EffectInteger(EffectsEnum.Effect_AddCriticalHit, 7)},
            new[] {new EffectInteger(EffectsEnum.Effect_AddRange, 1)},
            new[] {new EffectInteger(EffectsEnum.Effect_AddMP_128, 1)},
        };

        // Tableau des IDs des auras en fonction des rangs de prestige
        private static readonly int[] PrestigeAuraIds =
        {
            1,  // Aura ID pour Rang 1
            2,  // Aura ID pour Rang 2
            3,  // Aura ID pour Rang 3
            4,  // Aura ID pour Rang 4
            5,  // Aura ID pour Rang 5
            6,  // Aura ID pour Rang 6
            7,  // Aura ID pour Rang 7
            8,  // Aura ID pour Rang 8
            9,  // Aura ID pour Rang 9
            10, // Aura ID pour Rang 10
            11, // Aura ID pour Rang 11
            12, // Aura ID pour Rang 12
            13  // Aura ID pour Rang 13
        };

        private bool m_disabled;

        public bool PrestigeEnabled
        {
            get { return !m_disabled; }
        }

        [Initialization(typeof(ItemManager), Silent = true)]
        public void Initialize()
        {
            BonusItem = ItemManager.Instance.TryGetTemplate(ItemForBonus);

            if (BonusItem != null)
                return;

            logger.Error("Item {0} not found, prestiges disabled", ItemForBonus);
            m_disabled = true;
        }

        public EffectInteger[] GetPrestigeEffects(int rank)
        {
            return m_prestigesBonus.Take(rank).SelectMany(x => x.Select(y => (EffectInteger)y.Clone())).ToArray();
        }

        public short GetPrestigeTitle(int rank)
        {
            return PrestigeTitles[rank - 1];
        }

        // Méthode pour appliquer l'AuraId au personnage dans la base de données
        public void ApplyAura(Character character, int rank)
        {
            if (rank > 0 && rank <= PrestigeAuraIds.Length)
            {
                int auraId = PrestigeAuraIds[rank - 1];

                // Appliquer l'AuraId au personnage
                character.SetAuraId(auraId);

                // Log que l'aura a été appliquée
                logger.Info("Aura ID {0} appliqué au personnage {1}", auraId, character.Name);
            }
            else
            {
                logger.Warn("Aura ID pour le rang {0} non trouvé !", rank);
            }
        }
    }
}
