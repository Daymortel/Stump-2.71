using System.Globalization;
using Stump.DofusProtocol.Enums;
using Stump.DofusProtocol.Types;
using System.Collections.Generic;
using Stump.Server.WorldServer.Core.Network;
using Stump.Server.WorldServer.Database.Monsters;
using Stump.Server.WorldServer.Database.World;
using Stump.Server.WorldServer.Game.Actors.Stats;
using Stump.Server.WorldServer.Game.Fights.Teams;
using Stump.Server.WorldServer.Game.Maps.Cells;
using Stump.Server.WorldServer.Game.Actors.Interfaces;
using Stump.Server.WorldServer.Game.Spells;
using Stump.Server.WorldServer.Game.Fights;
using Stump.Server.WorldServer.Game.Actors.RolePlay.Characters;
using System;
using System.Linq;

namespace Stump.Server.WorldServer.Game.Actors.Fight
{
    public class SummonedMonster : SummonedFighter, ICreature
    {
        readonly StatsFields m_stats;

        // > Ordem : Zobal, Osamodas, Cra, Ecaflip, Feca, Sram, Iop, Sadida, Enutrof, Sacrier, Pandawa, Xelor, Eniripsa, Huppermago, Steamer, Ladino, Eliotrop, Kilorf
        private List<int> MontersDopplesId = new List<int> { 3136, 2609, 963, 960, 955, 958, 962, 964, 957, 2608, 969, 959, 961, 4312, 3303, 3131, 3990, 4802 };

        List<(int, int)> MontersLifeOsamodaId = new List<(int, int)>
        {
            ((int)MonsterIdEnum.INFORMO_5813, 70),

            ((int)MonsterIdEnum.DRAGONNET_ROUGE_5798, 210),
            ((int)MonsterIdEnum.DRAGONNET_ALBINOS_5800, 150),
            ((int)MonsterIdEnum.DRAGONNET_MLANIQUE_5799, 90),

            ((int)MonsterIdEnum.CRAPAUD_VERDOYANT_5793, 210),
            ((int)MonsterIdEnum.CRAPAUD_ALBINOS_5794, 150),
            ((int)MonsterIdEnum.CRAPAUD_MLANIQUE_5792, 90),

            ((int)MonsterIdEnum.BOUFTOU_CHTAIN_5796, 210),
            ((int)MonsterIdEnum.BOUFTOU_ALBINOS_5797, 150),
            ((int)MonsterIdEnum.BOUFTOU_MLANIQUE_5795, 90),

            ((int)MonsterIdEnum.TOFU_DOR_5791, 210),
            ((int)MonsterIdEnum.TOFU_ALBINOS_5790, 150),
            ((int)MonsterIdEnum.TOFU_MLANIQUE_5789, 90),
        };

        // > Ordem: A Sacrificada, A Inflavel, A Loka, A Bloqueadora, A Superpoderosa, Arvore
        private List<int> MontersSadidaId = new List<int> { 116, 117, 114, 115, 42, 282 };
        // > Ordem: Cofre Enutrof, Cofre Enutrof, Mochila Enutrof, Mochila Variante Enutrof, Mochila Animada
        private List<int> MontersEnutrofId = new List<int> { 285, 5127, 237, 5125 };
        // > Ordem: Pandawasta (Panda), Bambu Variante (Panda), Quadrante de Xelor (Xelor), Cumplice de Xelor (Xelor), Roleta Ecaflip (Ecaflip), Roleta Ecaflip (Ecaflip), Gatinho Enfurecido (Ecaflip), Gatinho Curandeiro (Ecaflip)
        // > Coelho (Eneripsa), Coelho Protetor (Eneripsa), Cão (Kilorf), Sincro (Xelor)
        private List<int> MontersClassesfId = new List<int> { 516, 5137, 3960, 5144, 5189, 5108, 45, 5107, 39, 4759, 4776, 3958 };

        public SummonedMonster(int id, FightTeam team, FightActor summoner, MonsterGrade template, Cell cell) : base(id, team, template.Spells.ToArray(), summoner, cell, template.MonsterId, template)
        {
            Monster = template;
            Look = Monster.Template.EntityLook.Clone();
            m_stats = new StatsFields(this);
            m_stats.Initialize(template);
            AdjustStats();


            if (Monster.Template.Id == (int)MonsterIdEnum.ROULETTE_5189 || Monster.Template.Id == (int)MonsterIdEnum.ROULETTE_5849) //Roleta Classe
            {
                Summoner.Fight.TurnStarted += SpellRoleta;
            }

            if (Monster.Template.Id == (int)MonsterIdEnum.ROULETTE_5108 || Monster.Template.Id == (int)MonsterIdEnum.ROULETTE_5850) //Roleta Variante
            {
                Team.FighterAdded += OnFighterAddedVariantRoleta;
                this.DamageInflicted += DanoRoleta;
            }
        }

