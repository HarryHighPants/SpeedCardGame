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

interface Props {
	connection: signalR.HubConnection | undefined
	playerId: string | undefined | null
	gameState: IGameState
	gameBoardLayout: GameBoardLayout
	onPlayCard: (topCard: ICard, centerPileIndex: number) => void
	onDraggingCardUpdated: (draggingCard: IRenderableCard) => void
	onEndDrag: (draggingCard: IRenderableCard) => void
	sendMovingCard: (movedCard: IMovedCardPos | undefined) => void
}

const CardsContainer = ({
	connection,
	gameBoardLayout,
	playerId,
	gameState,
	onPlayCard,
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
				// Reset states
				if (!card.highlighted) {
					card.highlighted = false
					if (!!card.forceUpdate) {
						card.forceUpdate()
					}
				}
				if (card.horizontalOffset !== 0) {
					card.horizontalOffset = 0
					if (!!card.forceUpdate) {
						card.forceUpdate()
					}
				}
				continue
			}

			// Check if we are a center card that can be dropped onto
			let droppingOntoCenter =
				card.location === CardLocationType.Center && draggingCard?.location === CardLocationType.Hand
			if (droppingOntoCenter) {
				// console.log(card.Id, offsetInfo.distance < GameBoardLayout.dropDistance)
				let shouldBeHighlighted = offsetInfo.distance < GameBoardLayout.dropDistance
				if (card.highlighted !== shouldBeHighlighted) {
					// console.log(card.Id, 'Updating highlighting',shouldBeHighlighted )
					card.highlighted = shouldBeHighlighted
					if (!!card.forceUpdate) {
						card.forceUpdate()
					}
				}
			}

			// Check if we are a hand card that can be dragged onto
			let droppingOntoHandCard =
				card.ourCard &&
				card.location === CardLocationType.Hand &&
				draggingCard?.location === CardLocationType.Kitty
			if (droppingOntoHandCard) {
				// We want to animate to either the left or the right on the dragged kitty card
				let horizontalOffset = (!!offsetInfo.delta && offsetInfo.delta?.X < 0 ? 1 : 0) * 50
				if (horizontalOffset !== card.horizontalOffset) {
					card.horizontalOffset = horizontalOffset
					if (!!card.forceUpdate) {
						card.forceUpdate()
					}
				}
			}
		}
	}

	const OnEndDrag = (topCard: IRenderableCard) => {
		DetectPlayCard(topCard, GetBottomCard(topCard))
		DraggingCardUpdated(undefined)
		onEndDrag(topCard)
	}

	const DetectPlayCard = (topCard: IRenderableCard, bottomCard: IRenderableCard | undefined) => {
		// If we are trying to play a card into the center
		if (
			topCard.location === CardLocationType.Hand &&
			!!bottomCard &&
			bottomCard.location === CardLocationType.Center
		) {
			let centerPileIndex: number = gameState.CenterPiles.findIndex(
				(cp) => cp.Cards.find((c) => c.Id === bottomCard.Id) !== undefined
			)
			if (centerPileIndex === -1) return
			console.log('Attempt play', topCard)
			onPlayCard(topCard, centerPileIndex)
		}
	}

	const GetBottomCard = (topCard: IRenderableCard) => {
		let cardDistances = renderableCards.map((c) => {
			return {
				card: c,
				distance: GetDistanceRect(
					topCard?.ref.current?.getBoundingClientRect(),
					c.ref.current?.getBoundingClientRect()
				),
			}
		})
		cardDistances = cardDistances.sort((a, b) => a.distance - b.distance)
		if (cardDistances[1].distance > GameBoardLayout.dropDistance) return
		return cardDistances[1].card
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
