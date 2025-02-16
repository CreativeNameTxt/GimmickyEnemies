using GimmickyEnemies;
using GimmickyEnemies.Features;
using Microsoft.Extensions.Logging;
using Nanoray.PluginManager;
using Newtonsoft.Json;
using Nickel;
using System;
using System.Collections.Generic;
using System.Reflection;
using static GimmickyEnemies.External.IKokoroApi.IV2;

namespace GimmickyEnemies.Enemies;

internal sealed class ImperfectImitationEnemy : AI, IRegisterable
{
    private int AiCounter;
    private List<Card> myDeck = new List<Card>();
    private List<int> hand = new List<int>();
    private List<int> exhaust = new List<int>();
    private List<int> discard = new List<int>();
    private List<int> draw = new List<int>();
    List<string> charsSelect = new List<string>();

    /*public class FakeCombat
    {
        public G g;
        public Combat c;
        public State s;
        public FakeCombat(State s, Combat c, G g)
        {
            this.g = Mutil.DeepCopy(g);
            this.c = Mutil.DeepCopy(c);
            this.s = Mutil.DeepCopy(s);
            this.g.state = this.s;
            this.s.route = this.c;
        }

        public void DrainActions()
        {
            int attemptsRemaining = 100;
            while (c.cardActions.Count > 0)
            {
                c.DrainCardActions(g);
                g.time += 1.0;
                attemptsRemaining--;
                if (attemptsRemaining <= 0)
                {
                    break;
                }
            }
            c.DrainCardActions(g);
            g.time += 1.0;
        }

        public float positionEvalEnd()
        {
            if (s.ship.hull <= 0)
            {
                // won
                return float.MaxValue;
            }
            if (c.otherShip.hull <= 0)
            {
                // lost
                return float.MinValue;
            }
            float result = 0;
            // hull points
            result += c.otherShip.hull * 10;
            result -= s.ship.hull * 10;
            // status points
            foreach (KeyValuePair<Status, int> stat in c.otherShip.statusEffects)
            {
                result += (DB.statuses[stat.Key].isGood ? 1 : -1) * stat.Value;
            }
            foreach (KeyValuePair<Status, int> stat in s.ship.statusEffects)
            {
                result -= (DB.statuses[stat.Key].isGood ? 1 : -1) * stat.Value;
            }
            return result;
        }
    }*/

    public static void Register(IPluginPackage<IModManifest> package, IModHelper helper)
    {
        helper.Content.Enemies.RegisterEnemy(new()
        {
            EnemyType = MethodBase.GetCurrentMethod()!.DeclaringType!,
            ShouldAppearOnMap = (_, map) => map is MapLawless ? BattleType.Elite : null,
            Name = ModEntry.Instance.AnyLocalizations.Bind(["enemy", "ImperfectImitationEnemy", "name"]).Localize
        });
    }

    public int GetBaseEnergy()
    {
        return 3;
    }

    public List<Card> GetDeckChar(string charname)
    {
        List<Card> cards = new List<Card>();
        if (charname == "comp")
        {
            cards.Add(new IFrameCard());
            cards.Add(new TemporalAnomalyCard());

            cards.Add(new BigShield());
            cards.Add(new PrepareCard());
            cards.Add(new BigGun());
            cards.Add(new AttackDroneCard());
            cards.Add(new SearCard());
            cards.Add(new SystemSecurity());
            cards.Add(new MedusaField());
        }
        if (charname == "dizzy")
        {
            cards.Add(new AcidCannon());
            cards.Add(new BigShield());
            cards.Add(new BlockShot());
            cards.Add(new Deflection());
            cards.Add(new MitosisCard());
            cards.Add(new MomentumCard());
            cards.Add(new RefreshInterval());
        }
        if (charname == "riggs")
        {
            cards.Add(new PrepareCard());
            cards.Add(new Whiplash());
            cards.Add(new Panic());
            cards.Add(new Juke());
        }
        if (charname == "peri")
        {
            cards.Add(new Barrage());
            cards.Add(new BattleRepairs());
            cards.Add(new Overdrive());
            cards.Add(new Glissade());
            cards.Add(new Lunge());
            cards.Add(new MultiShot());
            cards.Add(new BigGun());
            cards.Add(new Overpower());
            cards.Add(new PowerdriveCard());
            cards.Add(new Sidestep());
        }
        if (charname == "goat")
        {
            cards.Add(new AttackDroneCard());
            cards.Add(new Battalion());
            cards.Add(new MiniMe());
            cards.Add(new MissileLaunchCard());
            cards.Add(new RepairKitCard());
            cards.Add(new ScatterShot());
            cards.Add(new ShieldDroneCard());
            cards.Add(new SpaceMineCard());
            cards.Add(new StrikerSquadron());
        }
        if (charname == "eunice")
        {
            cards.Add(new ExothermicRelease());
            cards.Add(new HESlug());
            cards.Add(new Firewall());
            cards.Add(new FreezeDry());
            cards.Add(new Heatwave());
            cards.Add(new SearCard());
            cards.Add(new ThermalBattery());
            cards.Add(new VolatileVaporCard());
        }
        if (charname == "hacker")
        {
            cards.Add(new BranchPrediction());
            cards.Add(new CleanExhaustCard());
            cards.Add(new LazyBarrage());
            cards.Add(new Overclock());
            cards.Add(new SystemSecurity());
        }
        if (charname == "shard")
        {
            cards.Add(new BloodstoneBolt());
            cards.Add(new MageHand());
            cards.Add(new MedusaField());
            cards.Add(new MagiBattery());
            cards.Add(new OverflowingPower());
            cards.Add(new QuantumQuarryCard());
            cards.Add(new Shardsource());
            cards.Add(new SwizzleShift());
        }
        return cards;
    }