        void SpellRoleta(IFight fight, FightActor player)
        {
            if (player != Summoner)
                return;

            if (player.IsFighterTurn())
                CastAutoSpell(new Spell((int)SpellIdEnum.ROULETTE_12900, 1), Cell);
        }

        void OnFighterAddedVariantRoleta(FightTeam team, FightActor actor)
        {
            if (actor != this)
                return;

            CastAutoSpell(new Spell((int)SpellIdEnum.SPELL_STRIKE_12886, 1), Summoner.Cell);
        }

        void DanoRoleta(FightActor fighter, Damage damage)
        {
            if (fighter != this)
                return;
            if (damage.Source == null)
                return;

            CastAutoSpell(new Spell((int)SpellIdEnum.SPELL_STRIKE_12887, 1), damage.Source.Cell);
        }

        void AdjustStats()
        {
            var summonerStats = Summoner.Stats;

            foreach (var summonerStat in summonerStats.Fields)
            {
                if (this.Monster.Template.RaceId != 286)
                {
                    if (summonerStat.Key != PlayerFields.AP && summonerStat.Key != PlayerFields.MP)
                        this.m_stats[summonerStat.Key].Base +=
                            (int)(this.Summoner.Stats.Fields[summonerStat.Key].TotalSafe * 0.5);
                }
                else if (this is SummonedMonster summonedMonster &&
                         summonedMonster.Monster.Template.RaceId == 286)
                {
                    summonedMonster.Stats.AP.Base = summonerStats.AP.TotalMax;
                    summonedMonster.Stats.MP.Base = summonerStats.MP.TotalMax;
                    summonedMonster.Stats.Health.Base = summonerStats.Health.Total;
                    summonedMonster.Stats.Agility.Base = summonerStats.Agility.Total;
                    summonedMonster.Stats.Strength.Base = summonerStats.Strength.Total;
                    summonedMonster.Stats.Chance.Base = summonerStats.Chance.Total;
                    summonedMonster.Stats.Intelligence.Base = summonerStats.Intelligence.Total;
                    summonedMonster.Stats.Wisdom.Base = summonerStats.Wisdom.Total;
                    summonedMonster.Stats[PlayerFields.AirDamageBonus].Base =
                        summonerStats[PlayerFields.AirDamageBonus].Equiped;
                    summonedMonster.Stats[PlayerFields.FireDamageBonus].Base =
                        summonerStats[PlayerFields.FireDamageBonus].Equiped;
                    summonedMonster.Stats[PlayerFields.EarthDamageBonus].Base =
                        summonerStats[PlayerFields.EarthDamageBonus].Equiped;
                    summonedMonster.Stats[PlayerFields.WaterDamageBonus].Base =
                        summonerStats[PlayerFields.WaterDamageBonus].Equiped;
                    summonedMonster.Stats[PlayerFields.NeutralDamageBonus].Base =
                        summonerStats[PlayerFields.NeutralDamageBonus].Equiped;
                    summonedMonster.Stats[PlayerFields.CriticalHit].Base =
                        summonerStats[PlayerFields.CriticalHit].Equiped;
                    summonedMonster.Stats[PlayerFields.CriticalDamageBonus].Base =
                        summonerStats[PlayerFields.CriticalDamageBonus].Equiped;
                    summonedMonster.Stats[PlayerFields.Range].Base = summonerStats[PlayerFields.Range].Equiped +
                                                                     summonedMonster.Stats[PlayerFields.Range].Base;

                    summonedMonster.Stats[PlayerFields.NeutralResistPercent].Base =
                        (int)(summonerStats[PlayerFields.NeutralResistPercent].Equiped * 0.5);
                    summonedMonster.Stats[PlayerFields.WaterResistPercent].Base =
                        (int)(summonerStats[PlayerFields.WaterResistPercent].Equiped * 0.5);
                    summonedMonster.Stats[PlayerFields.AirResistPercent].Base =
                        (int)(summonerStats[PlayerFields.AirResistPercent].Equiped * 0.5);
                    summonedMonster.Stats[PlayerFields.FireResistPercent].Base =
                        (int)(summonerStats[PlayerFields.FireResistPercent].Equiped * 0.5);
                    summonedMonster.Stats[PlayerFields.EarthResistPercent].Base =
                        (int)(summonerStats[PlayerFields.EarthResistPercent].Equiped * 0.5);

                    summonedMonster.Stats[PlayerFields.NeutralElementReduction].Base =
                        (int)(summonerStats[PlayerFields.NeutralElementReduction].Equiped * 0.5);
                    summonedMonster.Stats[PlayerFields.WaterElementReduction].Base =
                        (int)(summonerStats[PlayerFields.WaterElementReduction].Equiped * 0.5);
                    summonedMonster.Stats[PlayerFields.AirElementReduction].Base =
                        (int)(summonerStats[PlayerFields.AirElementReduction].Equiped * 0.5);
                    summonedMonster.Stats[PlayerFields.FireElementReduction].Base =
                        (int)(summonerStats[PlayerFields.FireElementReduction].Equiped * 0.5);
                    summonedMonster.Stats[PlayerFields.EarthElementReduction].Base =
                        (int)(summonerStats[PlayerFields.EarthElementReduction].Equiped * 0.5);
                }
            }
        }

