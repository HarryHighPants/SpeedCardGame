import copyIcon from '../Assets/copyIcon.png'
import * as signalR from '@microsoft/signalr'
import { useEffect, useState } from 'react'
import { GameState } from '../Models/GameState'
import { Card, CardValue, Pos, Suit } from '../Models/Card'
import Draggable, { DraggableData } from 'react-draggable'
import styled from 'styled-components'
import React from 'react'
import { Player } from '../Models/Player'

interface Props {
    connection: signalR.HubConnection | undefined
    connectionId: string | undefined | null
    roomId: string | undefined
    gameState: GameState
}

const Game = ({ connection, connectionId, gameState, roomId }: Props) => {
    const [movedCards, setMovedCards] = useState<MovedCardPos[]>([])

    useEffect(() => {
        if (!connection) return
        connection.on('CardMoved', CardMoved)

        return () => {
            connection.off('CardMoved', CardMoved)
        }
    }, [connection])

    const CardMoved = (data: any) => {
        let parsedData: MovedCardPos = JSON.parse(data)

        let existingMovingCard = movedCards.find((c) => c.cardId === parsedData.cardId)
        if (existingMovingCard) {
            existingMovingCard.pos = parsedData.pos
        } else {
            movedCards.push(parsedData)
        }
        setMovedCards([...movedCards])
    }

    const IdleOnly = (cards: Card[]) => {
        return cards.filter((c) => !movedCards.find((mc) => mc.cardId === c.Id))
    }

    return (
        <Board>
            <RenderPlayer player={gameState.Players[0]} />
        </Board>
    )
}

// When rendering cards we need a
// A: Give them the cards from state and moved cards
// B:
// Player has a callback for on moveCard and onPlayCard

const RenderPlayer = (player: Player) => {
    return (
        <div>
            <div>
                <p>{player.Name}</p>
                {player.RequestingTopUp && <p>Requesting top up</p>}
            </div>
            {RenderHandCards(IdleOnly(player.HandCards))}
        </div>
    )
}

const RenderHandCards = (cards: Card[]) => {
    return <div>{cards.map((c) => RenderCard(c))}</div>
}

const RenderCard = (card: Card) => {
    const [position, setPosition] = useState({ x: 0, y: 0 })
    const trackPos = (data: DraggableData) => {
        setPosition({ x: data.x, y: data.y })
    }

    const nodeRef = React.useRef(null)
    return (
        <Draggable key={card.Id} nodeRef={nodeRef} onDrag={(e, data) => trackPos(data)}>
            <CardParent ref={nodeRef}>
                <CardElement>
                    <img draggable="false" width={80} key={card.Id} src={CardImgSrc(card)} alt={CardImgName(card)} />
                </CardElement>
                <div>
                    x: {position.x.toFixed(0)}, y: {position.y.toFixed(0)}
                </div>
            </CardParent>
        </Draggable>
    )
}

const Board = styled.div`
    background-color: #729bf5;
    width: 100%;
    height: 100%;
    max-width: 600px;
    user-select: none;
`

const CardParent = styled.div`
    background-color: #282c34;
    width: 80px;
    cursor: pointer;
    user-select: none;
`

const CardElement = styled.div``

const CardImgSrc = (card: Card) => {
    return `/Cards/${CardImgName(card)}.png`
}

const CardImgName = (card: Card) => {
    let valueName = CardValue[card.CardValue]
    if (card.CardValue < 9) {
        valueName = (card.CardValue + 2).toString()
    }
    valueName = valueName.toLowerCase()
    return `${valueName}_of_${Suit[card.Suit].toLowerCase()}`
}

export interface MovedCardPos {
    cardId: number
    pos: Pos
}

export default Game
