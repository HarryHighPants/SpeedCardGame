import styled from 'styled-components'
import {IPlayerConnection, Rank, RankColour} from '../../Interfaces/ILobby'

const LobbyPlayer = (
    connectionId: string,
    myPlayerName: string,
    player: IPlayerConnection,
    onUpdateName: (newName: string) => void,
    index: number
) => {

    return (
        <div key={`lobby-player-${player.playerId}`} style={{ display: 'flex' }}>
            <p style={{ margin: 0, paddingRight: 10, width: 13 }}>{index + 1}. </p>
            <div style={{ display: 'flex', flexDirection: 'column', minWidth: 0 }}>
                {player.playerId == connectionId ? (
                    <input
                        style={{
                            marginTop: -5,
                            height: 22,
                            fontSize: 'medium',
                            fontFamily: 'inherit',
                        }}
                        key={player.playerId}
                        maxLength={20}
                        value={myPlayerName}
                        onChange={(e) => onUpdateName(e.target.value)}
                    />
                ) : (
                    <PlayerName key={player.playerId}>{player.name}</PlayerName>
                )}
                <RankText style={{ color: RankColour[player.rank] }}>{Rank[player.rank]}</RankText>
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
