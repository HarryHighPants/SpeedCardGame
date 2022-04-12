import copyIcon from '../../Assets/copyIcon.png'
import * as signalR from '@microsoft/signalr'
import { useEffect, useState } from 'react'
import { useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { GameType, ILobby, IPlayerConnection } from '../../Interfaces/ILobby'
import Popup from './Popup'
import styled from 'styled-components'
import { HiOutlineDocumentDuplicate } from 'react-icons/hi'

interface Props {
	connection: signalR.HubConnection | undefined
	roomId: string | undefined
}

const Lobby = ({ connection, roomId }: Props) => {
	let navigate = useNavigate()
	const [lobbyData, setLobbyData] = useState<ILobby>()
	const [myPlayerName, setMyPlayerName] = useState<string>('Player')
	const [connectionId, setConnectionId] = useState<string>('')
	const [searchParams, setSearchParams] = useSearchParams()
	const [waitingForPlayers, setWaitingForPlayers] = useState(true)

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

	useEffect(() => {
		setWaitingForPlayers(lobbyData == null || lobbyData.Connections?.length < 2)
	}, [lobbyData])

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
		<Popup onBackButton={() => navigate(`/`)}>
			<Header2>Lobby</Header2>
			<div>
				<div>
					<p>Invite link:</p>
					<input value={window.location.href} disabled={true} />
					<CopyButton onClick={() => navigator.clipboard.writeText(window.location.href)} />
				</div>
				<div>
					<h4>Players</h4>
					{lobbyData != null ? (
						<ul>
							{lobbyData?.Connections?.map((p) => LobbyPlayer(connectionId, myPlayerName, p, UpdateName))}
						</ul>
					) : (
						<div>Connecting to room</div>
					)}
				</div>
				{waitingForPlayers && !!lobbyData && <p>Waiting for another player to join..</p>}
				<button disabled={waitingForPlayers} onClick={() => onStartGame()}>
					Start Game
				</button>
			</div>
		</Popup>
	)
}

export default Lobby

const CopyButton = styled(HiOutlineDocumentDuplicate)`
	width: 25px;
	height: 25px;
	color: white;
	cursor: pointer;

	&:hover {
		color: #bebebe;
	}
`

const Header2 = styled.h2`
	margin-top: -20px;
	width: 250px;
`

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