    public void GetDeck(State s)
    {
        List<Card> cards = new List<Card>();

        List<string> charsRemain = new List<string> { "comp", "dizzy", "riggs", "peri", "goat", "eunice", "hacker", "shard" };
        int charcntadd = 0;
        foreach (Character charctr in s.characters)
        {
            // ModEntry.Instance.Logger.Log(LogLevel.Information, charctr.type);
            if (charsRemain.Contains (charctr.type))
            {
                List<Card> cardsTmp = GetDeckChar(charctr.type);
                cards.AddRange(cardsTmp);
                charsSelect.Add(charctr.type);
                charsRemain.Remove(charctr.type);
            }
            else
            {
                charcntadd++;
            }
        }
        for (int i = 0; i < charcntadd; i++)
        {
            if (charsRemain.Count == 0)
            {
                break;
            }
            Rand rng = (s.rngAi);
            string chartmp = charsRemain.Random(rng);
            List<Card> cardsTmp = GetDeckChar(chartmp);
            cards.AddRange(cardsTmp);
            charsSelect.Add(chartmp);
            charsRemain.Remove(chartmp);
        }

        myDeck = cards;
        for (int i = 0; i < myDeck.Count; i++)
        {
            draw.Add(i);
        }
    }

    public override Ship BuildShipForSelf(State s)
    {
        character = new Character
        {
            type = "wasp"
        };
        
        var health = s.GetHarderEnemies() ? 10 : 8;
        return new Ship
        {
            x = 6,
            hull = health,
            hullMax = health,
            shieldMaxBase = 2,
            ai = this,
            baseEnergy = GetBaseEnergy(),
            chassisUnder = "chassis_lawless",
            parts = [
                new()
                {
                    key = "cannon.left",
                    type = PType.cannon,
                    skin = "wing_lawless"
                },
                new()
                {
                    key = "cockpit.left",
                    type = PType.cockpit,
                    skin = "cockpit_lawlessGiant"
                },
                new()
                {
                    key = "cockpit.right",
                    type = PType.cockpit,
                    skin = "cockpit_lawlessGiant",
                    flip = true
                },
                new()
                {
                    key = "cannon.right",
                    type = PType.cannon,
                    skin = "wing_lawless",
                    flip = true
                }
            ]
        };
    }

    public int GetTurnEnergy(Combat c, Ship ship)
    {
        int num = GetBaseEnergy();
        if (ship.Get(Status.energyNextTurn) > 0)
        {
            num += ship.Get(Status.energyNextTurn);
            ship.Set(Status.energyNextTurn, 0);
        }

        if (ship.Get(Status.energyLessNextTurn) > 0)
        {
            num -= ship.Get(Status.energyLessNextTurn);
            ship.Set(Status.energyLessNextTurn, 0);
        }

        if (c.modifier is MBinaryStar mBinaryStar)
        {
            num += (mBinaryStar.isGood ? 1 : (-1));
        }

        return (num > 0) ? num : 0;
    }

    public int GetCardDraw(Combat c, Ship ship)
    {
        int num = 6;
        if (ship.Get(Status.drawNextTurn) > 0)
        {
            num += ship.Get(Status.drawNextTurn);
            ship.Set(Status.drawNextTurn, 0);
        }

        if (ship.Get(Status.drawLessNextTurn) > 0)
        {
            num -= ship.Get(Status.drawLessNextTurn);
            ship.Set(Status.drawLessNextTurn, 0);
        }

        return (num > 0) ? num : 0;
    }

    public int GetCardToDraw(State s)
    {
        if (draw.Count == 0)
        {
            foreach (int i in discard)
            {
                draw.Add(i);
            }
            discard.Clear();
        }
        Rand rng = (s.rngAi);
        return draw.Random(rng);
    }
    public void DrawCard(State s)
    {
        int toDraw = GetCardToDraw(s);
        draw.Remove(toDraw);
        if (hand.Count >= 10)
        {
            discard.Add(toDraw);
        }
        else
        {
            hand.Add(toDraw);
        }
    }

