import * as signalR from '@microsoft/signalr'
import { HubConnectionState } from '@microsoft/signalr'
import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import Lobby from './Lobby'

interface Props {}

const Game = (props: Props) => {
    let urlParams = useParams()
    const [connection, setConnection] = useState<signalR.HubConnection>()
    const [gameId, setGameId] = useState<string>()

    useEffect(() => {
        // Builds the SignalR connection, mapping it to /server
        setConnection(
            new signalR.HubConnectionBuilder()
                .withUrl('https://localhost:7067/server')
                .withAutomaticReconnect()
                .configureLogging(signalR.LogLevel.Information)
                .build()
        )

        connection?.start().then(() => {
            JoinGame()
        })
    }, [])

    useEffect(() => {
        if (connection?.state == HubConnectionState.Connected) {
            setGameId(urlParams.gameId)
            JoinGame()
        }
    }, [urlParams.gameId])

    const JoinGame = () => {
        if (!gameId) {
            return
        }

        // Join the game on the server
        connection?.invoke('JoinGame', gameId)
    }

    return <Lobby gameId={gameId} connection={connection} />
}

export default Game
