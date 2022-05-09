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
    const renderableCardsRef = useRef()
    // @ts-ignore
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
        connection.on('MovingCardUpdated', UpdateMovingCard)

        return () => {
            connection.off('MovingCardUpdated', UpdateMovingCard)
        }
    }, [connection])

    const UpdateMovingCard = (data: IMovedCardPos) => {
        // @ts-ignore
        let movingCard = renderableCardsRef.current?.find((c) => c.Id === data.CardId)

        movingCard.isCustomPos = data?.Pos !== null
        let updatedPos = gameBoardLayout.getCardPosPixels(
            !!data?.Pos
                ? GameBoardLayout.FlipPosition(data?.Pos)
                : gameBoardLayout.GetCardDefaultPosition(
                      false,
                      data.Location,
                      data.Location === CardLocationType.Center && flippedCenterPiles
                          ? data.Index === 0
                              ? 1
                              : 0
                          : data.Index
                  )
        )
        movingCard.pos = updatedPos
        movingCard.forceUpdate()
    }

    const SendMovingCardToServer = async (draggingCard: IRenderableCard, endDrag: boolean = false) => {
        let rect = draggingCard?.ref?.current?.getBoundingClientRect()
        let cardIndex = -1
        if (draggingCard.location === CardLocationType.Hand) {
            gameState.Players.forEach((p) => {
                if (p.Id === playerId) {
                    cardIndex = p.HandCards.findIndex((c) => c.Id === draggingCard.Id)
                }
            })
        }
        let movedCard =
            rect !== undefined
                ? ({
                      CardId: draggingCard.Id,
                      Pos: endDrag
                          ? null
                          : GameBoardLayout.GetCardRectToPercent(rect, gameBoardLayout.gameBoardDimensions),
                      Location: draggingCard.location,
                      Index: cardIndex,
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

            if (draggingCard?.Id === card.Id || !card) continue

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
                <Card key={`card-${c.Id}`} card={c} draggingCardUpdated={DraggingCardUpdated} onDragEnd={OnEndDrag} />
            ))}
        </AnimatePresence>
    )
}

export default CardsContainer
