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
}

const getGameBoardDimensions = () => {
    return {
        x: clamp(window.innerWidth, 0, GameBoardLayout.maxWidth),
        y: window.innerHeight,
    } as IPos
}

const Game = ({ connection, connectionId, gameState }: Props) => {
    const [gameBoardDimensions, setGameBoardDimensions] = useState<IPos>(getGameBoardDimensions())
    const [gameBoardLayout, setGameBoardLayout] = useState<GameBoardLayout>()
    const [flippedCenterPiles, setflippedCenterPiles] = useState(false)
    const [localGameState, setLocalGameState] = useState<IGameState>(gameState)

    useEffect(() => {
        let newGameState = gameState
        if (gameState.players[0].id === connection?.connectionId) {
            // We need to invert the center piles so that 0 is on the right
            // This way we will have a perfectly mirrored board simplifying sending card IPos to the other player
            setflippedCenterPiles(true)
            newGameState.centerPiles = gameState.centerPiles.reverse()

            // Order the players so that we are the last player so we get shown at the bottom of the screen
            newGameState.players = gameState.players.reverse()
        }
        setLocalGameState(newGameState)
    }, [gameState])

    useEffect(() => {
        UpdateGameBoardLayout()
    }, [gameBoardDimensions])

    const UpdateGameBoardLayout = () => {
        let gameBoardLayout = new GameBoardLayout(gameBoardDimensions)
        setGameBoardLayout(gameBoardLayout)
    }

    useLayoutEffect(() => {
        function UpdateGameBoardDimensions() {
            setGameBoardDimensions(getGameBoardDimensions)
        }

        UpdateGameBoardDimensions()
        window.addEventListener('resize', UpdateGameBoardDimensions)
        return () => window.removeEventListener('resize', UpdateGameBoardDimensions)
    }, [])

    const SendPlayCard = (topCard: ICard, centerPileIndex: number) => {
        let correctedCenterPileIndex = flippedCenterPiles ? (centerPileIndex + 1) % 2 : centerPileIndex
        connection?.invoke('TryPlayCard', topCard.id, correctedCenterPileIndex).catch((e) => console.log(e))
    }

    const SendPickupFromKitty = () => {
        connection?.invoke('TryPickupFromKitty').catch((e) => console.log(e))
    }

    const SendRequestTopUp = () => {
        connection?.invoke('TryRequestTopUp').catch((e) => console.log(e))
    }

    const SendMovedCard = debounce(
        (movedCard: IMovedCardPos | undefined) => {
            connection?.invoke('UpdateMovingCard', movedCard).catch((e) => console.log(e))
        },
        100,
        { leading: true, trailing: true, maxWait: 100 }
    )

    return (
        <GameContainer>
            <Player
                mustTopUp={gameState.mustTopUp}
                connection={connection}
                key={`player-${localGameState.players[0].id}`}
                player={localGameState.players[0]}
                onTop={true}
            />
            {!!gameBoardLayout && (
                <GameBoard
                    sendMovingCard={SendMovedCard}
                    connection={connection}
                    playerId={connectionId}
                    gameState={localGameState}
                    gameBoardLayout={gameBoardLayout}
                    sendPlayCard={SendPlayCard}
                    sendPickupFromKitty={SendPickupFromKitty}
                    flippedCenterPiles={flippedCenterPiles}
                />
            )}

            <Player
                mustTopUp={gameState.mustTopUp}
                connection={connection}
                onRequestTopUp={SendRequestTopUp}
                key={`player-${localGameState.players[1].id}`}
                player={localGameState.players[1]}
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
