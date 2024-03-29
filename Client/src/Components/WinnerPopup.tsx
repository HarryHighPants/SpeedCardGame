import { useEffect, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import CelebrateShaker from './CelebrateShake'
import Popup from './Popup'
import useRoomId from '../Hooks/useRoomId'
import CopyableText from './CopyableText'
import toast from 'react-hot-toast'
import styled from 'styled-components'
import { HiShare } from 'react-icons/hi'
import config from '../Config'
import { IRankingStats } from '../Interfaces/IRankingStats'
import RankingStats from './RankingStats'
import MenuButton from './Menus/MenuButton'
import GameResults from './GameResults'
import ShareButton from './ShareButton'
import { Region } from '../Helpers/Region'
import usePersistentState from '../Hooks/usePersistentState'

interface Props {
    persistentId: string
    playerWon: boolean
    winnerName: string
    loserName: string
    cardsRemaining: number
}

const WinnerPopup = ({ persistentId, winnerName, loserName, cardsRemaining, playerWon }: Props) => {
    const navigate = useNavigate()
    const [selectedRegion] = usePersistentState('region', Region.OCEANIA)

    const [replayUrl, setReplayUrl] = useState('')
    const [rankingStats, setRankingStats] = useState<IRankingStats>()

    useEffect(() => {
        updateReplayUrl()
        getRankingStats()
    }, [selectedRegion])

    const getRankingStats = () => {
        fetch(`${config.apiGateway[selectedRegion]}/api/latest-ranking-stats/${persistentId}`)
            .then((response) => response.json())
            .then((data) => setRankingStats(data))
            .catch((error) => {
                console.log(error)
            })
    }

    const updateReplayUrl = () => {
        let currentRoomId = window.location.pathname.replace('/', '')
        let roomNumber = parseInt(currentRoomId.replace(/\D/g, '')) || 0
        let roomIdWithoutNumbers = currentRoomId.replace(/[0-9]/g, '')
        setReplayUrl(`/${roomIdWithoutNumbers}${roomNumber + 1}${window.location.search}`)
    }

    return (
        <Popup id={'WinnerPopup'} onHomeButton={true} customZIndex={62}>
            <h2>Game over</h2>

            <GameResults persistentId={persistentId} winnerName={winnerName} />

            <ShareButton
                playerWon={playerWon}
                opponentName={playerWon ? loserName : winnerName}
                cardsRemaining={cardsRemaining}
            />

            <BottomButton
                style={{ marginTop: 25 }}
                onClick={() => {
                    navigate(replayUrl)
                    window.location.reload()
                }}
            >
                Replay
            </BottomButton>
        </Popup>
    )
}

const BottomButton = styled(MenuButton)`
    height: 30px;
    padding: 0 10px;
    margin: 25px 5px 5px;
`

export default WinnerPopup
