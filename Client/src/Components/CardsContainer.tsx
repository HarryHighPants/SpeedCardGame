import * as signalR from '@microsoft/signalr'
import React, { useEffect, useLayoutEffect, useState } from 'react'
import { IGameState } from '../Interfaces/IGameState'
import { CardLocationType, ICard, IMovedCardPos, IPos, IRenderableCard } from '../Interfaces/ICard'
import styled from 'styled-components'
import Player from './Player'
import GameBoardLayout from '../Helpers/GameBoardLayout'
import Card from './Card'
import { AnimatePresence, LayoutGroup, PanInfo } from 'framer-motion'
import { clamp, GetDistanceRect, GetOffsetInfo, Overlaps } from '../Helpers/Utilities'
import { IPlayer } from '../Interfaces/IPlayer'
import gameBoardLayout from '../Helpers/GameBoardLayout'
import GameBoardAreas from './GameBoardAreas/GameBoardAreas'
import { debounce } from 'lodash'
import { IRenderableArea } from '../Interfaces/IBoardArea'

interface Props {
	connection: signalR.HubConnection | undefined
	playerId: string | undefined | null
	gameState: IGameState
	gameBoardLayout: GameBoardLayout
	onDraggingCardUpdated: (draggingCard: IRenderableCard) => void
	onEndDrag: (draggingCard: IRenderableCard) => void
	sendMovingCard: (movedCard: IMovedCardPos | undefined) => void
}

const CardsContainer = ({
	connection,
	gameBoardLayout,
	playerId,
	gameState,
	onDraggingCardUpdated,
	onEndDrag,
	sendMovingCard,
}: Props) => {
	const [movedCard, setMovedCard] = useState<IMovedCardPos>()
	const [renderableCards, setRenderableCards] = useState<IRenderableCard[]>([] as IRenderableCard[])

	useEffect(() => {
		UpdateRenderableCards()
	}, [gameBoardLayout])

	useEffect(() => {
		UpdateRenderableCards()
	}, [gameState])

	useEffect(() => {
		UpdateRenderableCards()
	}, [playerId])

	// todo: update just the moved card
	useEffect(() => {
		UpdateRenderableCards()
	}, [movedCard])

	const UpdateRenderableCards = () => {
		setRenderableCards(gameBoardLayout.GetRenderableCards(playerId, gameState, renderableCards, movedCard))
	}

	useEffect(() => {
		if (!connection) return
		connection.on('MovingCardUpdated', UpdateMovingCard)

		return () => {
			connection.off('MovingCardUpdated', UpdateMovingCard)
		}
	}, [connection])

	const UpdateMovingCard = (data: any) => {
		let parsedData: IMovedCardPos = JSON.parse(data)
		setMovedCard(parsedData)
	}

	const DraggingCardUpdated = (draggingCard: IRenderableCard | undefined) => {
		UpdateCardsHoverStates(draggingCard)
		// Send the event to the other player
		SendMovingCardToServer(draggingCard)
		if (!!draggingCard) {
			onDraggingCardUpdated(draggingCard)
		}
	}

	const SendMovingCardToServer = debounce(
		(draggingCard: IRenderableCard | undefined) => {
			let rect = draggingCard?.ref?.current?.getBoundingClientRect()
			let movedCard =
				draggingCard !== undefined && rect !== undefined
					? ({
							CardId: draggingCard.Id,
							Pos: GameBoardLayout.CardRectToPercent(rect, gameBoardLayout.gameBoardDimensions),
					  } as IMovedCardPos)
					: undefined
			sendMovingCard(movedCard)
		},
		100,
		{ leading: true, trailing: true, maxWait: 100 }
	)

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
