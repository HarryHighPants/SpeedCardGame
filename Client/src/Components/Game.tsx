import * as signalR from '@microsoft/signalr'
import { useEffect, useLayoutEffect, useState } from 'react'
import { IGameState } from '../Interfaces/IGameState'
import { ICard, IMovedCardPos, IPos } from '../Interfaces/ICard'
import styled from 'styled-components'
import Player from './Player'
import GameBoardLayout from '../Helpers/GameBoardLayout'
import { clamp } from '../Helpers/Utilities'
import GameBoard from './GameBoard'
import backgroundImg from '../Assets/felt-tiling.jpg'
import { debounce } from 'lodash'
import { useNavigate } from 'react-router'

interface Props {
    connection: signalR.HubConnection | undefined
    playerId: string | undefined
    gameState: IGameState
    roomId: string
    demoGame?: boolean
}

const getGameBoardDimensions = () => {
    return {
        x: clamp(window.innerWidth, 0, GameBoardLayout.maxWidth),
        y: window.innerHeight,
    } as IPos
}

const Game = ({ roomId, connection, playerId, gameState, demoGame }: Props) => {
    const navigate = useNavigate()

    const [gameBoardDimensions, setGameBoardDimensions] = useState<IPos>(getGameBoardDimensions())
    const [gameBoardLayout, setGameBoardLayout] = useState<GameBoardLayout>()
    const [flippedCenterPiles, setflippedCenterPiles] = useState(false)
    const [localGameState, setLocalGameState] = useState<IGameState>(gameState)

    useEffect(() => {
        const handleBack = () => {
            if (!window.confirm('Are you sure you want to go back?'))
                window.history.pushState(null, '', window.location.href)
            else navigate('/')
        }
        if (demoGame || !!gameState.winnerId) return
        window.history.pushState(null, '', window.location.href)
        window.addEventListener('popstate', handleBack)
        return () => window.removeEventListener('popstate', handleBack)
    }, [demoGame, gameState.winnerId, navigate])

    useEffect(() => {
        const onUnload = (e: BeforeUnloadEvent) => 'Game in progress. Are you sure you want to leave?'
        if (demoGame || !!gameState.winnerId) return
        window.addEventListener('beforeunload', onUnload)
        return () => window.removeEventListener('beforeunload', onUnload)
    }, [demoGame, gameState.winnerId])

    useEffect(() => {
        let newGameState = gameState
        if (gameState.players[0].idHash === playerId) {
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
        connection?.invoke('TryPlayCard', roomId, topCard.id, correctedCenterPileIndex).catch((e) => console.log(e))
    }

    const SendPickupFromKitty = () => {
        connection?.invoke('TryPickupFromKitty', roomId).catch((e) => console.log(e))
    }

    const SendRequestTopUp = () => {
        connection?.invoke('TryRequestTopUp', roomId).catch((e) => console.log(e))
    }

    const SendMovedCard = debounce(
        (movedCard: IMovedCardPos | undefined) => {
            connection?.invoke('UpdateMovingCard', roomId, movedCard).catch((e) => console.log(e))
        },
        100,
        { leading: true, trailing: true, maxWait: 100 }
    )

    return (
        <GameContainer>
            <Player
                mustTopUp={gameState.mustTopUp}
                connection={connection}
                key={`player-${localGameState.players[0].idHash}`}
                player={localGameState.players[0]}
                onTop={true}
            />
            {!!gameBoardLayout && (
                <GameBoard
                    sendMovingCard={SendMovedCard}
                    connection={connection}
                    playerId={playerId}
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
                key={`player-${localGameState.players[1].idHash}`}
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
