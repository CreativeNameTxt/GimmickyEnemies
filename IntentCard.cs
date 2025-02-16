using GimmickyEnemies;
using Microsoft.Extensions.Logging;
using Nickel;
using System;
using System.Collections.Generic;

namespace GimmickyEnemies.Features;

public class IntentCard : Intent
{
    public List<Card> cards = new List<Card>();

    public override string GetSingleTooltip(State s, Combat c, Ship fromShip)
    {
        return ModEntry.Instance.Localizations.Localize(["intent", "ICard", "desc"]);
    }

    public override List<Tooltip>? GetTooltips(State s, Combat c, Ship fromShip)
    {
        List<Tooltip> list = new List<Tooltip>
        {
            new GlossaryTooltip("ICard")
            {
                Description = GetSingleTooltip(s, c, fromShip)
            }
        };
        foreach (Card card in cards)
        {
            list.Add(new TTCard { card = card });
        }
        return list;
    }

    public override void Apply(State s, Combat c, Ship fromShip, int actualX)
    {
        int energy = fromShip.Get(ModEntry.Instance.EnergyImitationStatus.Status);
        foreach (Card card in cards)
        {
            int cost = card.GetCurrentCost(s);
            bool canPlay = true;
            if (cost > energy)
            {
                canPlay = false;
            }
            if (fromShip.Get(StatusMeta.deckToMissingStatus[card.GetMeta().deck]) > 0)
            {
                c.QueueImmediate(new AStatus
                {
                    status = StatusMeta.deckToMissingStatus[card.GetMeta().deck],
                    statusAmount = -1,
                    targetPlayer = fromShip.isPlayerShip
                });
                canPlay = false;
            }
            if (!canPlay)
            {
                fromShip.shake = 1;
            }
            else
            {
                List<CardAction> actions = new List<CardAction>();
                actions.Add(new AStatus
                {
                    status = ModEntry.Instance.EnergyImitationStatus.Status,
                    statusAmount = -cost,
                    targetPlayer = fromShip.isPlayerShip
                });
                foreach (CardAction action in card.GetActionsOverridden(s, c))
                {
                    bool canMakeAction = true;
                    if (action.shardcost is int shrdcst)
                    {
                        int shardhave = fromShip.Get(Status.shard);
                        if (shardhave >= shrdcst)
                        {
                            actions.Add(new AStatus
                            {
                                status = Status.shard,
                                statusAmount = -shrdcst,
                                targetPlayer = fromShip.isPlayerShip
                            });
                        }
                        else
                        {
                            canMakeAction = false;
                        }
                        action.shardcost = 0;
                    }
                    if (canMakeAction)
                    {
                        if (action is AAttack aAttack)
                        {
                            aAttack.targetPlayer ^= true;
                            actions.Add(aAttack);
                        }
                        else if (action is AStatus aStatus)
                        {
                            aStatus.targetPlayer ^= true;
                            actions.Add(aStatus);
                        }
                        else if (action is AMove aMove)
                        {
                            aMove.targetPlayer ^= true;
                            actions.Add(aMove);
                        }
                        else if (action is ASpawn aSpawn)
                        {
                            aSpawn.fromPlayer ^= true;
                            actions.Add(aSpawn);
                        }
                        else if (action is AMedusaField aMedusaField)
                        {
                            actions.Add(aMedusaField);
                        }
                        else if (action is AEnergy aEnergy)
                        {
                            actions.Add(new AStatus
                            {
                                status = ModEntry.Instance.EnergyImitationStatus.Status,
                                statusAmount = aEnergy.changeAmount,
                                targetPlayer = fromShip.isPlayerShip
                            });
                        }
                        else if (action is AHurt aHurt)
                        {
                            aHurt.targetPlayer ^= true;
                            actions.Add(aHurt);
                        }
                        else if (action is AHeal aHeal)
                        {
                            aHeal.targetPlayer ^= true;
                            actions.Add(aHeal);
                        }
                        else if (action is AEndTurn aEndTurn)
                        {
                            break;
                        }
                        else
                        {
                            ModEntry.Instance.Logger.Log(LogLevel.Information, "Unknown action triggered!");
                            ModEntry.Instance.Logger.Log(LogLevel.Information, action.Key());
                        }
                    }
                }
                c.QueueImmediate(actions);
            }
        }
    }

    public override Spr? GetSprite(State s)
    {
        return ModEntry.Instance.intentCardSpr;
    }
}
