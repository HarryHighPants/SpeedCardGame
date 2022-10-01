import { useEffect, useState } from 'react'
import styled from 'styled-components'
import Popup from '../Popup'
import config from '../../Config'
import { ILeaderboardPlayer, ILeaderboardResults } from '../../Interfaces/ILeaderboardResults'
import LoadingSpinner from '../LoadingSpinner'
import { useNavigate } from 'react-router'
import {Rank, RankColour} from "../../Interfaces/ILobby";

export const Leaderboard = () => {
    const navigate = useNavigate()
    const [leaderboardResults, setLeaderboardResults] = useState<ILeaderboardResults>()

    useEffect(() => {
        fetch(`${config.apiGateway.URL}/api/daily-leaderboard`)
            .then((response) => response.json())
            .then((data: ILeaderboardResults) => setLeaderboardResults(data))
            .catch((error) => {
                console.log(error)
            })
    }, [])

    return (
        <Popup id={'LeaderboardPopup'} onBackButton={() => navigate(-1)} customZIndex={65}>
            {leaderboardResults ? (
            <>
                <h4 style={{ marginTop: 30, marginBottom: 0}}>{leaderboardResults.botName}</h4>
                <h5 style={{ marginBottom: 35, marginTop: 0, color: RankColour[leaderboardResults.botRank] }}>{Rank[leaderboardResults.botRank]}</h5>
                    <LeaderboardWrapper>
                        {leaderboardResults.players.map((p) => (
                                <LeaderboardPlayer player={p} />
                        ))}
                    </LeaderboardWrapper>
            </>
            ) : (
                <>
                    <h4 style={{ marginTop: 60, marginBottom: 0}}>Loading..</h4>
                    <LoadingSpinner />
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
                <p style={{margin: 0, marginBottom: -3}}>{player.name}</p>
                <p  style={{margin: 0, fontSize: 12, color: RankColour[player.rank]}}>{Rank[player.rank]}</p>
            </div>
            <p style={{ marginLeft: 'auto', fontWeight: 'bold', fontSize: 'large' }}>{player.score > 0? '👑' : '☠️'}</p>
            <p style={{ fontWeight: 'bold', fontSize: 'large' }}>{player.score}</p>
        </StyledPlayer>
    )
}

const StyledPlace = styled.p<{ $place: number }>`
    color: ${({ $place }) =>
        $place === 1 ? '#ffff00' : $place === 2 ? '#e9e9e9' : $place === 3 ? '#d2691e' : '#CBCBCB'};
  font-weight: ${({ $place }) => $place < 4 ? 'bold' : 'normal'};
  font-size: ${({ $place }) => $place < 4 ? 'large' : 'medium'};
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
    flex-direction: column;
`
