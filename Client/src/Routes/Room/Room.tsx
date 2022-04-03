import * as signalR from '@microsoft/signalr'
import { HubConnection, HubConnectionState } from '@microsoft/signalr'
import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import Lobby from '../../Components/Lobby'
import { IGameState } from '../../Interfaces/IGameState'
import Game from '../../Components/Game'
import TestData from '../../Assets/TestData.js'

interface Props {}

const Room = (props: Props) => {
	let urlParams = useParams()
	const [connection, setConnection] = useState<HubConnection>()
	const [roomId, setRoomId] = useState<string | undefined>(urlParams.roomId)
	// const [gameState, setGameState] = useState<IGameState>(JSON.parse(TestData)) // Local debugging
	const [gameState, setGameState] = useState<IGameState>()

	useEffect(() => {
		// Builds the SignalR connection, mapping it to /server
		let signalRConnection = new signalR.HubConnectionBuilder()
			.withUrl('https://localhost:7067/server')
			.withAutomaticReconnect()
			.configureLogging(signalR.LogLevel.Information)
			.build()

		signalRConnection?.start().then(() => {
			setConnection(signalRConnection)
			JoinRoom()
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
		setRoomId(urlParams.roomId)
		if (connection?.state == HubConnectionState.Connected) {
			JoinRoom()
		}
	}, [urlParams.roomId, connection])

	const JoinRoom = () => {
		if (!roomId) return
		connection?.invoke('JoinRoom', roomId)
	}

	const UpdateGameState = (data: any) => {
		let parsedData: IGameState = JSON.parse(data)
		setGameState({...parsedData})
	}

	return gameState!! ? (
		<Game roomId={roomId} connection={connection} connectionId={connection?.connectionId} gameState={gameState} />
	) : (
		<Lobby roomId={roomId} connection={connection} />
	)
}

export default Room
