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

interface Props {
	connection: signalR.HubConnection | undefined
	playerId: string | undefined | null
	gameState: IGameState
	gameBoardLayout: GameBoardLayout
	sendPlayCard: (topCard: ICard, centerPileIndex: number) => void
	sendPickupFromKitty: () => void
	sendMovingCard: (movedCard: IMovedCardPos | undefined) => void
}

const GameBoard = ({
	connection,
	playerId,
	gameState,
	gameBoardLayout,
	sendPlayCard,
	sendPickupFromKitty,
	sendMovingCard,
}: Props) => {
	const [handArea, setHandArea] = useState<RefObject<HTMLDivElement>>()
	const [localGameState, dispatchGameState] = useReducer(gameStateReducer, gameState)
	const [renderableAreas, setRenderableAreas] = useState([] as IRenderableArea[])

	useEffect(() => {
		setRenderableAreas(gameBoardLayout.GetBoardAreas())
	}, [])

	useEffect(() => {
		dispatchGameState({ type: 'Replace', gameState: gameState })
	}, [gameState])

	const OnPlayCard = (topCard: ICard, centerPileIndex: number) => {
		sendPlayCard(topCard, centerPileIndex)
		dispatchGameState({ type: 'Play', topCard: topCard, centerPileIndex: centerPileIndex, playerId: playerId })
	}

	const DetectPlay = (draggingCard: IRenderableCard) => {
		let draggingCardRect = draggingCard?.ref.current?.getBoundingClientRect()
		let centerCardPlay = {index: -1, distance: Infinity}
		for (let i = 0; i < renderableAreas.length; i++) {
			let renderableArea = renderableAreas[i]
			let ourRect = renderableArea.ref?.current?.getBoundingClientRect()
			let offsetInfo = GetOffsetInfo(ourRect, draggingCardRect)
			if(offsetInfo.overlaps){
				if(renderableArea.location.type === "Center" && CanDropOntoCenter(renderableArea, draggingCard)){
					//Get the closest center pile
					if(centerCardPlay.distance > offsetInfo.distance)
					centerCardPlay = {index: renderableArea.location.index, distance: offsetInfo.distance}
				}

				if(CanDropOntoHand(renderableArea, draggingCard)) {
					console.log('Attempt pickup from kitty', draggingCard)
					sendPickupFromKitty()
					dispatchGameState({ type: 'Pickup', playerId: playerId })
				}
			}
		}
		if(centerCardPlay.index !== -1){
			OnPlayCard(draggingCard, centerCardPlay.index);
		}
	}

	const OnEndDrag = (draggingCard: IRenderableCard) => {
		// Detect Pickup From Kitty
		DetectPlay(draggingCard)
		OnDragUpdated(undefined)
	}

	const OnDragUpdated = (draggingCard: IRenderableCard | undefined) => {
		let draggingCardRect = draggingCard?.ref.current?.getBoundingClientRect()

		// Detect hoverable areas
		for (let i = 0; i < renderableAreas.length; i++) {
			let renderableArea = renderableAreas[i]

			if (!draggingCard?.ref) {
				SetHighlight(renderableArea, false)
				continue
			}

			if (CanDropOntoCenter(renderableArea, draggingCard) || CanDropOntoHand(renderableArea, draggingCard)) {
				let ourRect = renderableArea.ref?.current?.getBoundingClientRect()
				let offsetInfo = GetOffsetInfo(ourRect, draggingCardRect)
				SetHighlight(renderableArea, offsetInfo.overlaps)
			}
		}
	}

	const SetHighlight = (rArea: IRenderableArea, highlight: boolean) => {
		if (rArea.highlight !== highlight) {
			rArea.highlight = highlight
			if (!!rArea.forceUpdate) {
				rArea.forceUpdate()
			}
		}
	}

	const CanDropOntoCenter = (ra: IRenderableArea, c: IRenderableCard | undefined) => {
		return ra.location.type === 'Center' && c?.location === CardLocationType.Hand
	}

	const CanDropOntoHand = (ra: IRenderableArea, c: IRenderableCard | undefined) => {
		return ra.location.type === 'Hand' && c?.location === CardLocationType.Kitty
	}

	return (
		<GameBoardContainer>
			<GameBoardAreas
				ourId={playerId}
				gameBoardLayout={gameBoardLayout}
				gameState={localGameState}
				renderableAreas={renderableAreas}
			/>
			<CardsContainer
				sendMovingCard={sendMovingCard}
				connection={connection}
				playerId={playerId}
				gameState={localGameState}
				gameBoardLayout={gameBoardLayout}
				onDraggingCardUpdated={OnDragUpdated}
				onEndDrag={OnEndDrag}
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
