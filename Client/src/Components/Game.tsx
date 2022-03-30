import copyIcon from '../Assets/copyIcon.png'
import * as signalR from '@microsoft/signalr'
import { useEffect, useState } from 'react'
import { IGameState } from '../Interfaces/IGameState'
import { ICard, CardValue, IPos, Suit } from '../Interfaces/ICard'
import Draggable, { DraggableData } from 'react-draggable'
import styled from 'styled-components'
import React from 'react'
import { IPlayer } from '../Interfaces/IPlayer'
import RenderPlayer from './Player'
import Player from './Player'

interface Props {
    connection: signalR.HubConnection | undefined
    connectionId: string | undefined | null
    roomId: string | undefined
    gameState: IGameState
}

const Game = ({ connection, connectionId, gameState, roomId }: Props) => {
    const [movedCards, setMovedCards] = useState<IMovedCardPos[]>([])

    useEffect(() => {
        if (!connection) return
        connection.on('CardMoved', CardMoved)

        return () => {
            connection.off('CardMoved', CardMoved)
        }
    }, [connection])

    const CardMoved = (data: any) => {
        let parsedData: IMovedCardPos = JSON.parse(data)

        let existingMovingCard = movedCards.find((c) => c.cardId === parsedData.cardId)
        if (existingMovingCard) {
            existingMovingCard.pos = parsedData.pos
        } else {
            movedCards.push(parsedData)
        }
        setMovedCards([...movedCards])
    }

    const IdleOnly = (cards: ICard[]) => {
        return cards.filter((c) => !movedCards.find((mc) => mc.cardId === c.Id))
    }

    return (
        <Board>
            <Player movedCards={movedCards} player={gameState.Players[0]} />
            <Player movedCards={movedCards} player={gameState.Players[1]} />
        </Board>
    )
}

const Board = styled.div`
    background-color: #729bf5;
    width: 100%;
    height: 100%;
    max-width: 600px;
    user-select: none;
`

export interface IMovedCardPos {
    cardId: number
    pos: IPos
}

export default Game
