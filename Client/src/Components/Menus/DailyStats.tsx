import { useEffect, useState } from 'react'
import styled from 'styled-components'
import Popup from '../Popup'
import { IDailyResults } from '../../Interfaces/IDailyResults'
import Countdown from 'react-countdown'
import config from '../../Config'
import { getResetTime } from '../../Helpers/DailyHelper'
import GameResults from '../GameResults'
import ShareButton from '../ShareButton'
import LeaderboardButton from '../LeaderboardButton'
import usePersistentState from '../../Hooks/usePersistentState'
import { Region } from '../../Helpers/Region'

interface Props {
    persistentId: string
    gameOver: boolean
}

const DailyStats = ({ persistentId, gameOver }: Props) => {
    const [selectedRegion] = usePersistentState('region', Region.OCEANIA)

    const [dailyResults, setDailyResults] = useState<IDailyResults>()
    const [nextGameDate] = useState<number>(() => getResetTime())
    const [myPlayerName] = useState<string>(() => JSON.parse(localStorage.getItem('playerName') ?? `"Player"`))

    useEffect(() => {
        fetch(`${config.apiGateway[selectedRegion]}/api/daily-stats/${persistentId}`)
            .then((response) => response.json())
            .then((data: IDailyResults) => setDailyResults(data))
            .catch((error) => {
                console.log(error)
            })
    }, [gameOver, selectedRegion])

    if (!dailyResults || !dailyResults.completedToday) {
        return <></>
    }

    return (
        <Popup id={'DailyStatsPopup'} onHomeButton={true} customZIndex={65}>
            <LeaderboardButton />
            <h5 style={{ marginTop: 30, marginBottom: -5 }}>Statistics</h5>
            <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'end', marginBottom: 30 }}>
                <Stat value={dailyResults.dailyWins.toString()} description="Wins" />
                <Stat value={dailyResults.dailyLosses.toString()} description="Losses" />
                <Stat value={dailyResults.dailyWinStreak.toString()} description="Streak" />
                <Stat value={dailyResults.maxDailyWinStreak.toString()} description="Best streak" />
            </div>

            <GameResults
                persistentId={persistentId}
                winnerName={dailyResults.playerWon ? myPlayerName : dailyResults.botName}
            />

            <div style={{ display: 'flex', justifyContent: 'center', marginBottom: -10 }}>
                <Stat
                    value={<Countdown date={nextGameDate} zeroPadTime={2} daysInHours />}
                    description="Next opponent"
                />
                <ShareButton
                    playerWon={dailyResults.playerWon}
                    opponentName={dailyResults.botName}
                    cardsRemaining={dailyResults.lostBy}
                />
            </div>
        </Popup>
    )
}

interface StatProps {
    value: string | JSX.Element
    description: string
}
const Stat = ({ value, description }: StatProps): JSX.Element => {
    return (
        <StatWrapper>
            <StatValue>{value}</StatValue>
            <StatDescription>{description}</StatDescription>
        </StatWrapper>
    )
}

const StatValue = styled.h2`
    margin: 0;
`

const StatDescription = styled.p`
    white-space: nowrap;
    margin: 0;
`

const StatWrapper = styled.div`
    display: flex;
    flex-direction: column;
    align-items: center;
    margin: 15px;
`

const BottomButton = styled.button`
    height: 30px;
    padding: 0 10px;
    margin: 25px 5px 5px;
`

export default DailyStats
