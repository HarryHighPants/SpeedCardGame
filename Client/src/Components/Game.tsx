import copyIcon from '../Assets/copyIcon.png'
import * as signalR from '@microsoft/signalr'
import { useEffect, useState } from 'react'
import { GameState } from '../Models/GameState'
import {Card, CardValue, Pos, Suit} from '../Models/Card'

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
        <div>
            <div>
                <div>
                    <p>{gameState.Players[0].Name}</p>
                    {gameState.Players[0].RequestingTopUp && <p>Requesting top up</p>}
                </div>
                {RenderHandCards(IdleOnly(gameState.Players[0].HandCards))}
            </div>
        </div>
    )
}

const RenderHandCards = (cards: Card[]) => {
    return <div>{cards.map((c) => RenderCard(c))}</div>
}

const RenderCard = (card: Card) => {
    return <img width={80} key={card.Id} src={CardImgSrc(card)} alt={CardImgName(card)} />
}

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
