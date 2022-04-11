import copyIcon from '../../Assets/copyIcon.png'
import * as signalR from '@microsoft/signalr'
import { useEffect, useState } from 'react'
import { useParams, useSearchParams } from 'react-router-dom'
import {GameType, ILobby, IPlayerConnection} from "../../Interfaces/ILobby";

interface Props {
	connection: signalR.HubConnection | undefined
	roomId: string | undefined
}

const Lobby = ({ connection, roomId }: Props) => {
	const [lobbyData, setLobbyData] = useState<ILobby>()
	const [myPlayerName, setMyPlayerName] = useState<string>('Player')
	const [connectionId, setConnectionId] = useState<string>('')
	const [searchParams, setSearchParams] = useSearchParams()

	useEffect(() => {
		if (!connection) return
		connection.on('UpdateLobbyState', UpdateLobbyData)

		if (connection.connectionId) {
			setConnectionId(connection.connectionId)
		}
		return () => {
			connection.off('UpdateLobbyState', UpdateLobbyData)
		}
	}, [connection])

	const UpdateLobbyData = (data: any) => {
		let lobbyData: ILobby = JSON.parse(data)
		setLobbyData(lobbyData)
		let myPlayerInfo = lobbyData.Connections.find((c) => c.ConnectionId === connectionId)

		let inputGameType = searchParams.get('type') as GameType
		if (!!inputGameType && !lobbyData.GameStarted && !lobbyData.IsBotGame && inputGameType === 'bot') {
			let inputDifficulty = searchParams.get('difficulty')
			let botDifficulty = !!inputDifficulty ? +inputDifficulty : 0
			onStartGame(true, botDifficulty)
		}

		if (myPlayerInfo) {
			setMyPlayerName(myPlayerInfo.Name)
		}
	}

	const onStartGame = (botGame: boolean = false, botDifficulty: number = 0) => {
		connection?.invoke('StartGame', botGame, botDifficulty)
	}

	const UpdateName = (newName: string) => {
		// Update our name on the server
		connection?.invoke('UpdateName', newName)

		// Locally set our name
		setMyPlayerName(newName)
	}

	return (
		<div>
			<h2>Lobby</h2>
			<div>
				<div>
					<p>Invite link:</p>
					<input value={window.location.href} disabled={true} />
					<button onClick={() => navigator.clipboard.writeText(window.location.href)}>
						<img width={10} alt="Copy" src={copyIcon} />
					</button>
				</div>
				<div>
					<h4>Players</h4>
					{lobbyData != null ? (
						<ul>
							{lobbyData?.Connections?.map((p) => LobbyPlayer(connectionId, myPlayerName, p, UpdateName))}
						</ul>
					) : (
						<div>Loading</div>
					)}
				</div>
				<button disabled={lobbyData == null || lobbyData.Connections?.length < 2} onClick={() => onStartGame()}>
					Start Game
				</button>
			</div>
		</div>
	)
}

export default Lobby

const LobbyPlayer = (
	connectionId: string,
	myPlayerName: string,
	player: IPlayerConnection,
	onUpdateName: (newName: string) => void
) => {
	return (
		<li key={player.ConnectionId}>
			{player.ConnectionId == connectionId ? (
				<input maxLength={20} value={myPlayerName} onChange={(e) => onUpdateName(e.target.value)} />
			) : (
				player.Name
			)}
		</li>
	)
}
