import styled from 'styled-components'
import { IPlayerConnection } from '../../Interfaces/ILobby'

const LobbyPlayer = (
	connectionId: string,
	myPlayerName: string,
	player: IPlayerConnection,
	onUpdateName: (newName: string) => void,
	index: number
) => {
	return (
		<div key={`lobby-player-${player.ConnectionId}`} style={{ display: 'flex' }}>
			<p style={{ margin: 0, paddingRight: 10, width: 13 }}>{index + 1}. </p>
			{player.ConnectionId == connectionId ? (
				<input
					style={{
						marginTop: -5,
						height: 22,
						fontSize: 'medium',
						fontFamily: 'inherit',
						width: '100%',
					}}
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

export default LobbyPlayer
