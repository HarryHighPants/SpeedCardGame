import copyIcon from '../Assets/copyIcon.png'
import * as signalR from '@microsoft/signalr'
import { useEffect, useLayoutEffect, useState } from 'react'
import { IGameState } from '../Interfaces/IGameState'
import { ICard, CardValue, IPos, Suit, IRenderableCard, IDraggedRenderableCard } from '../Interfaces/ICard'
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
	const [draggedCard, setDraggedCard] = useState<IDraggedRenderableCard>()
	const [dropTarget, setDropTarget] = useState<IRenderableCard>()

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
		UpdateRenderableCards()
	}, [gameBoardDimensions])

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

	const getPosPixels = (pos: IPos): IPos => {
		return {
			x: pos.x * gameBoardDimensions.x,
			y: pos.y * gameBoardDimensions.y,
		}
	}

	const UpdateRenderableCards = () => {
		console.log('UpdateRenderableCards' + gameBoardDimensions.x)
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
				let zIndex = cIndex + 5
				if (!ourPlayer) {
					zIndex = Math.abs(cIndex - 4)
				}
				return {
					...hc,
					draggable: ourPlayer,
					droppableTarget: false,
					pos: getPosPixels(movedCard?.pos ?? handCardPositions[cIndex]),
					zIndex: zIndex,
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
				pos: getPosPixels(movedCard?.pos ?? kittyCardPosition),
				zIndex: ourPlayer ? 10 : 5,
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
				pos: getPosPixels(centerCardPositions[cpIndex]),
			} as IRenderableCard
		})
		newRenderableCards.push(...centerPilePositions)
		setRenderableCards(newRenderableCards)
	}

	// const UpdateHoveredDropTarget = (card: IRenderableCard | undefined) => {
	// 	if (card?.Id === dropTarget?.Id) return
	//
	// 	// If we are replacing the current dropTarget unset its property
	// 	if (!!dropTarget) {
	// 		dropTarget.hoveredDropTarget = false
	// 	}
	//
	// 	if (!!card) {
	// 		card.hoveredDropTarget = true
	// 		console.log('hovering over', card)
	// 	}
	//
	// 	setDropTarget(card)
	// 	// Update our renderable cards
	// 	setRenderableCards([...renderableCards])
	// }

	const OnStartDrag = (rect: DOMRect | undefined, card: IRenderableCard) => {
		setDraggedCard({ ...card, domRect: rect })
	}

	const OnDrag = (rect: DOMRect | undefined, card: IRenderableCard) => {
		setDraggedCard({ ...card, domRect: rect })
		// if (!rect) return
		//
		// // Get how close this card is to others
		// let distances = renderableCards
		// 	.map((c) => {
		// 		return {
		// 			card: c,
		// 			distance: !!card.ref.current
		// 				? GetDistance(
		// 						{
		// 							x: card.ref.current?.getBoundingClientRect().x,
		// 							y: card.ref.current?.getBoundingClientRect().y,
		// 						},
		// 						{ x: rect.x, y: rect.y }
		// 				  )
		// 				: Infinity,
		// 		}
		// 	})
		// 	.sort((d1, d2) => d1.distance - d2.distance)
		//
		// // See if this card is on top of any other card
		// if (distances[0].distance < GameBoardLayout.dropDistance) {
		// 	UpdateHoveredDropTarget(distances[0].card)
		// 	console.log(rect, distances[0].card.ref.current?.getBoundingClientRect())
		// } else if (!!dropTarget) {
		// 	UpdateHoveredDropTarget(undefined)
		// }
	}

	const OnEndDrag = (rect: DOMRect | undefined, card: IRenderableCard) => {
		setDraggedCard(undefined)
		// if (dropTarget!!) {
		// 	// We just dropped this card onto our dropTargetCard
		// }
		// setDraggingCard(undefined)
	}

	// const OnMouseEnter = (card: IRenderableCard) => {
	// 	if (!draggingCard || !card.droppableTarget) return
	// 	console.log('hovering over', card)
	// 	setDropTarget(card)
	// }
	//
	// const OnMouseExit = (card: IRenderableCard) => {
	// 	if (!card.droppableTarget) return
	// 	setDropTarget(undefined)
	// }

	const OnDroppedOn = (card: IRenderableCard) => {
		console.log('Got dropped on!')
	}

	return (
		<Board>
			<Player key={gameState.Players[0].Id} player={gameState.Players[0]} />
			{renderableCards.map((c) => (
				<Card
					card={c}
					gameBoardDimensions={gameBoardDimensions}
					onDragStart={OnStartDrag}
					onDrag={OnDrag}
					onDragEnd={OnEndDrag}
					onDroppedOn={OnDroppedOn}
					draggedCard={draggedCard}
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
