import { useEffect, useState } from 'react'
import styled from 'styled-components'
import Popup from '../Popup'
import config from '../../Config'
import { ILeaderboardPlayer, ILeaderboardResults } from '../../Interfaces/ILeaderboardResults'
import LoadingSpinner from '../LoadingSpinner'
import { useNavigate } from 'react-router'
import { Rank, RankColour } from '../../Interfaces/ILobby'
import usePersistentState from '../../Hooks/usePersistentState'
import { Region } from '../../Helpers/Region'

export const Leaderboard = () => {
    const [selectedRegion] = usePersistentState('region', Region.OCEANIA)

    const navigate = useNavigate()
    const [leaderboardResults, setLeaderboardResults] = useState<ILeaderboardResults>()
    const [fetchError, setFetchError] = useState<string>()

    useEffect(() => {
        fetch(`${config.apiGateway[selectedRegion]}/api/daily-leaderboard`)
            .then((response) => response.json())
            .then((data: ILeaderboardResults) => setLeaderboardResults(data))
            .catch((error) => {
                setFetchError(error.toString())
                console.log(error)
            })
    }, [selectedRegion])

    return (
        <Popup id={'LeaderboardPopup'} onBackButton={() => navigate(-1)} customZIndex={65}>
            {leaderboardResults ? (
                <>
                    <h4 style={{ marginTop: 30, marginBottom: 0 }}>{leaderboardResults.botName}</h4>
                    <h5 style={{ marginBottom: 35, marginTop: 0, color: RankColour[leaderboardResults.botRank] }}>
                        {Rank[leaderboardResults.botRank]}
                    </h5>
                    <LeaderboardWrapper>
                        {leaderboardResults.players.map((p, i) => (
                            <LeaderboardPlayer key={'player-' + i} player={p} />
                        ))}
                    </LeaderboardWrapper>
                </>
            ) : (
                <>
                    <h4 style={{ marginTop: 15, marginBottom: 0 }}>Daily Leaderboard</h4>
                    {!!fetchError ? (
                        <>
                            <h4 style={{ marginTop: 65, marginBottom: -10, color: '#de6e41' }}>
                                Error retrieving data
                            </h4>
                            <p style={{ color: '#de6e41' }}>{fetchError}</p>
                        </>
                    ) : (
                        <>
                            <h4 style={{ marginTop: 60, marginBottom: 0 }}>Loading..</h4>
                            <LoadingSpinner />
                        </>
                    )}
                </>
            )}
        </Popup>
    )
}
const LeaderboardPlayer = ({ player }: { player: ILeaderboardPlayer }): JSX.Element => {
    return (
        <StyledPlayer>
            <StyledPlace $place={player.place}>{player.place}</StyledPlace>
            <div>
                <p style={{ margin: 0, marginBottom: -3 }}>{player.name}</p>
                <p style={{ margin: 0, fontSize: 12, color: RankColour[player.rank] }}>{Rank[player.rank]}</p>
            </div>
            <p style={{ marginLeft: 'auto', fontWeight: 'bold', fontSize: 'large', paddingBottom: 1 }}>
                {player.score > 0 ? '👑' : '☠️'}
            </p>
            <p style={{ fontWeight: 'bold', fontSize: 'large' }}>{player.score}</p>
        </StyledPlayer>
    )
}

const StyledPlace = styled.p<{ $place: number }>`
    color: ${({ $place }) =>
        $place === 1 ? '#ffff00' : $place === 2 ? '#e9e9e9' : $place === 3 ? '#d2691e' : 'rgba(203,203,203,0.8)'};
    font-weight: ${({ $place }) => ($place < 4 ? 'bold' : 'normal')};
    font-size: ${({ $place }) => ($place < 4 ? 'x-large' : 'medium')};
    width: 15px;
    margin-right: 10px;
`

const StyledPlayer = styled.div`
    display: flex;
    height: 30px;
    gap: 10px;
    text-align: left;
    padding: 10px;
    align-items: center;
`

const LeaderboardWrapper = styled.div`
    display: flex;
    max-height: 55vh;
    flex-direction: column;
    overflow: scroll;
    margin-right: -10px;
    padding-right: 10px;
`