    public override void OnCombatStart(State s, Combat c)
    {
        c.otherShip.Set(ModEntry.Instance.EnergyImitationStatus.Status, GetTurnEnergy(c, c.otherShip));
    }

    public override EnemyDecision PickNextIntent(State s, Combat c, Ship ownShip)
    {
        /*List<string> possibleChars = new List<string> { "wasp", "scrap" };
        Rand rng = s.rngAi;
        character = new Character
        {
            type = possibleChars.Random(rng)
        };*/
        if (myDeck.Count == 0)
        {
            GetDeck(s);
        }
        c.otherShip.Set(ModEntry.Instance.EnergyImitationStatus.Status, GetTurnEnergy(c, c.otherShip)); // get energy
        int cardDrawCnt = GetCardDraw(c, c.otherShip);
        for (int i = 0; i < cardDrawCnt; i++)
        {
            DrawCard(s);
        }
        // FakeCombat fakeC = new FakeCombat(s, c, MG.inst.g);

        List<Intent> intents = [
        ];

        List<int> cardsIntendedToPlay = new List<int>();

        // choose played cards
        int energyPredicted = c.otherShip.Get(ModEntry.Instance.EnergyImitationStatus.Status);
        int shardPredicted = c.otherShip.Get(Status.shard);
        foreach (int i in hand)
        {
            Card cardTmp = myDeck[i];
            int cardCostPredicted = cardTmp.GetCurrentCost(s);
            bool shardCostFine = true;
            int shardCostPredicted = 0;
            foreach (CardAction action in cardTmp.GetActionsOverridden(s, c))
            {
                if (action.shardcost is int shrdcst)
                {
                    shardCostPredicted += shrdcst;
                    if (shardCostPredicted > shardPredicted)
                    {
                        shardCostFine = false;
                    }
                }
                if (action is AStatus aStatus)
                {
                    if (aStatus.status == Status.shard)
                    {
                        shardCostPredicted -= aStatus.statusAmount;
                    }
                }
            }
            if (cardCostPredicted <= energyPredicted && shardCostFine)
            {
                energyPredicted -= cardCostPredicted;
                shardPredicted -= shardCostPredicted;
                cardsIntendedToPlay.Add(i);
                foreach (CardAction action in cardTmp.GetActionsOverridden(s, c))
                {
                    if (action is AEnergy aEnergy)
                    {
                        energyPredicted += aEnergy.changeAmount;
                    }
                }
            }
        }

        // exhaust logic and move to discard logic
        foreach (int i in cardsIntendedToPlay)
        {
            hand.Remove(i);
            if (myDeck[i].GetDataWithOverrides(s).exhaust)
            {
                exhaust.Add(i);
            }
            else
            {
                discard.Add(i);
            }
        }
        int idxHandRemoveAttempt = 0;
        while (hand.Count > idxHandRemoveAttempt)
        {
            if (myDeck[hand[idxHandRemoveAttempt]].GetDataWithOverrides(s).retain)
            {
                idxHandRemoveAttempt++;
            }
            else
            {
                discard.Add(hand[0]);
                hand.RemoveAt(0);
            }
        }

        int n = ownShip.parts.Count;
        List<int> cardCounts = new List<int>();
        for (int i = 0; i < n; i++)
        {
            cardCounts.Add(0);
        }
        int m = cardsIntendedToPlay.Count;
        while (m >= n)
        {
            m -= n;
            for (int i = 0; i < n; i++)
            {
                cardCounts[i]++;
            }
        }
        int k = m / 2;
        for (int i = 0; i < k; i++)
        {
            cardCounts[(n / 2) - i - 1]++;
            cardCounts[(n / 2) + i + (n % 2 == 1 ? 1 : 0)]++;
        }
        if (m % 2 == 1)
        {
            if (n % 2 == 1)
            {
                cardCounts[(n / 2)]++;
            }
            else
            {
                cardCounts[(n / 2) - k - 1]++;
            }
        }
        int i0 = 0;
        for (int i = 0; i < n; i++)
        {
            List<Card> cardsTmp = new List<Card>();
            for (int j = 0; j < cardCounts[i]; j++)
            {
                if (i0 < cardsIntendedToPlay.Count)
                {
                    cardsTmp.Add(myDeck[cardsIntendedToPlay[i0]]);
                    i0++;
                }
            }
            if (cardsTmp.Count > 0)
            {
                IntentCard intentTmp = new IntentCard();
                intentTmp.fromX = i;
                intentTmp.cards = cardsTmp;
                intents.Add(intentTmp);
            }
        }
        AiCounter++;
        return new EnemyDecision
        {
            actions = AIHelpers.MoveToAimAt(s, ownShip, s.ship, "cannon.left"),
            intents = intents
        };
    }
}
