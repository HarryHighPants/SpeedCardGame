import styled from 'styled-components'
import { IPlayerConnection, Rank } from '../../Interfaces/ILobby'

const LobbyPlayer = (
    connectionId: string,
    myPlayerName: string,
    player: IPlayerConnection,
    onUpdateName: (newName: string) => void,
    index: number
) => {
    const getRankColour = (rank: Rank) => {
        switch (rank) {
            case 0:
                return '#b8b8b8'
            case 1:
                return '#ff8d63'
            case 2:
                return '#ffd26f'
            case 3:
                return '#6fd9ff'
            case 4:
                return '#ff5050'
            case 5:
                return '#ea75ff'
        }
    }

    return (
        <div key={`lobby-player-${player.connectionId}`} style={{ display: 'flex' }}>
            <p style={{ margin: 0, paddingRight: 10, width: 13 }}>{index + 1}. </p>
            <div style={{ display: 'flex', flexDirection: 'column', minWidth: 0 }}>
                {player.connectionId == connectionId ? (
                    <input
                        style={{
                            marginTop: -5,
                            height: 22,
                            fontSize: 'medium',
                            fontFamily: 'inherit',
                        }}
                        key={player.connectionId}
                        maxLength={20}
                        value={myPlayerName}
                        onChange={(e) => onUpdateName(e.target.value)}
                    />
                ) : (
                    <PlayerName key={player.connectionId}>{player.name}</PlayerName>
                )}
                <RankText style={{ color: getRankColour(player.rank) }}>{Rank[player.rank]}</RankText>
            </div>
        </div>
    )
}

const PlayerName = styled.p`
    margin: 0;
    text-align: left;
`

const RankText = styled.p`
    margin: 0;
    text-align: left;
    font-style: italic;
    font-size: small;
`

export default LobbyPlayer
