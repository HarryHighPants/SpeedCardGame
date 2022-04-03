import * as signalR from '@microsoft/signalr'
import React, {useEffect, useLayoutEffect, useState} from 'react'
import {IGameState} from '../Interfaces/IGameState'
import {CardLocationType, IPos, IRenderableCard} from '../Interfaces/ICard'
import styled from 'styled-components'
import Player from './Player'
import GameBoardLayout from '../Helpers/GameBoardLayout'
import Card from './Card'
import {PanInfo} from 'framer-motion'

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
	const [draggingCard, setDraggingCard] = useState<IRenderableCard>()
	// const [draggingCardMoved, onDraggingCardMoved] = useState(0)

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

	const getPosPixels = (pos: IPos): IPos => {
		return {
			x: pos!! ? pos.x * gameBoardDimensions.x : 0,
			y: pos!! ? pos.y * gameBoardDimensions.y : 0,
		}
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
				let zIndex = cIndex + 5
				if (!ourPlayer) {
					zIndex = Math.abs(cIndex - 4)
				}
				return {
					...hc,
					ourCard: ourPlayer,
					location: CardLocationType.Hand,
					pos: getPosPixels(movedCard?.pos ?? handCardPositions[cIndex]),
					zIndex: zIndex,
					ref: React.createRef<HTMLDivElement>(),
				} as IRenderableCard
			})
			newRenderableCards.push(...handCards)

			// Add the players Kitty card
			let movedCard = ourPlayer ? null : movedCards.find((c) => c.cardId === p.TopKittyCardId)
			let kittyCardPosition = GameBoardLayout.GetKittyCardPosition(i)
			let kittyCard = {
				Id: p.TopKittyCardId,
				ourCard: ourPlayer,
				location: CardLocationType.Kitty,
				pos: getPosPixels(movedCard?.pos ?? kittyCardPosition),
				zIndex: ourPlayer ? 10 : 5,
				ref: React.createRef<HTMLDivElement>(),
			} as IRenderableCard
			newRenderableCards.push(kittyCard)
		})

		// Add the center piles
		let centerCardPositions = GameBoardLayout.GetCenterCardPositions()
		let centerPilePositions = gameState.CenterPiles.map((cp, cpIndex) => {
			return {
				...cp,
				location: CardLocationType.Center,
				pos: getPosPixels(centerCardPositions[cpIndex]),
				ref: React.createRef<HTMLDivElement>(),
				ourCard: false
			} as IRenderableCard
		})
		newRenderableCards.push(...centerPilePositions)
		setRenderableCards(newRenderableCards)
	}

	const OnStartDrag = (panInfo: PanInfo, card: IRenderableCard) => {
		setDraggingCard({...card})
	}

	const OnDrag = (panInfo: PanInfo, card: IRenderableCard) => {
		setDraggingCard({...card})
	}

	const OnEndDrag = (panInfo: PanInfo, droppedCard: IRenderableCard) => {
		setDraggingCard(undefined)
		// if (!!dropTarget) {
		// 	let ourPlayer = gameState.Players.find((p) => p.Id === connectionId)
		// 	let playedCardInHand = ourPlayer?.HandCards.find((c) => c.Id === droppedCard.Id)
		// 	if (!!playedCardInHand) {
		// 		// See if we dropped it onto a center pile
		// 		let centerPileCard = gameState.CenterPiles.find((c) => c.Id === dropTarget.Id)
		// 		if (!!centerPileCard) {
		// 			// Try and play the card
		// 			console.log('Attempt play', droppedCard)
		// 		}
		// 	}
		//
		// 	let playedKittyCard = ourPlayer?.TopKittyCardId === droppedCard.Id
		// 	if (playedKittyCard) {
		// 		let handCard = ourPlayer?.HandCards.find((c) => c.Id === dropTarget.Id)
		// 		if (!!handCard) {
		// 			// Try and pickup from Kitty
		// 			console.log('Attempt pickup kitty', droppedCard)
		// 		}
		// 	}
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

	return (
		<Board>
			<Player key={gameState.Players[0].Id} player={gameState.Players[0]} />
			{renderableCards.map((c) => (
				<Card
					// 	onMouseEnter={OnMouseEnter}
					// 	onMouseExit={OnMouseExit}
					card={c}
					gameBoardDimensions={gameBoardDimensions}
					onDragStart={OnStartDrag}
					onDrag={OnDrag}
					onDragEnd={OnEndDrag}
					draggingCard={draggingCard}
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
	user-select: none;
`

export interface IMovedCardPos {
	cardId: number
	pos: IPos
}

export default Game
