import * as signalR from '@microsoft/signalr'
import { HubConnection, HubConnectionState } from '@microsoft/signalr'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { IGameState } from '../../Interfaces/IGameState'
import Game from '../../Components/Game'
import Lobby from './Lobby'
import HomeButton from '../HomeButton'
import useStateRef from 'react-usestateref'
import WinnerPopup from '../WinnerPopup'
import useRoomId from '../../Hooks/useRoomId'
import { Toaster } from 'react-hot-toast'
import { IPlayer } from '../../Interfaces/IPlayer'
import { BotDifficulty } from '../../Interfaces/ILobby'
import config from '../../Config'
import { v4 as uuid } from 'uuid'
import DailyStats from './DailyStats'
import { Region } from '../../Helpers/Region'
import usePersistentState from '../../Hooks/usePersistentState'

interface Props {
    onGameStarted: () => void
}

const Room = ({ onGameStarted }: Props) => {
    const [connection, setConnection, connectionRef] = useStateRef<HubConnection>()
    const [gameState, setGameState] = useState<IGameState>()
    const [roomId, roomIdRef] = useRoomId()
    const [connectionError, setConnectionError] = useState<string>()
    const [persistentId, setPersistentId] = useState<string>(() => localStorage.getItem('persistentId') ?? uuid())
    const [hashedPersistentId, setHashedPersistentId] = useState<string>()
    const [winningPlayer, setWinningPlayer] = useState<IPlayer>()
    const [playerWon, setPlayerWon] = useState<boolean>(false)
    const [losingPlayer, setLosingPlayer] = useState<IPlayer>()
    const [losingPlayerCardsRemaining, setLosingPlayerCardsRemaining] = useState<number>(0)
    const [searchParams, setSearchParams] = useSearchParams()
    const [selectedRegion, setSelectedRegion] = usePersistentState('region', Region.OCEANIA)

    useEffect(() => {
        if (!!roomId && !!persistentId) {
            if (connectionRef.current?.state === HubConnectionState.Connected) {
                connectionRef.current?.stop()
            }
            CreateConnection()
        }
    }, [roomId, persistentId, selectedRegion])

    useEffect(() => {
        console.log('setting persistent Id', persistentId)
        localStorage.setItem('persistentId', persistentId)

        fetch(`${config.apiGateway[selectedRegion]}/api/id-hash/${persistentId}`)
            .then((response) => response.text())
            .then((data) => setHashedPersistentId(data))
    }, [persistentId, selectedRegion])

    useEffect(() => {
        let winningPlayer = gameState?.players.find((p) => p.idHash === gameState.winnerId)
        setWinningPlayer(winningPlayer)

        let losingPlayer = gameState?.players.find((p) => p.idHash !== gameState.winnerId)
        setLosingPlayer(losingPlayer)

        setPlayerWon(gameState?.winnerId === hashedPersistentId)

        setLosingPlayerCardsRemaining((losingPlayer?.handCards.length ?? 0) + (losingPlayer?.kittyCardsCount ?? 0))
    }, [gameState?.winnerId])

    const CreateConnection = () => {
        let hubUrl = `${config.apiGateway[selectedRegion]}/server`
        console.log('connecting to: ', hubUrl)
        // Builds the SignalR connection, mapping it to /server
        let signalRConnection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, {
                skipNegotiation: true,
                transport: signalR.HttpTransportType.WebSockets,
                accessTokenFactory: () => persistentId,
            })
            .configureLogging(signalR.LogLevel.Information)
            .build()

        // signalRConnection.serverTimeoutInMilliseconds = 15000
        // signalRConnection.keepAliveIntervalInMilliseconds = 7500

        signalRConnection
            ?.start()
            .then(() => {
                setConnection(signalRConnection)
                ConnectionStatusUpdated()
            })
            .catch((e) => {
                console.log(e)
                setConnectionError('Could not connect to the server')
            })
    }

    const ConnectionStatusUpdated = () => {
        switch (connectionRef.current?.state) {
            case HubConnectionState.Connected:
                connectionRef.current?.onclose((error) => {
                    if (!!error) {
                        ConnectionStatusUpdated()
                        setConnection(undefined)
                        CreateConnection()
                    } else {
                        connectionRef.current?.off('UpdateGameState', UpdateGameState)
                    }
                })
                connectionRef.current?.on('UpdateGameState', UpdateGameState)
                JoinRoom()

                break
            case HubConnectionState.Disconnected:
                connectionRef.current?.off('UpdateGameState', UpdateGameState)
                break
            default:
                break
        }
    }

    const JoinRoom = () => {
        if (!roomIdRef?.current) return
        let botDifficulty = searchParams.get('difficulty')
        connectionRef.current?.invoke('JoinRoom', roomIdRef?.current, !!botDifficulty ? parseInt(botDifficulty) : null)
    }

    const UpdateGameState = (updatedGameState: IGameState) => {
        if (!gameState) {
            onGameStarted()
        }
        setGameState({ ...updatedGameState })
    }

    const stopConnection = useCallback(() => {
        connectionRef.current?.invoke('LeaveRoom', roomIdRef?.current)
        connection?.stop()
    }, [connection])

    return (
        <>
            <Toaster />

            {!!gameState && (
                <>
                    <Game
                        key={persistentId + roomId}
                        connection={connection}
                        playerId={hashedPersistentId}
                        gameState={gameState}
                        roomId={roomId}
                    />
                    <HomeButton onClick={stopConnection} requireConfirmation />
                    {!!gameState.winnerId && winningPlayer && losingPlayer && (
                        <WinnerPopup
                            persistentId={persistentId}
                            winnerName={winningPlayer.name}
                            loserName={losingPlayer.name}
                            cardsRemaining={losingPlayerCardsRemaining}
                            playerWon={playerWon}
                        />
                    )}
                </>
            )}
            <Lobby
                setSelectedRegion={setSelectedRegion}
                selectedRegion={selectedRegion}
                roomId={roomId}
                connection={connection}
                playerId={hashedPersistentId}
                gameState={gameState}
                onBack={() => connection?.stop()}
                connectionError={connectionError}
            />
            {parseInt(searchParams.get('difficulty') ?? '0') === BotDifficulty.Daily && (
                <DailyStats persistentId={persistentId} gameOver={!!gameState?.winnerId} />
            )}
        </>
    )
}

export default Room
