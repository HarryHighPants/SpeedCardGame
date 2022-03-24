namespace Engine.UnitTests;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Models;

public class ModelGenerator
{
    public static GameState CreateGameBasic(int? centerCard1, int? centerCard2 = null, int? player1Card = null,
        int? player2Card = null,
        int? player1Kitty = null,
        int? player2Kitty = null, int? player1TopUp = null, int? player2TopUp = null,
        bool player1RequestingTopUp = false,
        bool player2RequestingTopUp = false) =>
        CreateGameCustom(new List<int?> {centerCard1}, new List<int?> {centerCard2},
            new List<int?> {player1Card}, new List<int?> {player1Kitty}, new List<int?> {player1TopUp},
            player1RequestingTopUp,
            new List<int?> {player2Card}, new List<int?> {player2Kitty}, new List<int?> {player2TopUp},
            player2RequestingTopUp);

    public static GameState CreateGameCustom(
        List<int?>? centerPile1 = null,
        List<int?>? centerPile2 = null,
        List<int?>? player1Cards = null,
        List<int?>? player1Kittys = null,
        List<int?>? player1TopUps = null,
        bool player1RequestingTopup = false,
        List<int?>? player2Cards = null,
        List<int?>? player2Kittys = null,
        List<int?>? player2TopUps = null,
        bool player2RequestingTopup = false) =>
        new()
        {
            Players = new List<Player>
            {
                CreateBasicPlayer("Player 1", player1Cards, player1Kittys, player1TopUps,
                    player1RequestingTopup),
                CreateBasicPlayer("Player 2", player2Cards, player2Kittys, player2TopUps,
                    player2RequestingTopup)
            }.ToImmutableList(),
            CenterPiles =
                new List<CenterPile>
                    {
                        new() {Cards = CreateBasicCards(centerPile1)}, new() {Cards = CreateBasicCards(centerPile2)}
                    }
                    .ToImmutableList(),
            MoveHistory = ImmutableList<Move>.Empty
        };

    private static Player CreateBasicPlayer(string name = "Player", List<int?>? hand = null, List<int?>? kitty = null,
        List<int?>? topUp = null, bool requestingTopUp = false) =>
        new()
        {
            Id = GetRandomId(),
            Name = name,
            HandCards = CreateBasicCards(hand),
            KittyCards = CreateBasicCards(kitty),
            TopUpCards = CreateBasicCards(topUp),
            RequestingTopUp = requestingTopUp
        };

    private static ImmutableList<Card> CreateBasicCards(List<int?>? values)
    {
        var cards = new List<Card>();
        values?.ForEach(v =>
        {
            if (v != null)
            {
                cards.Add(CreateBasicCard((int)v));
            }
        });
        return cards.ToImmutableList();
    }

    public static Card CreateBasicCard(int value)
    {
        if (value == null)
        {
            return new Card();
        }

        return new Card {Id = GetRandomId(), CardValue = (CardValue)value, Suit = Suit.Clubs};
    }

    private static int GetRandomId() => Guid.NewGuid().GetHashCode();
}
