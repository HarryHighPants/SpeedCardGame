import React, { useEffect, useState } from 'react'
import Game from './Game'
import { IGameState } from '../Interfaces/IGameState'
import {BackgroundData} from '../Assets/BackgroundGameData'

const AutomatedGame = () => {
	const backgroundDataCount = BackgroundData.length
	const bottomPlayerId = (JSON.parse(BackgroundData[0]) as IGameState).Players[1].Id;

	const GetCurrentGameState = () => {
		let stateIndex = currentStateIndex
		if (currentStateIndex > backgroundDataCount - 1) {
			stateIndex = 0
			setCurrentStateIndex(stateIndex)
		}
		return JSON.parse(BackgroundData[stateIndex])
	}

	const [currentStateIndex, setCurrentStateIndex] = useState(0)
	const [currentGameState, setCurrentGameState] = useState<IGameState>(GetCurrentGameState())
	const [isActive, setIsActive] = useState(true)

	useEffect(() => {
		let interval: number | undefined = undefined
		if (isActive) {
			interval = window.setInterval(() => {
				setCurrentStateIndex((currentStateIndex) => currentStateIndex + 1)
			}, 500)
		}
		return () => clearInterval(interval)
	}, [isActive])

	useEffect(() => {
		setCurrentGameState(GetCurrentGameState())
	}, [currentStateIndex])

	return (
		<Game
			connection={undefined}
			connectionId={bottomPlayerId}
			gameState={currentGameState}
			invertedCenterPiles={false}
		/>
	)
}

export default AutomatedGame
