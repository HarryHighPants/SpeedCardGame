import { memo } from 'react'
import { HiOutlineHome } from 'react-icons/hi'
import { useNavigate } from 'react-router-dom'
import IconButton from './IconButton'

interface Props {
    onClick?: () => void
}

const HomeButton = ({ onClick }: Props) => {
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
                if (!!onClick) {
                    onClick()
                }
                navigate('/')
            }}
        />
    )
}

export default memo(HomeButton)
