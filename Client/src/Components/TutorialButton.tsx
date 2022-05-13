import { memo } from 'react'
import { FiHelpCircle } from 'react-icons/fi'
import IconButton from './IconButton'

interface Props {
    onClick?: () => void
}

const TutorialButton = ({ onClick }: Props) => {
    return (
        <IconButton
            icon={FiHelpCircle}
            style={{
                position: 'absolute',
                top: 0,
                right: 0,
            }}
            onClick={onClick}
        />
    )
}

export default memo(TutorialButton)
