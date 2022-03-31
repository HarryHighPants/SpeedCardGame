import copyIcon from '../Assets/copyIcon.png'
import * as signalR from '@microsoft/signalr'
import { useEffect, useLayoutEffect, useState } from 'react'
import { IGameState } from '../Interfaces/IGameState'
import { ICard, CardValue, IPos, Suit, IRenderableCard } from '../Interfaces/ICard'
import Draggable, { DraggableData } from 'react-draggable'
import styled from 'styled-components'
import React from 'react'
import { IPlayer } from '../Interfaces/IPlayer'
import RenderPlayer from './Player'
import Player from './Player'
import GameBoardLayout from '../Helpers/GameBoardLayout'
import Card from './Card'
import { PanInfo } from 'framer-motion'
import card from './Card'

interface Props {
	connection: signalR.HubConnection | undefined
	connectionId: string | undefined | null
	roomId: string | undefined
	gameState: IGameState
}

interface BoardDimensions {
	connection: signalR.HubConnection | undefined
	connectionId: string | undefined | null
	roomId: string | undefined
	gameState: IGameState
}

// Clamp number between two values with the following line:
const clamp = (num: number, min: number, max: number) => Math.min(Math.max(num, min), max)

const Game = ({ connection, connectionId, gameState, roomId }: Props) => {
	const [movedCards, setMovedCards] = useState<IMovedCardPos[]>([])
	const [renderableCards, setRenderableCards] = useState<IRenderableCard[]>([] as IRenderableCard[])
	const [gameBoardDimensions, setGameBoardDimensions] = useState<IPos>({ x: 600, y: 700 })
	const [highestCardZIndex, setHighestCardZIndex] = useState(2)
	const [draggedCards, setDraggedCards] = useState<IRenderableCard[]>([] as IRenderableCard[])

	useLayoutEffect(() => {
		function UpdateGameBoardDimensions() {
			setGameBoardDimensions({
				x: clamp(window.innerWidth, 0, GameBoardLayout.maxWidth),
				y: window.innerHeight,
			} as IPos)
		}

		UpdateGameBoardDimensions()
		window.addEventListener('resize', UpdateGameBoardDimensions)
		return () => window.removeEventListener('resize', UpdateGameBoardDimensions)
	}, [])

	useEffect(() => {
		if (!connection) return
		connection.on('CardMoved', CardMoved)

		return () => {
			connection.off('CardMoved', CardMoved)
		}
	}, [connection])

	useEffect(() => {
		UpdateRenderableCards()
	}, [gameState])

	const CardMoved = (data: any) => {
		let parsedData: IMovedCardPos = JSON.parse(data)

		let existingMovingCard = movedCards.find((c) => c.cardId === parsedData.cardId)
		if (existingMovingCard) {
			existingMovingCard.pos = parsedData.pos
		} else {
			movedCards.push(parsedData)
		}
		setMovedCards([...movedCards])
		UpdateRenderableCards()
	}

	const UpdateRenderableCards = () => {
		let ourId = connectionId ?? 'CUqUsFYm1zVoW-WcGr6sUQ'
		let newRenderableCards = [] as IRenderableCard[]

		// Add players cards
		gameState.Players.map((p, i) => {
			let ourPlayer = p.Id == ourId

			// Add the players hand cards
			let handCardPositions = GameBoardLayout.GetHandCardPositions(i)
			let handCards = p.HandCards.map((hc, cIndex) => {
				// We only want to override the position of cards that aren't ours
				let movedCard = ourPlayer ? null : movedCards.find((c) => c.cardId === hc.Id)
				return {
					...hc,
					draggable: ourPlayer,
					droppableTarget: false,
					pos: movedCard?.pos ?? handCardPositions[cIndex],
				} as IRenderableCard
			})
			newRenderableCards.push(...handCards)

			// Add the players Kitty card
			let movedCard = ourPlayer ? null : movedCards.find((c) => c.cardId === p.TopKittyCardId)
			let kittyCardPosition = GameBoardLayout.GetKittyCardPosition(i)
			let kittyCard = {
				Id: p.TopKittyCardId,
				draggable: ourPlayer,
				droppableTarget: false,
				pos: movedCard?.pos ?? kittyCardPosition,
			} as IRenderableCard
			newRenderableCards.push(kittyCard)
		})

		// Add the center piles
		let centerCardPositions = GameBoardLayout.GetCenterCardPositions()
		let centerPilePositions = gameState.CenterPiles.map((cp, cpIndex) => {
			return {
				...cp,
				draggable: false,
				droppableTarget: true,
				pos: centerCardPositions[cpIndex],
			} as IRenderableCard
		})
		newRenderableCards.push(...centerPilePositions)
		setRenderableCards(newRenderableCards)
	}

	const OnStartDrag = (panInfo: PanInfo, card: IRenderableCard) => {
		setHighestCardZIndex(highestCardZIndex + 1)
		let newDraggedCards = draggedCards
		newDraggedCards.push(card)
		setDraggedCards([...newDraggedCards])
	}

	const OnEndDrag = (panInfo: PanInfo, card: IRenderableCard) => {
		let newDraggedCards = draggedCards.filter((c) => c != card)
		setDraggedCards([...newDraggedCards])
	}

	return (
		<Board>
			<Player key={gameState.Players[0].Id} player={gameState.Players[0]} />
			{renderableCards.map((c) => (
				<Card
					card={c}
					gameBoardDimensions={gameBoardDimensions}
					highestCardZIndex={highestCardZIndex}
					onDragStart={OnStartDrag}
					onDragEnd={OnEndDrag}
				/>
			))}
			<Player key={gameState.Players[1].Id} player={gameState.Players[1]} />
		</Board>
	)
}

const Board = styled.div`
	margin-top: 100px;
	position: relative;
	background-color: #729bf5;
	width: 100%;
	height: 100%;
	max-width: ${GameBoardLayout.maxWidth}px;
	//user-select: none;
`

export interface IMovedCardPos {
	cardId: number
	pos: IPos
}

export default Game