        public override int CalculateArmorValue(int reduction)
        {
            if (Summoner.Level <= 200)
            {
                return (int)(reduction * (100 + 5 * (Summoner.Level)) / 100d);
            }

            return (int)(reduction * (100 + 5 * 200) / 100d);
        }

        public override bool CanPlay() => base.CanPlay() && Monster.Template.CanPlay;

        public override bool CanMove() => base.CanMove() && MonsterGrade.MovementPoints > 0;

        public override bool CanTackle(FightActor fighter) => base.CanTackle(fighter) && Monster.Template.CanTackle;

        public MonsterGrade Monster
        {
            get;
        }

        public override ObjectPosition MapPosition
        {
            get { return Position; }
        }

        public override ushort Level
        {
            get { return (byte)Monster.Level; }
        }

        public override bool Vip
        {
            get { return false; }
        }

        public override RoleEnum Role
        {
            get { return RoleEnum.Player; }
        }

        public override Character Owner => (Summoner as CharacterFighter).Character;

        public MonsterGrade MonsterGrade
        {
            get { return Monster; }
        }

        public override StatsFields Stats
        {
            get { return m_stats; }
        }

        public override string GetMapRunningFighterName()
        {
            return Monster.Id.ToString(CultureInfo.InvariantCulture);
        }

        public override string Name
        {
            get { return Monster.Template.Name; }
        }

        public override bool CanBePushed()
        {
            return base.CanBePushed() && Monster.Template.CanBePushed;
        }

        public override bool CanSwitchPos()
        {
            return base.CanSwitchPos() && Monster.Template.CanSwitchPos;
        }

        //Version 2.61 by Kenshin
        public override GameFightFighterInformations GetGameFightFighterInformations(WorldClient client = null)
        {
            return new GameFightMonsterInformations(
                contextualId: Id,
                look: Look.GetEntityLook(),
                disposition: GetEntityDispositionInformations(),
                spawnInfo: GetGameContextBasicSpawnInformation(client),
                wave: 0,
                stats: GetGameFightMinimalStats(),
                previousPositions: new ushort[0],
                creatureGenericId: (ushort)Monster.MonsterId,
                creatureGrade: (sbyte)Monster.GradeId,
                creatureLevel: (short)Monster.Level);
        }

        public override GameFightFighterLightInformations GetGameFightFighterLightInformations(WorldClient client = null)
        {
            return new GameFightFighterMonsterLightInformations(
                sex: true,
                alive: IsAlive(),
                id: Id,
                wave: 0,
                level: Level,
                breed: (sbyte)BreedEnum.MONSTER,
                creatureGenericId: (ushort)Monster.Template.Id);
        }

        public override GameFightCharacteristics GetGameFightMinimalStats(WorldClient client = null)
        {
            return new GameFightCharacteristics(
                characteristics: new CharacterCharacteristics(GetFightActorCharacteristic()),
                summoner: Summoner.Id,
                summoned: true,
                invisibilityState: (sbyte)(client == null ? VisibleState : GetVisibleStateFor(client.Character)));
        }

        public override FightTeamMemberInformations GetFightTeamMemberInformations()
        {
            return new FightTeamMemberMonsterInformations(id: Id, monsterId: Monster.Template.Id, grade: (sbyte)Monster.GradeId);
        }
    }
}