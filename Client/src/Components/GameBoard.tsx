import * as signalR from '@microsoft/signalr'
import React, { useEffect, useLayoutEffect, useState } from 'react'
import { IGameState } from '../Interfaces/IGameState'
import { CardLocationType, ICard, IMovedCardPos, IPos, IRenderableCard } from '../Interfaces/ICard'
import styled from 'styled-components'
import Player from './Player'
import GameBoardLayout from '../Helpers/GameBoardLayout'
import Card from './Card'
import { AnimatePresence, LayoutGroup, PanInfo } from 'framer-motion'
import { clamp, GetDistanceRect } from '../Helpers/Utilities'
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
	const [cardBeingDragged, setCardBeingDragged] = useState<IRenderableCard>()
	const [handAreaHighlighted, setHandAreaHighlighted] = useState<boolean>(false)

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
	}

	const DraggingCardUpdated = (draggingCard: IRenderableCard | undefined) => {
		let newDraggingCard = draggingCard !== undefined ? { ...draggingCard } : undefined
		setCardBeingDragged(newDraggingCard)

		let rect = newDraggingCard?.ref?.current?.getBoundingClientRect()

		// Send the event to the other player
		let movedCard =
			newDraggingCard !== undefined && rect !== undefined
				? ({
						CardId: newDraggingCard.Id,
						Pos: GameBoardLayout.CardRectToPercent(rect, gameBoardDimensions),
				  } as IMovedCardPos)
				: undefined
		console.log('Sending:', movedCard?.Pos?.X, movedCard?.Pos?.Y)
		console.log('ideal', {X: 0.8, Y: 0.75})
		onDraggingCardUpdated(movedCard)
	}

	const OnEndDrag = (topCard: IRenderableCard) => {
		let bottomCard = GetBottomCard()
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

	const GetBottomCard = () => {
		let cardDistances = renderableCards.map((c) => {
			return {
				card: c,
				distance: GetDistanceRect(
					cardBeingDragged?.ref.current?.getBoundingClientRect(),
					c.ref.current?.getBoundingClientRect()
				),
			}
		})
		cardDistances = cardDistances.sort((a, b) => a.distance - b.distance)
		if (cardDistances[1].distance > gameBoardLayout.dropDistance) return
		return cardDistances[1].card
	}

	return (
		<GameBoardContainer>
			<GameBoardAreas
				cardBeingDragged={cardBeingDragged}
				ourId={playerId}
				gameBoardDimensions={gameBoardDimensions}
				gameState={gameState}
				setHandAreaHighlighted={setHandAreaHighlighted}
			/>
			<div>
				<AnimatePresence>
					{renderableCards.map((c) => (
						<Card
							key={`card-${c.Id}`}
							card={c}
							draggingCardUpdated={DraggingCardUpdated}
							onDragEnd={OnEndDrag}
							cardBeingDragged={cardBeingDragged}
						/>
					))}
				</AnimatePresence>
			</div>
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
