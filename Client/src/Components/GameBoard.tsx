import * as signalR from '@microsoft/signalr'
import React, { useEffect, useLayoutEffect, useState } from 'react'
import { IGameState } from '../Interfaces/IGameState'
import { CardLocationType, ICard, IMovedCardPos, IPos, IRenderableCard } from '../Interfaces/ICard'
import styled from 'styled-components'
import Player from './Player'
import GameBoardLayout from '../Helpers/GameBoardLayout'
import Card from './Card'
import { LayoutGroup, PanInfo } from 'framer-motion'
import { clamp, GetDistanceRect } from '../Helpers/Utilities'
import { IPlayer } from '../Interfaces/IPlayer'
import gameBoardLayout from '../Helpers/GameBoardLayout'
import GameBoardAreas from './GameBoardAreas/GameBoardAreas'

interface Props {
	playerId: string | undefined | null
	gameState: IGameState
	movedCards: IMovedCardPos[]
	gameBoardDimensions: IPos
	onPlayCard: (topCard: ICard, centerPileIndex: number) => void
	onPickupFromKitty: () => void
}

const GameBoard = ({ gameBoardDimensions, playerId, gameState, movedCards, onPlayCard, onPickupFromKitty }: Props) => {
	const [renderableCards, setRenderableCards] = useState<IRenderableCard[]>([] as IRenderableCard[])
	const [cardBeingDragged, setCardBeingDragged] = useState<IRenderableCard>()
	const [handAreaHighlighted, setHandAreaHighlighted] = useState<boolean>(false)

	useEffect(() => {
		UpdateRenderableCards()
	}, [gameBoardDimensions])

	useEffect(() => {
		UpdateRenderableCards()
	}, [gameState])

	const UpdateRenderableCards = () => {
		let gameBoardLayout = new GameBoardLayout(gameBoardDimensions, movedCards, renderableCards)
		setRenderableCards(gameBoardLayout.GetRenderableCards(playerId, gameState))
	}

	const OnEndDrag = (topCard: IRenderableCard) => {
		let bottomCard = GetBottomCard()
		DetectMove(topCard, bottomCard)
		setCardBeingDragged(undefined)
	}

	const DetectMove = (topCard: IRenderableCard, bottomCard: IRenderableCard | undefined) => {
		// If we are trying to play a card into the center
		if (
			topCard.location === CardLocationType.Hand &&
			!!bottomCard &&
			bottomCard.location === CardLocationType.Center
		) {
			let centerPileCard = gameState.CenterPiles.find((c) => c.Id === bottomCard.Id)
			if (!centerPileCard) return
			let centerPileId = gameState.CenterPiles.indexOf(centerPileCard)
			if (centerPileId === -1) return
			console.log('Attempt play', topCard)
			onPlayCard(topCard, centerPileId)
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
				{renderableCards.map((c) => (
					<Card
						key={`card-${c.Id}`}
						card={c}
						setDraggingCard={setCardBeingDragged}
						onDragEnd={OnEndDrag}
						cardBeingDragged={cardBeingDragged}
					/>
				))}
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
