import toast from 'react-hot-toast'
import { HiShare } from 'react-icons/hi'
import styled from 'styled-components'
import MenuButton from './Menus/MenuButton'

interface Props {
    playerWon: boolean
    opponentName: string
    cardsRemaining: number
}

const ShareButton = ({ playerWon, opponentName, cardsRemaining }: Props) => {
    const onShare = () => {
        let outcomeText =
            (playerWon ? `👑 beat ` : `☠️ lost against `) +
            `${opponentName} by ${cardsRemaining} card${cardsRemaining > 1 ? 's' : ''}`
        let shareText = `${outcomeText}\n♦️speed.harryab.com`
        navigator.clipboard.writeText(shareText)
        toast.success('Share text copied to clipboard!')
    }

    return (
        <BottomButton style={{ marginTop: 25 }} onClick={() => onShare()}>
            Share
            <HiShare style={{ marginBottom: -2, marginLeft: 5 }} />
        </BottomButton>
    )
}

const BottomButton = styled(MenuButton)`
    height: 30px;
    padding: 0 10px;
    margin: 25px 5px 5px;
`

export default ShareButton
