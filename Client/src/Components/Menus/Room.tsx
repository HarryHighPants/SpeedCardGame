import * as signalR from '@microsoft/signalr'
import { HubConnection, HubConnectionState, ILogger, LogLevel } from '@microsoft/signalr'
import { useEffect, useState } from 'react'
import { useLocation, useNavigate, useParams } from 'react-router-dom'
import { IGameState } from '../../Interfaces/IGameState'
import Game from '../../Components/Game'
import TestData from '../../Assets/TestData.js'
import Lobby from './Lobby'
import styled from 'styled-components'
import { HiOutlineHome } from 'react-icons/hi'
import Popup from '../Popup'
import HomeButton from '../HomeButton'

interface Props {
	onGameStarted: () => void
}

const Room = ({ onGameStarted }: Props) => {
	let urlParams = useParams()
	let navigate = useNavigate()
	const [connection, setConnection] = useState<HubConnection>()
	const [gameState, setGameState] = useState<IGameState>()
	const [connectionStatus, setConnectionStatus] = useState<HubConnectionState | undefined>()
	const [connectionId, setConnectionId] = useState<string | null>()

	useEffect(() => {
		CreateConnection()
	}, [])

	const CreateConnection = () => {
		// Builds the SignalR connection, mapping it to /server
		let signalRConnection = new signalR.HubConnectionBuilder()
			.withUrl(`http://${window.location.hostname}:5169/server`)
			.configureLogging(signalR.LogLevel.Information)
			.build()

		signalRConnection.serverTimeoutInMilliseconds = 15000
		signalRConnection.keepAliveIntervalInMilliseconds = 7500

		signalRConnection?.start().then(() => {
			setConnection(signalRConnection)
			setConnectionId(signalRConnection.connectionId)
			ConnectionStatusUpdated(signalRConnection)
		})
	}

	const ConnectionStatusUpdated = (hubConnection: HubConnection) => {
		console.log('ConnectionStatusUpdated', hubConnection.state)
		switch (hubConnection.state) {
			case HubConnectionState.Connected:
				hubConnection.onclose((error) => {
					if (!!error) {
						ConnectionStatusUpdated(hubConnection)
						setConnection(undefined)
						CreateConnection()
					} else {
						hubConnection.off('UpdateGameState', UpdateGameState)
					}
				})
				console.log("hubConnection.on('UpdateGameState', UpdateGameState)")
				hubConnection.on('UpdateGameState', UpdateGameState)
				JoinRoom(hubConnection)

				break
			case HubConnectionState.Disconnected:
				console.log("hubConnection.off('UpdateGameState', UpdateGameState)")
				hubConnection.off('UpdateGameState', UpdateGameState)
				break
			default:
				break
		}
		setConnectionStatus(hubConnection.state)
	}

	const JoinRoom = (hubConnection: HubConnection) => {
		console.log('JoinRoom func', urlParams.roomId, hubConnection.connectionId)
		if (!urlParams.roomId) return
		hubConnection.invoke('JoinRoom', urlParams.roomId)
	}

	const UpdateGameState = (data: any) => {
		let parsedData: IGameState = JSON.parse(data)
		if (!gameState) {
			onGameStarted()
		}
		console.log(
			'gamestate',
			parsedData.Players.map((p) => p.Id)
		)
		setGameState({ ...parsedData })
	}

	return (
		<>
			{!!gameState && (
				<>
					<Game key={connectionId} connection={connection} connectionId={connectionId} gameState={gameState} />
					<HomeButton onClick={() => connection?.stop()} />
					{!!gameState.WinnerId && (
						<Popup id={"WinnerPopup"} onHomeButton={true}>
							<h3>Winner is {gameState.Players.find((p) => p.Id === gameState.WinnerId)?.Name}</h3>
						</Popup>
					)}
				</>
			)}
			<Lobby roomId={urlParams.roomId} connection={connection} gameState={gameState} onBack={()=>connection?.stop()}/>
		</>
	)
}

export default Room
