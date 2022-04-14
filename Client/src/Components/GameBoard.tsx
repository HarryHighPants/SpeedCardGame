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

	const DetectPickupFromKitty = (draggingCard: IRenderableCard) => {
		if (
			draggingCard.location === CardLocationType.Kitty &&
			renderableAreas.find((a) => a.highlight && a.location.type === 'Hand')
		) {
			console.log('Attempt pickup from kitty', draggingCard)
			sendPickupFromKitty()
			dispatchGameState({ type: 'Pickup', playerId: playerId })
		}
	}

	const OnEndDrag = (draggingCard: IRenderableCard) => {
		// Detect Pickup From Kitty
		DetectPickupFromKitty(draggingCard)
		OnDragUpdated(undefined)
	}

	const OnDragUpdated = (draggingCard: IRenderableCard | undefined) => {
		let draggingCardRect = draggingCard?.ref.current?.getBoundingClientRect()

		// Detect hoverable areas
		for (let i = 0; i < renderableAreas.length; i++) {
			let renderableArea = renderableAreas[i]
			let ourRect = renderableArea.ref?.current?.getBoundingClientRect()
			let offsetInfo = GetOffsetInfo(ourRect, draggingCardRect)

			if (offsetInfo.distance === Infinity) {
				// Reset states
				if (renderableArea.highlight) {
					renderableArea.highlight = false
					if (!!renderableArea.forceUpdate) {
						renderableArea.forceUpdate()
					}
				}
				continue
			}

			// Check if a center area can be dropped onto
			let droppingOntoCenter =
				renderableArea.location.type === "Center" && draggingCard?.location === CardLocationType.Hand
			// Check if our hand area is being dropped onto
			let droppingOntoHand =
				renderableArea.location.type === "Hand" && draggingCard?.location === CardLocationType.Kitty

			if (droppingOntoCenter || droppingOntoHand) {
				if (renderableArea.highlight !== offsetInfo.overlaps) {
					renderableArea.highlight = offsetInfo.overlaps
					if (!!renderableArea.forceUpdate) {
						renderableArea.forceUpdate()
					}
				}
			}
		}
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
				onPlayCard={OnPlayCard}
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
