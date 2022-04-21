import * as signalR from '@microsoft/signalr'
import { HubConnection, HubConnectionState, ILogger, LogLevel } from '@microsoft/signalr'
import { useEffect } from 'react'
import { useLocation, useNavigate, useParams } from 'react-router-dom'
import { IGameState } from '../../Interfaces/IGameState'
import Game from '../../Components/Game'
import TestData from '../../Assets/TestData.js'
import Lobby from './Lobby'
import styled from 'styled-components'
import { HiOutlineHome } from 'react-icons/hi'
import Popup from '../Popup'
import HomeButton from '../HomeButton'
import useState from 'react-usestateref'
import { motion } from 'framer-motion'
import CelebrateShaker from "../CelebrateShake";
import WinnerPopup from "../WinnerPopup";

interface Props {
	onGameStarted: () => void
}

const Room = ({ onGameStarted }: Props) => {
	let navigate = useNavigate()
	const [connection, setConnection, connectionRef] = useState<HubConnection>()
	const [gameState, setGameState] = useState<IGameState>()
	const [roomId, setRoomId, roomIdRef] = useState('')
	const [connectionStatus, setConnectionStatus] = useState<HubConnectionState | undefined>()
	const [connectionId, setConnectionId] = useState<string | null>()

	useEffect(() => {
		let newRoomId = window.location.pathname.replace('/', '')
		if(!!newRoomId){
			if(connectionRef.current?.state === HubConnectionState.Connected){
				connectionRef.current?.stop()
			}
			CreateConnection()
		}

		setRoomId(newRoomId)
	}, [window.location.pathname])

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
			ConnectionStatusUpdated()
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
		setConnectionStatus(connectionRef.current?.state)
	}

	const JoinRoom = () => {
		if (!roomIdRef?.current) return
		connectionRef.current?.invoke('JoinRoom', roomIdRef?.current)
	}

	const UpdateGameState = (data: any) => {
		let parsedData: IGameState = JSON.parse(data)
		if (!gameState) {
			onGameStarted()
		}
		setGameState({ ...parsedData })
	}

	return (
		<>
			{!!gameState && (
				<>
					<Game
						key={connectionId+roomId}
						connection={connection}
						connectionId={connectionId}
						gameState={gameState}
					/>
					<HomeButton onClick={() => connection?.stop()} />
					{!!gameState.WinnerId && (
						<WinnerPopup name={gameState.Players.find((p) => p.Id === gameState.WinnerId)?.Name}/>
					)}
				</>
			)}
			<Lobby roomId={roomId} connection={connection} gameState={gameState} onBack={() => connection?.stop()} />
		</>
	)
}

export default Room
