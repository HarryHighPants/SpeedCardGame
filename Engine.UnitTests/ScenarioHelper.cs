using System;
using System.Collections.Generic;

namespace Engine.UnitTests;

public class ScenarioHelper
{
    public static GameState defaultGameState = new()
    {
        CenterPiles = new List<List<Card>>
        {
            new() {new Card {Id = 0, Suit = Suit.Hearts, Value = 0}},
            new() {new Card {Id = 1, Suit = Suit.Hearts, Value = 1}}
        },
        Players = new List<Player>
        {
            new()
            {
                Id = 0, Name = "Player 1", HandCards = new List<Card>
                {
                    new() {Id = 2, Suit = Suit.Hearts, Value = 2},
                    new() {Id = 3, Suit = Suit.Hearts, Value = 12}
                },
                KittyCards = new List<Card>
                {
                    new() {Id = 6, Suit = Suit.Hearts, Value = 3},
                    new() {Id = 7, Suit = Suit.Hearts, Value = 11}
                },
                TopUpCards = new List<Card>
                {
                    new() {Id = 12, Suit = Suit.Hearts, Value = 10},
                    new() {Id = 13, Suit = Suit.Hearts, Value = 9}
                }
            },
            new()
            {
                Id = 0, Name = "Player 2", HandCards = new List<Card>
                {
                    new() {Id = 4, Suit = Suit.Clubs, Value = 2},
                    new() {Id = 5, Suit = Suit.Clubs, Value = 12}
                },
                KittyCards = new List<Card>
                {
                    new() {Id = 8, Suit = Suit.Clubs, Value = 3},
                    new() {Id = 9, Suit = Suit.Clubs, Value = 11}
                },
                TopUpCards = new List<Card>
                {
                    new() {Id = 10, Suit = Suit.Clubs, Value = 10},
                    new() {Id = 11, Suit = Suit.Clubs, Value = 9}
                }
            }
        },
        Settings = new Settings {RandomSeed = 0}
    };

    public static GameState CreateGameBasic(int? centerCard1, int? centerCard2 = null, int? player1Card = null,
        int? player2Card = null,
        int? player1Kitty = null,
        int? player2Kitty = null, int? player1TopUp = null, int? player2TopUp = null,
        bool player1RequestingTopUp = false,
        bool player2RequestingTopUp = false)
    {
        return CreateGameCustom(new List<int?> {centerCard1}, new List<int?> {centerCard2},
            new List<int?> {player1Card}, new List<int?> {player1Kitty}, new List<int?> {player1TopUp},
            player1RequestingTopUp,
            new List<int?> {player2Card}, new List<int?> {player2Kitty}, new List<int?> {player2TopUp},
            player2RequestingTopUp);
    }

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
        bool player2RequestingTopup = false)
    {
        return new GameState
        {
            Players = new List<Player>
            {
                CreateBasicPlayer("Player 1", player1Cards, player1Kittys, player1TopUps, player1RequestingTopup),
                CreateBasicPlayer("Player 2", player2Cards, player2Kittys, player2TopUps, player2RequestingTopup)
            },
            CenterPiles = new List<List<Card>>
            {
                CreateBasicCards(centerPile1),
                CreateBasicCards(centerPile2)
            }
        };
    }

    private static Player CreateBasicPlayer(string? name = null, List<int?>? hand = null, List<int?>? kitty = null,
        List<int?>? topUp = null, bool requestingTopUp = false)
    {
        return new Player
        {
            Id = GetRandomId(), Name = name, HandCards = CreateBasicCards(hand), KittyCards = CreateBasicCards(kitty),
            TopUpCards = CreateBasicCards(topUp), RequestingTopUp = requestingTopUp
        };
    }

    private static List<Card> CreateBasicCards(List<int?>? values)
    {
        var cards = new List<Card>();
        values?.ForEach(v =>
        {
            if (v != null) cards.Add(CreateBasicCard((int) v));
        });
        return cards;
    }

    private static Card CreateBasicCard(int value)
    {
        if (value == null) return new Card();
        return new Card {Id = GetRandomId(), Value = value, Suit = Suit.Clubs};
    }

    private static int GetRandomId()
    {
        return Guid.NewGuid().GetHashCode();
    }
}