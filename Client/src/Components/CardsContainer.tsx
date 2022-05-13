import * as signalR from '@microsoft/signalr'
import React, { useEffect, useRef, useState } from 'react'
import { IGameState } from '../Interfaces/IGameState'
import { CardLocationType, IMovedCardPos, IRenderableCard } from '../Interfaces/ICard'
import GameBoardLayout from '../Helpers/GameBoardLayout'
import Card from './Card'
import { AnimatePresence } from 'framer-motion'
import { delay, GetOffsetInfo } from '../Helpers/Utilities'

interface Props {
    connection: signalR.HubConnection | undefined
    playerId: string | undefined | null
    gameState: IGameState
    gameBoardLayout: GameBoardLayout
    onDraggingCardUpdated: (draggingCard: IRenderableCard) => void
    onEndDrag: (draggingCard: IRenderableCard) => void
    sendMovingCard: (movedCard: IMovedCardPos | undefined) => void
    flippedCenterPiles: boolean
}

const CardsContainer = ({
    connection,
    gameBoardLayout,
    playerId,
    gameState,
    onDraggingCardUpdated,
    onEndDrag,
    sendMovingCard,
    flippedCenterPiles,
}: Props) => {
    const [renderableCards, setRenderableCards] = useState<IRenderableCard[]>([] as IRenderableCard[])
    const renderableCardsRef = useRef<IRenderableCard[]>()
    renderableCardsRef.current = renderableCards

    useEffect(() => {
        UpdateRenderableCards()
    }, [gameBoardLayout])

    useEffect(() => {
        UpdateRenderableCards()
    }, [gameState])

    useEffect(() => {
        UpdateRenderableCards()
    }, [playerId])

    const UpdateRenderableCards = () => {
        setRenderableCards(gameBoardLayout.GetRenderableCards(playerId, gameState, renderableCards))
    }

    useEffect(() => {
        if (!connection) return
        connection.on('UpdateMovingCard', UpdateMovingCard)

        return () => {
            connection.off('UpdateMovingCard', UpdateMovingCard)
        }
    }, [connection])

    const UpdateMovingCard = (data: IMovedCardPos) => {
        let movingCard = renderableCardsRef.current?.find((c) => c.id === data.cardId)
        if(!movingCard){
            return "Moving card not found"
        }
        
        movingCard.isCustomPos = data.pos !== null
        let updatedPos = gameBoardLayout.getCardPosPixels(
            !!data.pos
                ? GameBoardLayout.FlipPosition(data.pos)
                : gameBoardLayout.GetCardDefaultPosition(
                      false,
                      movingCard.location,
                    movingCard.location === CardLocationType.Center && flippedCenterPiles
                          ? movingCard.pileIndex === 0
                              ? 1
                              : 0
                          : movingCard.pileIndex
                  )
        )
        movingCard.pos = updatedPos
        movingCard.forceUpdate()
    }

    const SendMovingCardToServer = async (draggingCard: IRenderableCard, endDrag: boolean = false) => {
        let rect = draggingCard?.ref?.current?.getBoundingClientRect()
        let cardIndex = -1
        if (draggingCard.location === CardLocationType.Hand) {
            gameState.players.forEach((p) => {
                if (p.idHash === playerId) {
                    cardIndex = p.handCards.findIndex((c) => c.id === draggingCard.id)
                }
            })
        }
        let movedCard =
            rect !== undefined
                ? ({
                      cardId: draggingCard.id,
                      pos: endDrag
                          ? null
                          : GameBoardLayout.GetCardRectToPercent(rect, gameBoardLayout.gameBoardDimensions),
                  } as IMovedCardPos)
                : undefined
        sendMovingCard(movedCard)
    }

    const DraggingCardUpdated = (draggingCard: IRenderableCard | undefined) => {
        UpdateCardsHoverStates(draggingCard)
        if (!!draggingCard) {
            onDraggingCardUpdated(draggingCard)

            // Send the event to the other player
            SendMovingCardToServer(draggingCard)
        }
    }

    const UpdateCardsHoverStates = (draggingCard: IRenderableCard | undefined) => {
        for (let i = 0; i < renderableCards.length; i++) {
            let card = renderableCards[i]

            if (draggingCard?.id === card.id || !card) continue

            let draggingCardRect = draggingCard?.ref.current?.getBoundingClientRect()
            let ourRect = card.ref?.current?.getBoundingClientRect()
            let offsetInfo = GetOffsetInfo(ourRect, draggingCardRect)

            if (offsetInfo.distance === Infinity) {
                SetOffset(card, 0)
                continue
            }

            // Check if we are a hand card that can be dragged onto
            let droppingOntoHandCard =
                card.ourCard &&
                card.location === CardLocationType.Hand &&
                draggingCard?.location === CardLocationType.Kitty
            if (droppingOntoHandCard) {
                // We want to animate to either the left or the right on the dragged kitty card
                let horizontalOffset = (!!offsetInfo.delta && offsetInfo.delta?.X < 0 ? 1 : 0) * 50
                SetOffset(card, horizontalOffset)
            }
        }
    }

    const SetOffset = (rCard: IRenderableCard, horizontalOffset: number) => {
        if (rCard.horizontalOffset !== horizontalOffset) {
            rCard.horizontalOffset = horizontalOffset
            if (!!rCard.forceUpdate) {
                rCard.forceUpdate()
            }
        }
    }

    const OnEndDrag = (topCard: IRenderableCard) => {
        SendMovingCardToServer(topCard, true)
        DraggingCardUpdated(undefined)
        onEndDrag(topCard)
    }

    return (
        <AnimatePresence>
            {renderableCards.map((c) => (
                <Card key={`card-${c.id}`} card={c} draggingCardUpdated={DraggingCardUpdated} onDragEnd={OnEndDrag} />
            ))}
        </AnimatePresence>
    )
}

export default CardsContainer
