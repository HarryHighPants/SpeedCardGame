import * as signalR from '@microsoft/signalr'
import React, { useEffect, useLayoutEffect, useState } from 'react'
import { IGameState } from '../Interfaces/IGameState'
import { CardLocationType, ICard, IMovedCardPos, IPos, IRenderableCard } from '../Interfaces/ICard'
import styled from 'styled-components'
import Player from './Player'
import GameBoardLayout from '../Helpers/GameBoardLayout'
import Card from './Card'
import { LayoutGroup, PanInfo } from 'framer-motion'
import { clamp } from '../Helpers/Utilities'
import GameBoard from './GameBoard'
import backgroundImg from '../Assets/felt-tiling.jpg'
import { debounce } from 'lodash'

interface Props {
	connection: signalR.HubConnection | undefined
	connectionId: string | undefined | null
	gameState: IGameState
	invertedCenterPiles: boolean
}

const getGameBoardDimensions = () => {
	return {
		X: clamp(window.innerWidth, 0, GameBoardLayout.maxWidth),
		Y: window.innerHeight,
	} as IPos
}

const Game = ({ connection, connectionId, gameState, invertedCenterPiles }: Props) => {
	const [movedCard, setMovedCard] = useState<IMovedCardPos>()
	const [gameBoardDimensions, setGameBoardDimensions] = useState<IPos>(getGameBoardDimensions())
	const [localGameState, setLocalGameState] = useState<IGameState>(gameState)

	useEffect(() => {
		setLocalGameState(gameState)
	}, [gameState])

	useLayoutEffect(() => {
		function UpdateGameBoardDimensions() {
			setGameBoardDimensions(getGameBoardDimensions)
		}

		UpdateGameBoardDimensions()
		window.addEventListener('resize', UpdateGameBoardDimensions)
		return () => window.removeEventListener('resize', UpdateGameBoardDimensions)
	}, [])

	useEffect(() => {
		if (!connection) return
		connection.on('MovingCardUpdated', UpdateMovingCard)

		return () => {
			connection.off('MovingCardUpdated', UpdateMovingCard)
		}
	}, [connection])

	const UpdateMovingCard = (data: any) => {
		let parsedData: IMovedCardPos = JSON.parse(data)
		console.log(data)
		setMovedCard(parsedData)
	}

	const OnPlayCard = (topCard: ICard, centerPileIndex: number) => {
		// Call the event
		let correctedCenterPileIndex = invertedCenterPiles ? (centerPileIndex + 1) % 2 : centerPileIndex
		connection?.invoke('TryPlayCard', topCard.Id, correctedCenterPileIndex).catch((e) => console.log(e))

		// Show any messages (Move to a warnings component)

		// assume the server will return success and update the gamestate
		UpdateGameStatePlayCard(topCard, centerPileIndex)
	}
	const UpdateGameStatePlayCard = (topCard: ICard, centerPileIndex: number) => {
		let player = gameState.Players.find((p) => p.Id === connectionId)
		if (player == null) {
			return
		}

		let playerIndex = gameState.Players.indexOf(player)
		gameState.Players[playerIndex].HandCards = player.HandCards.filter((c) => c.Id != topCard.Id)
		gameState.CenterPiles[centerPileIndex].Cards.push(topCard)
		setLocalGameState({ ...gameState })
		console.log('UpdateGameStatePlayCard')
	}

	const OnPickupFromKitty = () => {
		// Call the event
		connection?.invoke('TryPickupFromKitty').catch((e) => console.log(e))
		// Show any messages (Move to a warnings component)
		// assume the server will return success and update the gamestate
		UpdateGameStatePickupFromKitty()
	}
	const UpdateGameStatePickupFromKitty = () => {
		let player = gameState.Players.find((p) => p.Id === connectionId)
		if (player == null) {
			return
		}

		let playerIndex = gameState.Players.indexOf(player)
		gameState.Players[playerIndex].HandCards.push({ Id: player.TopKittyCardId } as ICard)
		gameState.Players[playerIndex].TopKittyCardId = -1
		setLocalGameState({ ...gameState })
		console.log('UpdateGameStatePickupFromKitty')
	}

	const OnRequestTopUp = () => {
		// Call the event
		connection?.invoke('TryRequestTopUp').catch((e) => console.log(e))
		// Show any messages (Move to a warnings component)
	}

	const OnDraggingCardUpdated = debounce((draggingCard: IMovedCardPos | undefined) => {
		// connection?.invoke('UpdateMovingCard', draggingCard).catch((e) => console.log(e))
	}, 100, {leading: true, trailing: true, maxWait: 100})

	return (
		<GameContainer>
			<Player key={`player-${localGameState.Players[0].Id}`} player={localGameState.Players[0]} onTop={true} />
			<GameBoard
				playerId={connectionId}
				gameState={localGameState}
				movedCard={movedCard}
				gameBoardDimensions={gameBoardDimensions}
				onPlayCard={OnPlayCard}
				onPickupFromKitty={OnPickupFromKitty}
				onDraggingCardUpdated={OnDraggingCardUpdated}
			/>
			<Player
				onRequestTopUp={OnRequestTopUp}
				key={`player-${localGameState.Players[1].Id}`}
				player={localGameState.Players[1]}
				onTop={false}
			/>
			<Background key={'bg'} style={{ backgroundImage: `url(${backgroundImg})` }} />
		</GameContainer>
	)
}

const GameContainer = styled.div`
	display: flex;
	flex-direction: column;
	align-items: center;
	height: 100%;
	max-width: 100%;
	max-height: 100%;
	overflow: hidden;
	overscroll-behavior: none;
`

const Background = styled.div`
	background-color: #52704b;
	background-size: 150px;
	background-blend-mode: soft-light;
	position: absolute;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	z-index: -5;
`

export default Game
