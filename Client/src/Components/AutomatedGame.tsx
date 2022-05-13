import React, { useEffect, useState } from 'react'
import Game from './Game'
import { IGameState } from '../Interfaces/IGameState'
import { BackgroundData } from '../Assets/BackgroundGameData'

const AutomatedGame = () => {
    const backgroundDataCount = BackgroundData.length
    const bottomPlayerId = (BackgroundData[0]).players[1].idHash

    const GetCurrentGameState = () => {
        let stateIndex = currentStateIndex
        if (currentStateIndex > backgroundDataCount - 1) {
            stateIndex = 0
            setCurrentStateIndex(stateIndex)
        }
        return BackgroundData[stateIndex]
    }

    const [currentStateIndex, setCurrentStateIndex] = useState(0)
    const [currentGameState, setCurrentGameState] = useState<IGameState>(GetCurrentGameState())
    const [isActive, setIsActive] = useState(true)

    useEffect(() => {
        let interval: number | undefined = undefined
        if (isActive) {
            interval = window.setInterval(() => {
                setCurrentStateIndex((currentStateIndex) => currentStateIndex + 1)
            }, 1000)
        }
        return () => clearInterval(interval)
    }, [isActive])

    useEffect(() => {
        setCurrentGameState(GetCurrentGameState())
    }, [currentStateIndex])

    return <Game connection={undefined} playerId={bottomPlayerId} gameState={currentGameState} roomId={''} />
}

export default AutomatedGame
