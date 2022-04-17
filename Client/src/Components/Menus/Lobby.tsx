import copyIcon from '../../Assets/copyIcon.png'
import * as signalR from '@microsoft/signalr'
import { useEffect, useState } from 'react'
import { useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { GameType, ILobby, IPlayerConnection } from '../../Interfaces/ILobby'
import styled from 'styled-components'
import { HiOutlineDocumentDuplicate } from 'react-icons/hi'
import Popup from '../Popup'

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
			<LobbyWrapper>
				<Group>
					<GameCodeTitle>Game Code:</GameCodeTitle>
					<GameCodeWrapper>
						<input value={roomId} disabled={true} />
						<CopyButton onClick={() => navigator.clipboard.writeText(window.location.href)} />
					</GameCodeWrapper>
				</Group>
				<Group>
					<PlayersTitle>Players:</PlayersTitle>
					<PlayersContainer>
						{lobbyData != null ? (
							<>
								{lobbyData?.Connections?.map((p, i) =>
									LobbyPlayer(connectionId, myPlayerName, p, UpdateName, i)
								)}
							</>
						) : (
							<div>Connecting to room</div>
						)}
					</PlayersContainer>
				</Group>
				{waitingForPlayers && !!lobbyData && <p>Waiting for another player to join..</p>}
				<StartButton disabled={waitingForPlayers} onClick={() => onStartGame()}>
					Start Game
				</StartButton>
			</LobbyWrapper>
		</Popup>
	)
}

export default Lobby

const LobbyWrapper = styled.div`
	display: flex;
	flex-direction: column;
	align-items: center;
`

const StartButton = styled.button`
	margin-top: 20px;
`

const Group = styled.div`
	width: 175px;
	display: flex;
	flex-direction: column;
	margin-bottom: 30px;
	align-items: flex-start;
`

const PlayersTitle = styled.h4`
	margin-bottom: 10px;
`
const PlayersContainer = styled.div`
	display: flex;
	justify-content: center;
	flex-direction: column;
	gap: 10px;
	width: 100%;
`

const GameCodeTitle = styled.p`
	margin-bottom: 5px;
`

const GameCodeWrapper = styled.div`
	display: flex;
	justify-content: center;
	width: 100%;
`

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
	width: 250px;
	margin-bottom: 0px;
`

const LobbyPlayer = (
	connectionId: string,
	myPlayerName: string,
	player: IPlayerConnection,
	onUpdateName: (newName: string) => void,
	index: number
) => {
	return (
		<div key={`lobby-player-${player.ConnectionId}`} style={{ display: 'flex' }}>
			<p style={{ margin: 0, paddingRight: 10 }}>{index + 1}. </p>
			{player.ConnectionId == connectionId ? (
				<input
					key={player.ConnectionId}
					maxLength={20}
					value={myPlayerName}
					onChange={(e) => onUpdateName(e.target.value)}
				/>
			) : (
				<PlayerName key={player.ConnectionId}>{player.Name}</PlayerName>
			)}
		</div>
	)
}

const PlayerName = styled.p`
	margin: 0;
	text-align: left;
`
