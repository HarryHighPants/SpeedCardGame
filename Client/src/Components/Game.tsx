import copyIcon from '../Assets/copyIcon.png'
import * as signalR from '@microsoft/signalr'
import { useEffect, useState } from 'react'
import { IGameState } from '../Interfaces/IGameState'
import { ICard, CardValue, IPos, Suit, IRenderableCard } from '../Interfaces/ICard'
import Draggable, { DraggableData } from 'react-draggable'
import styled from 'styled-components'
import React from 'react'
import { IPlayer } from '../Interfaces/IPlayer'
import RenderPlayer from './Player'
import Player from './Player'
import BoardLayout from '../Helpers/GameBoardLayout'

interface Props {
    connection: signalR.HubConnection | undefined
    connectionId: string | undefined | null
    roomId: string | undefined
    gameState: IGameState
}

interface BoardDimensions {
    connection: signalR.HubConnection | undefined
    connectionId: string | undefined | null
    roomId: string | undefined
    gameState: IGameState
}

const gameBoardInfo = {
    maxWidth: 1200,
}

// Clamp number between two values with the following line:
const clamp = (num: number, min: number, max: number) => Math.min(Math.max(num, min), max)

const Game = ({ connection, connectionId, gameState, roomId }: Props) => {
    const [movedCards, setMovedCards] = useState<IMovedCardPos[]>([])
    const [renderableCards, setRenderableCards] = useState<IRenderableCard[]>([])
    const [gameBoardDimensions, setGameBoardDimensions] = useState<IPos>({ x: 600, y: 700 })

    useEffect(() => {
        setGameBoardDimensions(UpdateGameBoardDimensions())
    }, [window.innerWidth])

    useEffect(() => {
        if (!connection) return
        connection.on('CardMoved', CardMoved)

        return () => {
            connection.off('CardMoved', CardMoved)
        }
    }, [connection])

    useEffect(() => {
        UpdateRenderableCards()
    }, [gameState])

    const CardMoved = (data: any) => {
        let parsedData: IMovedCardPos = JSON.parse(data)

        let existingMovingCard = movedCards.find((c) => c.cardId === parsedData.cardId)
        if (existingMovingCard) {
            existingMovingCard.pos = parsedData.pos
        } else {
            movedCards.push(parsedData)
        }
        setMovedCards([...movedCards])
        UpdateRenderableCards()
    }

    const UpdateRenderableCards = () => {
        let ourId = connectionId ?? 'CUqUsFYm1zVoW-WcGr6sUQ'
        let newRenderableCards = [] as IRenderableCard[]

        // Add players cards
        gameState.Players.map((p, i) => {
            let ourPlayer = p.Id == ourId

            // Add the players hand cards
            let handCardPositions = BoardLayout.GetHandCardPositions(i)
            let handCards = p.HandCards.map((hc, cIndex) => {
              // We only want to override the position of cards that aren't ours
                let movedCard = ourPlayer ? null : movedCards.find((c) => c.cardId === hc.Id)
                return {
                    ...hc,
                    draggable: ourPlayer,
                    droppableTarget: false,
                    pos: movedCard?.pos ?? handCardPositions[cIndex],
                } as IRenderableCard
            })
            newRenderableCards.push(...handCards)

            // Add the players Kitty card
            let kittyCardPosition = BoardLayout.GetKittyCardPosition(i)
            let kittyCard = {
                Id: p.TopKittyCardId,
                draggable: ourPlayer,
                droppableTarget: false,
                pos: kittyCardPosition,
            } as IRenderableCard
            newRenderableCards.push(kittyCard)
        })

        // Add the center piles
        let centerCardPositions = BoardLayout.GetCenterCardPositions()
        let centerPilePositions = gameState.CenterPiles.map((cp, cpIndex) => {
            return {
                ...cp,
                draggable: false,
                droppableTarget: true,
                pos: centerCardPositions[cpIndex],
            } as IRenderableCard
        })
        newRenderableCards.push(...centerPilePositions)
    }

    const IdleOnly = (cards: ICard[]) => {
        return cards.filter((c) => !movedCards.find((mc) => mc.cardId === c.Id))
    }

    return (
        <Board>
            <Player
                key={gameState.Players[0].Id}
                movedCards={movedCards}
                player={gameState.Players[0]}
                gameBoardDimensions={gameBoardDimensions}
            />
            <Player
                key={gameState.Players[1].Id}
                movedCards={movedCards}
                player={gameState.Players[1]}
                gameBoardDimensions={gameBoardDimensions}
            />
        </Board>
    )
}

const UpdateGameBoardDimensions = (): IPos => {
    return {
        x: clamp(window.innerWidth, 0, gameBoardInfo.maxWidth),
        y: window.innerHeight,
    }
}

const Board = styled.div`
    background-color: #729bf5;
    width: 100%;
    height: 100%;
    max-width: ${gameBoardInfo.maxWidth};
    //user-select: none;
`

export interface IMovedCardPos {
    cardId: number
    pos: IPos
}

export default Game
