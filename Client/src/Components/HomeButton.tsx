import { memo } from 'react'
import { HiOutlineHome } from 'react-icons/hi'
import { useNavigate } from 'react-router-dom'
import IconButton from './IconButton'

interface Props {
    onClick?: () => void
    requireConfirmation?: boolean
}

const HomeButton = ({ onClick, requireConfirmation }: Props) => {
    const navigate = useNavigate()

    return (
        <IconButton
            icon={HiOutlineHome}
            style={{
                position: 'absolute',
                top: 0,
                left: 0,
                zIndex: 60,
            }}
            onClick={() => {
                if (requireConfirmation) {
                    if (!window.confirm('Are you sure you want to leave the game?')) return
                }
                if (!!onClick) {
                    onClick()
                }
                navigate('/')
            }}
        />
    )
}

export default memo(HomeButton)
