import * as signalR from '@microsoft/signalr'
import { HubConnection, HubConnectionState } from '@microsoft/signalr'
import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import Lobby from './Lobby'

interface Props {}

const Game = (props: Props) => {
    let urlParams = useParams()
    const [connection, setConnection] = useState<HubConnection>()
    const [gameId, setGameId] = useState<string | undefined>(urlParams.gameId)

    useEffect(() => {
        // Builds the SignalR connection, mapping it to /server
        let signalRConnection = new signalR.HubConnectionBuilder()
            .withUrl('https://localhost:7067/server')
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build()

        signalRConnection?.start().then(() => {
          setConnection(signalRConnection)
          JoinGame()
        })
    }, [])

    useEffect(() => {
        setGameId(urlParams.gameId)
        if (connection?.state == HubConnectionState.Connected) {
            JoinGame()
        }
    }, [urlParams.gameId, connection])

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
