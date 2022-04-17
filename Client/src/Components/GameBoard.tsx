import * as signalR from '@microsoft/signalr'
import React, { RefObject, useEffect, useLayoutEffect, useReducer, useState } from 'react'
import { gameStateReducer, IGameState } from '../Interfaces/IGameState'
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
import CardsContainer from './CardsContainer'
import { debounce } from 'lodash'
import { IRenderableArea } from '../Interfaces/IBoardArea'
import GameBoardLayoutAreas from '../Helpers/GameBoardLayoutAreas'

interface Props {
	connection: signalR.HubConnection | undefined
	playerId: string | undefined | null
	gameState: IGameState
	gameBoardLayout: GameBoardLayout
	sendPlayCard: (topCard: ICard, centerPileIndex: number) => void
	sendPickupFromKitty: () => void
	sendMovingCard: (movedCard: IMovedCardPos | undefined) => void
	flippedCenterPiles: boolean
}

const GameBoard = ({
	connection,
	playerId,
	gameState,
	gameBoardLayout,
	sendPlayCard,
	sendPickupFromKitty,
	sendMovingCard,
	flippedCenterPiles,
}: Props) => {
	const [localGameState, dispatchLocalGameState] = useReducer(gameStateReducer, gameState)
	const [renderableAreas, setRenderableAreas] = useState([] as IRenderableArea[])

	useEffect(() => {
		setRenderableAreas(gameBoardLayout.GetBoardAreas(renderableAreas))
	}, [gameBoardLayout.gameBoardDimensions])

	useEffect(() => {
		dispatchLocalGameState({ type: 'Replace', gameState: gameState })
	}, [gameState])

	const OnPlayCard = (topCard: ICard, centerPileIndex: number) => {
		sendPlayCard(topCard, centerPileIndex)
		dispatchLocalGameState({ type: 'Play', topCard: topCard, centerPileIndex: centerPileIndex, playerId: playerId })
	}

	const OnPickupFromKitty = () => {
		console.log('Attempt pickup from kitty')
		sendPickupFromKitty()
		dispatchLocalGameState({ type: 'Pickup', playerId: playerId })
	}

	const DetectPlay = (draggingCard: IRenderableCard) => {
		let playableAreas = GameBoardLayoutAreas.GetPlayableAreas(renderableAreas, draggingCard)
		playableAreas.forEach((area) => {
			if (area.location.type === 'Center') {
				OnPlayCard(draggingCard, area.location.index)
			}
			if (area.location.type === 'Hand') {
				OnPickupFromKitty()
			}
		})
	}

	const OnEndDrag = (draggingCard: IRenderableCard) => {
		DetectPlay(draggingCard)
		OnDragUpdated(undefined)
	}

	const OnDragUpdated = (draggingCard: IRenderableCard | undefined) => {
		let playableAreas = GameBoardLayoutAreas.GetPlayableAreas(renderableAreas, draggingCard)
		renderableAreas.forEach((area) => SetHighlight(area, playableAreas.includes(area)))
	}

	const SetHighlight = (rArea: IRenderableArea, highlight: boolean) => {
		if (rArea.highlight !== highlight) {
			rArea.highlight = highlight
			if (!!rArea.forceUpdate) {
				rArea.forceUpdate()
			}
		}
	}

	return (
		<GameBoardContainer>
			<GameBoardAreas ourId={playerId} gameState={localGameState} renderableAreas={renderableAreas} />
			<CardsContainer
				sendMovingCard={sendMovingCard}
				connection={connection}
				playerId={playerId}
				gameState={localGameState}
				gameBoardLayout={gameBoardLayout}
				onDraggingCardUpdated={OnDragUpdated}
				onEndDrag={OnEndDrag}
				flippedCenterPiles={flippedCenterPiles}
			/>
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
