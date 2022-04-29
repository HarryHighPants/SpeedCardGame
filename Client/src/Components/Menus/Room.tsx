import * as signalR from '@microsoft/signalr'
import {HubConnection, HubConnectionState, ILogger, LogLevel} from '@microsoft/signalr'
import {useEffect} from 'react'
import {useLocation, useNavigate, useParams, useSearchParams} from 'react-router-dom'
import {IGameState} from '../../Interfaces/IGameState'
import Game from '../../Components/Game'
import TestData from '../../Assets/TestData.js'
import Lobby from './Lobby'
import styled from 'styled-components'
import {HiOutlineHome} from 'react-icons/hi'
import Popup from '../Popup'
import HomeButton from '../HomeButton'
import useState from 'react-usestateref'
import {motion} from 'framer-motion'
import CelebrateShaker from '../CelebrateShake'
import WinnerPopup from '../WinnerPopup'
import useRoomId from '../../Hooks/useRoomId'
import toast, {Toaster} from 'react-hot-toast'
import {IPlayer} from '../../Interfaces/IPlayer'
import {GameType} from '../../Interfaces/ILobby'
import config from "../../Config";
import { v4 as uuid } from 'uuid'


interface Props {
	onGameStarted: () => void
}

const Room = ({onGameStarted}: Props) => {
	let navigate = useNavigate()
	const [connection, setConnection, connectionRef] = useState<HubConnection>()
	const [gameState, setGameState] = useState<IGameState>()
	const [roomId, roomIdRef] = useRoomId()
	const [connectionStatus, setConnectionStatus] = useState<HubConnectionState | undefined>()
	const [persistentId, setPersistentId] = useState<string>(() => localStorage.getItem('persistentId') ?? uuid())
	const [connectionId, setConnectionId] = useState<string | null>()
	const [winningPlayer, setWinningPlayer] = useState<IPlayer>()
	const [playerWon, setPlayerWon] = useState<boolean>(false)
	const [losingPlayer, setLosingPlayer] = useState<IPlayer>()
	const [losingPlayerCardsRemaining, setLosingPlayerCardsRemaining] = useState<number>(0)
	const [searchParams, setSearchParams] = useSearchParams()

	useEffect(() => {
		if (!!roomId) {
			if (connectionRef.current?.state === HubConnectionState.Connected) {
				connectionRef.current?.stop()
			}
			CreateConnection()
		}
	}, [roomId])

	useEffect(() => {
		console.log("setting persistent Id", persistentId)
		localStorage.setItem('persistentId', persistentId)
	}, [persistentId])

	useEffect(() => {
		let winningpPlayer = gameState?.Players.find((p) => p.Id === gameState.WinnerId)
		setWinningPlayer(winningpPlayer)

		let losingPlayer = gameState?.Players.find((p) => p.Id !== gameState.WinnerId)
		setLosingPlayer(losingPlayer)

		setPlayerWon(gameState?.WinnerId === connectionId)

		setLosingPlayerCardsRemaining((losingPlayer?.HandCards.length ?? 0) + (losingPlayer?.KittyCardsCount ?? 0))
	}, [gameState?.WinnerId])

	const CreateConnection = () => {
		console.log("connecting to: ", config.apiGateway.URL)
		// Builds the SignalR connection, mapping it to /server
		let signalRConnection = new signalR.HubConnectionBuilder()
			.withUrl(config.apiGateway.URL)
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
		let isBotGame = searchParams.get('type') as GameType === "bot"
		let botDifficulty = searchParams.get('difficulty') ?? "-1"
		connectionRef.current?.invoke('JoinRoom', roomIdRef?.current, persistentId, isBotGame, parseInt(botDifficulty))
	}

	const UpdateGameState = (data: any) => {
		let parsedData: IGameState = JSON.parse(data)
		if (!gameState) {
			onGameStarted()
		}
		setGameState({...parsedData})
	}

	return (
		<>
			<Toaster/>

			{!!gameState && (
				<>
					<Game
						key={connectionId + roomId}
						connection={connection}
						connectionId={connectionId}
						gameState={gameState}
					/>
					<HomeButton onClick={() => connection?.stop()}/>
					{!!gameState.WinnerId && (
						<WinnerPopup
							winnerName={winningPlayer?.Name}
							loserName={losingPlayer?.Name}
							cardsRemaining={losingPlayerCardsRemaining}
							playerWon={playerWon}
						/>
					)}
				</>
			)}
			<Lobby roomId={roomId} connection={connection} gameState={gameState} onBack={() => connection?.stop()}/>
		</>
	)
}

export default Room
