import { memo } from 'react'
import { MdLeaderboard } from 'react-icons/md'
import { useNavigate } from 'react-router-dom'
import IconButton from './IconButton'

const LeaderboardButton = () => {
    const navigate = useNavigate()

    return (
        <IconButton
            icon={MdLeaderboard}
            style={{
                position: 'absolute',
                top: 3,
                right: 0,
                zIndex: 60,
            }}
            onClick={() => navigate('/leaderboard')}
        />
    )
}

export default memo(LeaderboardButton)
