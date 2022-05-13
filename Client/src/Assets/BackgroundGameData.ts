import { IGameState } from '../Interfaces/IGameState'

export const BackgroundData: IGameState[] = [
    {
        players: [
            {
                idHash: 'Player2',
                name: 'Hary',
                handCards: [
                    { id: 22, suit: 1, cardValue: 9 },
                    { id: 11, suit: 0, cardValue: 11 },
                    { id: 38, suit: 2, cardValue: 12 },
                    { id: 0, suit: 0, cardValue: 0 },
                ],
                topKittyCardId: 6,
                kittyCardsCount: 6,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Hary picked up card Two',
            },
            {
                idHash: 'Player1',
                name: 'Harrowing Hayden',
                handCards: [
                    { id: 35, suit: 2, cardValue: 9 },
                    { id: 9, suit: 0, cardValue: 9 },
                    { id: 12, suit: 0, cardValue: 12 },
                    { id: 21, suit: 1, cardValue: 8 },
                    { id: 32, suit: 2, cardValue: 6 },
                ],
                topKittyCardId: 13,
                kittyCardsCount: 9,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Harrowing Hayden picked up card Eight',
            },
        ],
        centerPiles: [
            {
                cards: [
                    { id: 15, suit: 1, cardValue: 2 },
                    { id: 14, suit: 1, cardValue: 1 },
                    { id: 2, suit: 0, cardValue: 2 },
                ],
            },
            {
                cards: [
                    { id: 17, suit: 1, cardValue: 4 },
                    { id: 3, suit: 0, cardValue: 3 },
                    { id: 43, suit: 3, cardValue: 4 },
                ],
            },
        ],
        lastMove: 'Hary picked up card Two',
        winnerId: undefined,
        mustTopUp: false,
    },
    {
        players: [
            {
                idHash: 'Player2',
                name: 'Hary',
                handCards: [
                    { id: 22, suit: 1, cardValue: 9 },
                    { id: 11, suit: 0, cardValue: 11 },
                    { id: 38, suit: 2, cardValue: 12 },
                    { id: 0, suit: 0, cardValue: 0 },
                    { id: 6, suit: 0, cardValue: 6 },
                ],
                topKittyCardId: 40,
                kittyCardsCount: 5,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Hary picked up card Eight',
            },
            {
                idHash: 'Player1',
                name: 'Harrowing Hayden',
                handCards: [
                    { id: 35, suit: 2, cardValue: 9 },
                    { id: 9, suit: 0, cardValue: 9 },
                    { id: 12, suit: 0, cardValue: 12 },
                    { id: 21, suit: 1, cardValue: 8 },
                    { id: 32, suit: 2, cardValue: 6 },
                ],
                topKittyCardId: 13,
                kittyCardsCount: 9,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Harrowing Hayden picked up card Eight',
            },
        ],
        centerPiles: [
            {
                cards: [
                    { id: 15, suit: 1, cardValue: 2 },
                    { id: 14, suit: 1, cardValue: 1 },
                    { id: 2, suit: 0, cardValue: 2 },
                ],
            },
            {
                cards: [
                    { id: 17, suit: 1, cardValue: 4 },
                    { id: 3, suit: 0, cardValue: 3 },
                    { id: 43, suit: 3, cardValue: 4 },
                ],
            },
        ],
        lastMove: 'Hary picked up card Eight',
        winnerId: undefined,
        mustTopUp: true,
    },
    {
        players: [
            {
                idHash: 'Player2',
                name: 'Hary',
                handCards: [
                    { id: 22, suit: 1, cardValue: 9 },
                    { id: 11, suit: 0, cardValue: 11 },
                    { id: 38, suit: 2, cardValue: 12 },
                    { id: 0, suit: 0, cardValue: 0 },
                    { id: 6, suit: 0, cardValue: 6 },
                ],
                topKittyCardId: 40,
                kittyCardsCount: 5,
                requestingTopUp: false,
                canRequestTopUp: true,
                lastMove: 'Hary picked up card Eight',
            },
            {
                idHash: 'Player1',
                name: 'Harrowing Hayden',
                handCards: [
                    { id: 35, suit: 2, cardValue: 9 },
                    { id: 9, suit: 0, cardValue: 9 },
                    { id: 12, suit: 0, cardValue: 12 },
                    { id: 21, suit: 1, cardValue: 8 },
                    { id: 32, suit: 2, cardValue: 6 },
                ],
                topKittyCardId: 13,
                kittyCardsCount: 9,
                requestingTopUp: true,
                canRequestTopUp: false,
                lastMove: 'Harrowing Hayden requested top up',
            },
        ],
        centerPiles: [
            {
                cards: [
                    { id: 15, suit: 1, cardValue: 2 },
                    { id: 14, suit: 1, cardValue: 1 },
                    { id: 2, suit: 0, cardValue: 2 },
                ],
            },
            {
                cards: [
                    { id: 17, suit: 1, cardValue: 4 },
                    { id: 3, suit: 0, cardValue: 3 },
                    { id: 43, suit: 3, cardValue: 4 },
                ],
            },
        ],
        lastMove: 'Harrowing Hayden requested top up',
        winnerId: undefined,
        mustTopUp: true,
    },
    {
        players: [
            {
                idHash: 'Player2',
                name: 'Hary',
                handCards: [
                    { id: 22, suit: 1, cardValue: 9 },
                    { id: 11, suit: 0, cardValue: 11 },
                    { id: 38, suit: 2, cardValue: 12 },
                    { id: 0, suit: 0, cardValue: 0 },
                    { id: 6, suit: 0, cardValue: 6 },
                ],
                topKittyCardId: 40,
                kittyCardsCount: 5,
                requestingTopUp: false,
                canRequestTopUp: true,
                lastMove: 'Hary requested top up',
            },
            {
                idHash: 'Player1',
                name: 'Harrowing Hayden',
                handCards: [
                    { id: 35, suit: 2, cardValue: 9 },
                    { id: 9, suit: 0, cardValue: 9 },
                    { id: 12, suit: 0, cardValue: 12 },
                    { id: 21, suit: 1, cardValue: 8 },
                    { id: 32, suit: 2, cardValue: 6 },
                ],
                topKittyCardId: 13,
                kittyCardsCount: 9,
                requestingTopUp: false,
                canRequestTopUp: true,
                lastMove: 'Harrowing Hayden requested top up',
            },
        ],
        centerPiles: [
            {
                cards: [
                    { id: 14, suit: 1, cardValue: 1 },
                    { id: 2, suit: 0, cardValue: 2 },
                    { id: 1, suit: 0, cardValue: 1 },
                ],
            },
            {
                cards: [
                    { id: 3, suit: 0, cardValue: 3 },
                    { id: 43, suit: 3, cardValue: 4 },
                    { id: 34, suit: 2, cardValue: 8 },
                ],
            },
        ],
        lastMove: 'Center cards were topped up',
        winnerId: undefined,
        mustTopUp: false,
    },
    {
        players: [
            {
                idHash: 'Player2',
                name: 'Hary',
                handCards: [
                    { id: 22, suit: 1, cardValue: 9 },
                    { id: 11, suit: 0, cardValue: 11 },
                    { id: 38, suit: 2, cardValue: 12 },
                    { id: 0, suit: 0, cardValue: 0 },
                    { id: 6, suit: 0, cardValue: 6 },
                ],
                topKittyCardId: 40,
                kittyCardsCount: 5,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Hary requested top up',
            },
            {
                idHash: 'Player1',
                name: 'Harrowing Hayden',
                handCards: [
                    { id: 9, suit: 0, cardValue: 9 },
                    { id: 12, suit: 0, cardValue: 12 },
                    { id: 21, suit: 1, cardValue: 8 },
                    { id: 32, suit: 2, cardValue: 6 },
                ],
                topKittyCardId: 13,
                kittyCardsCount: 9,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Harrowing Hayden played card Jack onto Ten',
            },
        ],
        centerPiles: [
            {
                cards: [
                    { id: 14, suit: 1, cardValue: 1 },
                    { id: 2, suit: 0, cardValue: 2 },
                    { id: 1, suit: 0, cardValue: 1 },
                ],
            },
            {
                cards: [
                    { id: 43, suit: 3, cardValue: 4 },
                    { id: 34, suit: 2, cardValue: 8 },
                    { id: 35, suit: 2, cardValue: 9 },
                ],
            },
        ],
        lastMove: 'Harrowing Hayden played card Jack onto Ten',
        winnerId: undefined,
        mustTopUp: false,
    },
    {
        players: [
            {
                idHash: 'Player2',
                name: 'Hary',
                handCards: [
                    { id: 22, suit: 1, cardValue: 9 },
                    { id: 11, suit: 0, cardValue: 11 },
                    { id: 38, suit: 2, cardValue: 12 },
                    { id: 0, suit: 0, cardValue: 0 },
                    { id: 6, suit: 0, cardValue: 6 },
                ],
                topKittyCardId: 40,
                kittyCardsCount: 5,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Hary requested top up',
            },
            {
                idHash: 'Player1',
                name: 'Harrowing Hayden',
                handCards: [
                    { id: 9, suit: 0, cardValue: 9 },
                    { id: 12, suit: 0, cardValue: 12 },
                    { id: 32, suit: 2, cardValue: 6 },
                ],
                topKittyCardId: 13,
                kittyCardsCount: 9,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Harrowing Hayden played card Ten onto Jack',
            },
        ],
        centerPiles: [
            {
                cards: [
                    { id: 14, suit: 1, cardValue: 1 },
                    { id: 2, suit: 0, cardValue: 2 },
                    { id: 1, suit: 0, cardValue: 1 },
                ],
            },
            {
                cards: [
                    { id: 34, suit: 2, cardValue: 8 },
                    { id: 35, suit: 2, cardValue: 9 },
                    { id: 21, suit: 1, cardValue: 8 },
                ],
            },
        ],
        lastMove: 'Harrowing Hayden played card Ten onto Jack',
        winnerId: undefined,
        mustTopUp: false,
    },
    {
        players: [
            {
                idHash: 'Player2',
                name: 'Hary',
                handCards: [
                    { id: 22, suit: 1, cardValue: 9 },
                    { id: 11, suit: 0, cardValue: 11 },
                    { id: 38, suit: 2, cardValue: 12 },
                    { id: 6, suit: 0, cardValue: 6 },
                ],
                topKittyCardId: 40,
                kittyCardsCount: 5,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Hary played card Two onto Three',
            },
            {
                idHash: 'Player1',
                name: 'Harrowing Hayden',
                handCards: [
                    { id: 9, suit: 0, cardValue: 9 },
                    { id: 12, suit: 0, cardValue: 12 },
                    { id: 32, suit: 2, cardValue: 6 },
                ],
                topKittyCardId: 13,
                kittyCardsCount: 9,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Harrowing Hayden played card Ten onto Jack',
            },
        ],
        centerPiles: [
            {
                cards: [
                    { id: 2, suit: 0, cardValue: 2 },
                    { id: 1, suit: 0, cardValue: 1 },
                    { id: 0, suit: 0, cardValue: 0 },
                ],
            },
            {
                cards: [
                    { id: 34, suit: 2, cardValue: 8 },
                    { id: 35, suit: 2, cardValue: 9 },
                    { id: 21, suit: 1, cardValue: 8 },
                ],
            },
        ],
        lastMove: 'Hary played card Two onto Three',
        winnerId: undefined,
        mustTopUp: false,
    },
    {
        players: [
            {
                idHash: 'Player1',
                name: 'Harrowing Hayden',
                handCards: [
                    { id: 12, suit: 0, cardValue: 12 },
                    { id: 32, suit: 2, cardValue: 6 },
                ],
                topKittyCardId: 13,
                kittyCardsCount: 9,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Harrowing Hayden played card Jack onto Ten',
            },
            {
                idHash: 'Player2',
                name: 'Hary',
                handCards: [
                    { id: 22, suit: 1, cardValue: 9 },
                    { id: 11, suit: 0, cardValue: 11 },
                    { id: 38, suit: 2, cardValue: 12 },
                    { id: 6, suit: 0, cardValue: 6 },
                ],
                topKittyCardId: 40,
                kittyCardsCount: 5,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Hary played card Two onto Three',
            },
        ],
        centerPiles: [
            {
                cards: [
                    { id: 35, suit: 2, cardValue: 9 },
                    { id: 21, suit: 1, cardValue: 8 },
                    { id: 9, suit: 0, cardValue: 9 },
                ],
            },
            {
                cards: [
                    { id: 2, suit: 0, cardValue: 2 },
                    { id: 1, suit: 0, cardValue: 1 },
                    { id: 0, suit: 0, cardValue: 0 },
                ],
            },
        ],
        lastMove: 'Harrowing Hayden played card Jack onto Ten',
        winnerId: undefined,
        mustTopUp: false,
    },
    {
        players: [
            {
                idHash: 'Player2',
                name: 'Hary',
                handCards: [
                    { id: 22, suit: 1, cardValue: 9 },
                    { id: 11, suit: 0, cardValue: 11 },
                    { id: 6, suit: 0, cardValue: 6 },
                ],
                topKittyCardId: 40,
                kittyCardsCount: 5,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Hary played card Ace onto Two',
            },
            {
                idHash: 'Player1',
                name: 'Harrowing Hayden',
                handCards: [
                    { id: 12, suit: 0, cardValue: 12 },
                    { id: 32, suit: 2, cardValue: 6 },
                ],
                topKittyCardId: 13,
                kittyCardsCount: 9,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Harrowing Hayden played card Jack onto Ten',
            },
        ],
        centerPiles: [
            {
                cards: [
                    { id: 1, suit: 0, cardValue: 1 },
                    { id: 0, suit: 0, cardValue: 0 },
                    { id: 38, suit: 2, cardValue: 12 },
                ],
            },
            {
                cards: [
                    { id: 35, suit: 2, cardValue: 9 },
                    { id: 21, suit: 1, cardValue: 8 },
                    { id: 9, suit: 0, cardValue: 9 },
                ],
            },
        ],
        lastMove: 'Hary played card Ace onto Two',
        winnerId: undefined,
        mustTopUp: false,
    },
    {
        players: [
            {
                idHash: 'Player2',
                name: 'Hary',
                handCards: [
                    { id: 22, suit: 1, cardValue: 9 },
                    { id: 6, suit: 0, cardValue: 6 },
                ],
                topKittyCardId: 40,
                kittyCardsCount: 5,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Hary played card King onto Ace',
            },
            {
                idHash: 'Player1',
                name: 'Harrowing Hayden',
                handCards: [
                    { id: 12, suit: 0, cardValue: 12 },
                    { id: 32, suit: 2, cardValue: 6 },
                ],
                topKittyCardId: 13,
                kittyCardsCount: 9,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Harrowing Hayden played card Jack onto Ten',
            },
        ],
        centerPiles: [
            {
                cards: [
                    { id: 0, suit: 0, cardValue: 0 },
                    { id: 38, suit: 2, cardValue: 12 },
                    { id: 11, suit: 0, cardValue: 11 },
                ],
            },
            {
                cards: [
                    { id: 35, suit: 2, cardValue: 9 },
                    { id: 21, suit: 1, cardValue: 8 },
                    { id: 9, suit: 0, cardValue: 9 },
                ],
            },
        ],
        lastMove: 'Hary played card King onto Ace',
        winnerId: undefined,
        mustTopUp: false,
    },
    {
        players: [
            {
                idHash: 'Player2',
                name: 'Hary',
                handCards: [
                    { id: 22, suit: 1, cardValue: 9 },
                    { id: 6, suit: 0, cardValue: 6 },
                ],
                topKittyCardId: 40,
                kittyCardsCount: 5,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Hary played card King onto Ace',
            },
            {
                idHash: 'Player1',
                name: 'Harrowing Hayden',
                handCards: [
                    { id: 12, suit: 0, cardValue: 12 },
                    { id: 32, suit: 2, cardValue: 6 },
                    { id: 13, suit: 1, cardValue: 0 },
                ],
                topKittyCardId: 8,
                kittyCardsCount: 8,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Harrowing Hayden picked up card Two',
            },
        ],
        centerPiles: [
            {
                cards: [
                    { id: 0, suit: 0, cardValue: 0 },
                    { id: 38, suit: 2, cardValue: 12 },
                    { id: 11, suit: 0, cardValue: 11 },
                ],
            },
            {
                cards: [
                    { id: 35, suit: 2, cardValue: 9 },
                    { id: 21, suit: 1, cardValue: 8 },
                    { id: 9, suit: 0, cardValue: 9 },
                ],
            },
        ],
        lastMove: 'Harrowing Hayden picked up card Two',
        winnerId: undefined,
        mustTopUp: false,
    },
    {
        players: [
            {
                idHash: 'Player2',
                name: 'Hary',
                handCards: [
                    { id: 22, suit: 1, cardValue: 9 },
                    { id: 6, suit: 0, cardValue: 6 },
                    { id: 40, suit: 3, cardValue: 1 },
                ],
                topKittyCardId: 18,
                kittyCardsCount: 4,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Hary picked up card Three',
            },
            {
                idHash: 'Player1',
                name: 'Harrowing Hayden',
                handCards: [
                    { id: 12, suit: 0, cardValue: 12 },
                    { id: 32, suit: 2, cardValue: 6 },
                    { id: 13, suit: 1, cardValue: 0 },
                ],
                topKittyCardId: 8,
                kittyCardsCount: 8,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Harrowing Hayden picked up card Two',
            },
        ],
        centerPiles: [
            {
                cards: [
                    { id: 0, suit: 0, cardValue: 0 },
                    { id: 38, suit: 2, cardValue: 12 },
                    { id: 11, suit: 0, cardValue: 11 },
                ],
            },
            {
                cards: [
                    { id: 35, suit: 2, cardValue: 9 },
                    { id: 21, suit: 1, cardValue: 8 },
                    { id: 9, suit: 0, cardValue: 9 },
                ],
            },
        ],
        lastMove: 'Hary picked up card Three',
        winnerId: undefined,
        mustTopUp: false,
    },
    {
        players: [
            {
                idHash: 'Player2',
                name: 'Hary',
                handCards: [
                    { id: 22, suit: 1, cardValue: 9 },
                    { id: 6, suit: 0, cardValue: 6 },
                    { id: 40, suit: 3, cardValue: 1 },
                ],
                topKittyCardId: 18,
                kittyCardsCount: 4,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Hary picked up card Three',
            },
            {
                idHash: 'Player1',
                name: 'Harrowing Hayden',
                handCards: [
                    { id: 32, suit: 2, cardValue: 6 },
                    { id: 13, suit: 1, cardValue: 0 },
                ],
                topKittyCardId: 8,
                kittyCardsCount: 8,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Harrowing Hayden played card Ace onto King',
            },
        ],
        centerPiles: [
            {
                cards: [
                    { id: 38, suit: 2, cardValue: 12 },
                    { id: 11, suit: 0, cardValue: 11 },
                    { id: 12, suit: 0, cardValue: 12 },
                ],
            },
            {
                cards: [
                    { id: 35, suit: 2, cardValue: 9 },
                    { id: 21, suit: 1, cardValue: 8 },
                    { id: 9, suit: 0, cardValue: 9 },
                ],
            },
        ],
        lastMove: 'Harrowing Hayden played card Ace onto King',
        winnerId: undefined,
        mustTopUp: false,
    },
    {
        players: [
            {
                idHash: 'Player2',
                name: 'Hary',
                handCards: [
                    { id: 22, suit: 1, cardValue: 9 },
                    { id: 6, suit: 0, cardValue: 6 },
                    { id: 40, suit: 3, cardValue: 1 },
                    { id: 18, suit: 1, cardValue: 5 },
                ],
                topKittyCardId: 5,
                kittyCardsCount: 3,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Hary picked up card Seven',
            },
            {
                idHash: 'Player1',
                name: 'Harrowing Hayden',
                handCards: [
                    { id: 32, suit: 2, cardValue: 6 },
                    { id: 13, suit: 1, cardValue: 0 },
                ],
                topKittyCardId: 8,
                kittyCardsCount: 8,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Harrowing Hayden played card Ace onto King',
            },
        ],
        centerPiles: [
            {
                cards: [
                    { id: 38, suit: 2, cardValue: 12 },
                    { id: 11, suit: 0, cardValue: 11 },
                    { id: 12, suit: 0, cardValue: 12 },
                ],
            },
            {
                cards: [
                    { id: 35, suit: 2, cardValue: 9 },
                    { id: 21, suit: 1, cardValue: 8 },
                    { id: 9, suit: 0, cardValue: 9 },
                ],
            },
        ],
        lastMove: 'Hary picked up card Seven',
        winnerId: undefined,
        mustTopUp: false,
    },
    {
        players: [
            {
                idHash: 'Player2',
                name: 'Hary',
                handCards: [
                    { id: 22, suit: 1, cardValue: 9 },
                    { id: 6, suit: 0, cardValue: 6 },
                    { id: 40, suit: 3, cardValue: 1 },
                    { id: 18, suit: 1, cardValue: 5 },
                    { id: 5, suit: 0, cardValue: 5 },
                ],
                topKittyCardId: 46,
                kittyCardsCount: 2,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Hary picked up card Seven',
            },
            {
                idHash: 'Player1',
                name: 'Harrowing Hayden',
                handCards: [
                    { id: 32, suit: 2, cardValue: 6 },
                    { id: 13, suit: 1, cardValue: 0 },
                ],
                topKittyCardId: 8,
                kittyCardsCount: 8,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Harrowing Hayden played card Ace onto King',
            },
        ],
        centerPiles: [
            {
                cards: [
                    { id: 38, suit: 2, cardValue: 12 },
                    { id: 11, suit: 0, cardValue: 11 },
                    { id: 12, suit: 0, cardValue: 12 },
                ],
            },
            {
                cards: [
                    { id: 35, suit: 2, cardValue: 9 },
                    { id: 21, suit: 1, cardValue: 8 },
                    { id: 9, suit: 0, cardValue: 9 },
                ],
            },
        ],
        lastMove: 'Hary picked up card Seven',
        winnerId: undefined,
        mustTopUp: false,
    },
    {
        players: [
            {
                idHash: 'Player2',
                name: 'Hary',
                handCards: [
                    { id: 22, suit: 1, cardValue: 9 },
                    { id: 6, suit: 0, cardValue: 6 },
                    { id: 40, suit: 3, cardValue: 1 },
                    { id: 18, suit: 1, cardValue: 5 },
                    { id: 5, suit: 0, cardValue: 5 },
                ],
                topKittyCardId: 46,
                kittyCardsCount: 2,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Hary picked up card Seven',
            },
            {
                idHash: 'Player1',
                name: 'Harrowing Hayden',
                handCards: [{ id: 32, suit: 2, cardValue: 6 }],
                topKittyCardId: 8,
                kittyCardsCount: 8,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Harrowing Hayden played card Two onto Ace',
            },
        ],
        centerPiles: [
            {
                cards: [
                    { id: 11, suit: 0, cardValue: 11 },
                    { id: 12, suit: 0, cardValue: 12 },
                    { id: 13, suit: 1, cardValue: 0 },
                ],
            },
            {
                cards: [
                    { id: 35, suit: 2, cardValue: 9 },
                    { id: 21, suit: 1, cardValue: 8 },
                    { id: 9, suit: 0, cardValue: 9 },
                ],
            },
        ],
        lastMove: 'Harrowing Hayden played card Two onto Ace',
        winnerId: undefined,
        mustTopUp: false,
    },
    {
        players: [
            {
                idHash: 'Player2',
                name: 'Hary',
                handCards: [
                    { id: 22, suit: 1, cardValue: 9 },
                    { id: 6, suit: 0, cardValue: 6 },
                    { id: 18, suit: 1, cardValue: 5 },
                    { id: 5, suit: 0, cardValue: 5 },
                ],
                topKittyCardId: 46,
                kittyCardsCount: 2,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Hary played card Three onto Two',
            },
            {
                idHash: 'Player1',
                name: 'Harrowing Hayden',
                handCards: [{ id: 32, suit: 2, cardValue: 6 }],
                topKittyCardId: 8,
                kittyCardsCount: 8,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Harrowing Hayden played card Two onto Ace',
            },
        ],
        centerPiles: [
            {
                cards: [
                    { id: 12, suit: 0, cardValue: 12 },
                    { id: 13, suit: 1, cardValue: 0 },
                    { id: 40, suit: 3, cardValue: 1 },
                ],
            },
            {
                cards: [
                    { id: 35, suit: 2, cardValue: 9 },
                    { id: 21, suit: 1, cardValue: 8 },
                    { id: 9, suit: 0, cardValue: 9 },
                ],
            },
        ],
        lastMove: 'Hary played card Three onto Two',
        winnerId: undefined,
        mustTopUp: false,
    },
    {
        players: [
            {
                idHash: 'Player1',
                name: 'Harrowing Hayden',
                handCards: [
                    { id: 32, suit: 2, cardValue: 6 },
                    { id: 8, suit: 0, cardValue: 8 },
                ],
                topKittyCardId: 36,
                kittyCardsCount: 7,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Harrowing Hayden picked up card Ten',
            },
            {
                idHash: 'Player2',
                name: 'Hary',
                handCards: [
                    { id: 22, suit: 1, cardValue: 9 },
                    { id: 6, suit: 0, cardValue: 6 },
                    { id: 18, suit: 1, cardValue: 5 },
                    { id: 5, suit: 0, cardValue: 5 },
                ],
                topKittyCardId: 46,
                kittyCardsCount: 2,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Hary played card Three onto Two',
            },
        ],
        centerPiles: [
            {
                cards: [
                    { id: 35, suit: 2, cardValue: 9 },
                    { id: 21, suit: 1, cardValue: 8 },
                    { id: 9, suit: 0, cardValue: 9 },
                ],
            },
            {
                cards: [
                    { id: 12, suit: 0, cardValue: 12 },
                    { id: 13, suit: 1, cardValue: 0 },
                    { id: 40, suit: 3, cardValue: 1 },
                ],
            },
        ],
        lastMove: 'Harrowing Hayden picked up card Ten',
        winnerId: undefined,
        mustTopUp: false,
    },
    {
        players: [
            {
                idHash: 'Player2',
                name: 'Hary',
                handCards: [
                    { id: 22, suit: 1, cardValue: 9 },
                    { id: 6, suit: 0, cardValue: 6 },
                    { id: 18, suit: 1, cardValue: 5 },
                    { id: 5, suit: 0, cardValue: 5 },
                    { id: 46, suit: 3, cardValue: 7 },
                ],
                topKittyCardId: 16,
                kittyCardsCount: 1,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Hary picked up card Nine',
            },
            {
                idHash: 'Player1',
                name: 'Harrowing Hayden',
                handCards: [
                    { id: 32, suit: 2, cardValue: 6 },
                    { id: 8, suit: 0, cardValue: 8 },
                ],
                topKittyCardId: 36,
                kittyCardsCount: 7,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Harrowing Hayden picked up card Ten',
            },
        ],
        centerPiles: [
            {
                cards: [
                    { id: 12, suit: 0, cardValue: 12 },
                    { id: 13, suit: 1, cardValue: 0 },
                    { id: 40, suit: 3, cardValue: 1 },
                ],
            },
            {
                cards: [
                    { id: 35, suit: 2, cardValue: 9 },
                    { id: 21, suit: 1, cardValue: 8 },
                    { id: 9, suit: 0, cardValue: 9 },
                ],
            },
        ],
        lastMove: 'Hary picked up card Nine',
        winnerId: undefined,
        mustTopUp: false,
    },
    {
        players: [
            {
                idHash: 'Player2',
                name: 'Hary',
                handCards: [
                    { id: 22, suit: 1, cardValue: 9 },
                    { id: 6, suit: 0, cardValue: 6 },
                    { id: 18, suit: 1, cardValue: 5 },
                    { id: 5, suit: 0, cardValue: 5 },
                    { id: 46, suit: 3, cardValue: 7 },
                ],
                topKittyCardId: 16,
                kittyCardsCount: 1,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Hary picked up card Nine',
            },
            {
                idHash: 'Player1',
                name: 'Harrowing Hayden',
                handCards: [{ id: 32, suit: 2, cardValue: 6 }],
                topKittyCardId: 36,
                kittyCardsCount: 7,
                requestingTopUp: false,
                canRequestTopUp: false,
                lastMove: 'Harrowing Hayden played card Ten onto Jack',
            },
        ],
        centerPiles: [
            {
                cards: [
                    { id: 12, suit: 0, cardValue: 12 },
                    { id: 13, suit: 1, cardValue: 0 },
                    { id: 40, suit: 3, cardValue: 1 },
                ],
            },
            {
                cards: [
                    { id: 21, suit: 1, cardValue: 8 },
                    { id: 9, suit: 0, cardValue: 9 },
                    { id: 8, suit: 0, cardValue: 8 },
                ],
            },
        ],
        lastMove: 'Harrowing Hayden played card Ten onto Jack',
        winnerId: undefined,
        mustTopUp: false,
    },
]
