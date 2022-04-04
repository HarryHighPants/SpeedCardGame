import * as signalR from '@microsoft/signalr'
import React, { useEffect, useLayoutEffect, useState } from 'react'
import { IGameState } from '../Interfaces/IGameState'
import { CardLocationType, ICard, IMovedCardPos, IPos, IRenderableCard } from '../Interfaces/ICard'
import styled from 'styled-components'
import Player from './Player'
import GameBoardLayout from '../Helpers/GameBoardLayout'
import Card from './Card'
import { LayoutGroup, PanInfo } from 'framer-motion'
import { GetDistanceRect } from '../Helpers/Distance'
import { clamp } from '../Helpers/Utilities'
import { IPlayer } from '../Interfaces/IPlayer'
import gameBoardLayout from '../Helpers/GameBoardLayout'

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
	const [draggingCard, setDraggingCard] = useState<IRenderableCard>()

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
		if (!!bottomCard) {
			DetectMove(topCard, bottomCard)
		}
		setDraggingCard(undefined)
	}

	const DetectMove = (topCard: IRenderableCard, bottomCard: IRenderableCard) => {
		// If we are trying to play a card into the center
		if (topCard.location === CardLocationType.Hand && bottomCard.location === CardLocationType.Center) {
			let centerPileCard = gameState.CenterPiles.find((c) => c.Id === bottomCard.Id)
			if (!centerPileCard) return
			let centerPileId = gameState.CenterPiles.indexOf(centerPileCard)
			if (centerPileId === -1) return
			console.log('Attempt play', topCard)
			onPlayCard(topCard, centerPileId)
		}

		// If we are trying to pickup a card from the kitty
		if (
			topCard.location === CardLocationType.Kitty &&
			bottomCard.location === CardLocationType.Hand &&
			bottomCard.ourCard
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
					draggingCard?.ref.current?.getBoundingClientRect(),
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
			{renderableCards.map((c) => (
				<Card
					key={`card-${c.Id}`}
					card={c}
					setDraggingCard={setDraggingCard}
					onDragEnd={OnEndDrag}
					cardBeingDragged={draggingCard}
				/>
			))}
		</GameBoardContainer>
	)
}

const GameBoardContainer = styled.div`
	background-color: #4d6947;
	position: relative;
	height: 100%;
	flex: 1;
	user-select: none;
`

export default GameBoard
