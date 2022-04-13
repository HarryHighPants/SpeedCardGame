import * as signalR from '@microsoft/signalr'
import React, { useEffect, useLayoutEffect, useState } from 'react'
import { IGameState } from '../Interfaces/IGameState'
import { CardLocationType, ICard, IMovedCardPos, IPos, IRenderableCard } from '../Interfaces/ICard'
import styled from 'styled-components'
import Player from './Player'
import GameBoardLayout from '../Helpers/GameBoardLayout'
import Card from './Card'
import { AnimatePresence, LayoutGroup, PanInfo } from 'framer-motion'
import { clamp, GetDistanceRect, Overlaps } from '../Helpers/Utilities'
import { IPlayer } from '../Interfaces/IPlayer'
import gameBoardLayout from '../Helpers/GameBoardLayout'
import GameBoardAreas from './GameBoardAreas/GameBoardAreas'

interface Props {
	playerId: string | undefined | null
	gameState: IGameState
	movedCard: IMovedCardPos | undefined
	gameBoardDimensions: IPos
	onPlayCard: (topCard: ICard, centerPileIndex: number) => void
	onPickupFromKitty: () => void
	onDraggingCardUpdated: (draggingCard: IMovedCardPos | undefined) => void
}

const GameBoard = ({
	gameBoardDimensions,
	playerId,
	gameState,
	movedCard,
	onPlayCard,
	onPickupFromKitty,
	onDraggingCardUpdated,
}: Props) => {
	const [renderableCards, setRenderableCards] = useState<IRenderableCard[]>([] as IRenderableCard[])
	const [handAreaHighlighted, setHandAreaHighlighted] = useState<boolean>(false)
	const [gameBoardLayout, setGameBoardLayout] = useState<GameBoardLayout>()

	useEffect(() => {
		UpdateRenderableCards()
	}, [gameBoardDimensions])

	useEffect(() => {
		UpdateRenderableCards()
	}, [gameState])

	useEffect(() => {
		UpdateRenderableCards()
	}, [movedCard])

	const UpdateRenderableCards = () => {
		let gameBoardLayout = new GameBoardLayout(gameBoardDimensions, movedCard, renderableCards)
		setRenderableCards(gameBoardLayout.GetRenderableCards(playerId, gameState))
		setGameBoardLayout(gameBoardLayout)
	}

	const DraggingCardUpdated = (draggingCard: IRenderableCard | undefined) => {
		let newDraggingCard = draggingCard !== undefined ? { ...draggingCard } : undefined

		UpdateCardsHoverStates(draggingCard)

		// Send the event to the other player
		let rect = newDraggingCard?.ref?.current?.getBoundingClientRect()
		let movedCard =
			newDraggingCard !== undefined && rect !== undefined
				? ({
						CardId: newDraggingCard.Id,
						Pos: GameBoardLayout.CardRectToPercent(rect, gameBoardDimensions),
				  } as IMovedCardPos)
				: undefined
		onDraggingCardUpdated(movedCard)
	}

	const GetOffsetInfo = (ourRect: DOMRect | undefined, draggingCardRect: DOMRect | undefined) => {
		let distance = GetDistanceRect(draggingCardRect, ourRect)
		let overlaps = Overlaps(ourRect, draggingCardRect)
		let delta =
			!draggingCardRect || !ourRect
				? undefined
				: { X: draggingCardRect.x - ourRect.x, Y: draggingCardRect.y - ourRect.y }
		return { distance, overlaps, delta }
	}

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
					if(!!card.forceUpdate){
						card.forceUpdate()
					}
				}
				if (card.horizontalOffset !== 0) {
					card.horizontalOffset = 0
					if(!!card.forceUpdate){
						card.forceUpdate()
					}
				}
				continue
			}

			// Check if we are a center card that can be dropped onto
			let droppingOntoCenter = card.location === CardLocationType.Center && draggingCard?.location === CardLocationType.Hand
			if (droppingOntoCenter) {
				// console.log(card.Id, offsetInfo.distance < GameBoardLayout.dropDistance)
				let shouldBeHighlighted = offsetInfo.distance < GameBoardLayout.dropDistance
				if (card.highlighted !== shouldBeHighlighted) {
					// console.log(card.Id, 'Updating highlighting',shouldBeHighlighted )
					card.highlighted = shouldBeHighlighted
					if(!!card.forceUpdate){
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
					if(!!card.forceUpdate){
						card.forceUpdate()
					}
				}
			}
		}
	}

	const OnEndDrag = (topCard: IRenderableCard) => {
		let bottomCard = GetBottomCard(topCard)
		DetectMove(topCard, bottomCard)
		DraggingCardUpdated(undefined)
	}

	const DetectMove = (topCard: IRenderableCard, bottomCard: IRenderableCard | undefined) => {
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

		// If we are trying to pickup a card from the kitty
		if (
			(topCard.location === CardLocationType.Kitty &&
				!!bottomCard &&
				bottomCard.location === CardLocationType.Hand &&
				bottomCard.ourCard) ||
			handAreaHighlighted
		) {
			console.log('Attempt pickup from kitty', topCard)
			onPickupFromKitty()
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
		<GameBoardContainer>
			<GameBoardAreas
				ourId={playerId}
				gameBoardLayout={gameBoardLayout}
				gameState={gameState}
				setHandAreaHighlighted={setHandAreaHighlighted}
			/>
			<AnimatePresence>
				{renderableCards.map((c) => (
					<Card
						key={`card-${c.Id}`}
						card={c}
						draggingCardUpdated={DraggingCardUpdated}
						onDragEnd={OnEndDrag}
					/>
				))}
			</AnimatePresence>
		</GameBoardContainer>
	)
}

const GameBoardContainer = styled.div`
	position: relative;
	height: 100%;
	width: 100%;
	max-width: ${GameBoardLayout.maxWidth}px;
	flex: 1;
	user-select: none;
`

export default GameBoard
