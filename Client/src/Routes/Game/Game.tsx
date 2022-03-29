import * as signalR from '@microsoft/signalr'
import { HubConnection, HubConnectionState } from '@microsoft/signalr'
import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import Lobby from '../../Components/Lobby'
import {GameState} from "../../Models/GameState";

interface Props {}

const Game = (props: Props) => {
    let urlParams = useParams()
    const [connection, setConnection] = useState<HubConnection>()
    const [gameId, setGameId] = useState<string | undefined>(urlParams.gameId)
    const [gameState, setGameState] = useState<GameState>()

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
        if (connection?.state !== HubConnectionState.Connected) return
        connection.on('UpdateGameState', UpdateGameState)

        return () => {
            connection.off('UpdateGameState', UpdateGameState)
        }
    }, [connection])

    useEffect(() => {
        setGameId(urlParams.gameId)
        if (connection?.state == HubConnectionState.Connected) {
            JoinGame()
        }
    }, [urlParams.gameId, connection])

    const JoinGame = () => {
        if (!gameId) return
        connection?.invoke('JoinGame', gameId)
    }

    const UpdateGameState = (data: any) => {
        let parsedData: GameState = JSON.parse(data)
      console.log(parsedData);
      debugger
        setGameState(parsedData)
    }

    return <Lobby gameId={gameId} connection={connection} />
}

export default Game
