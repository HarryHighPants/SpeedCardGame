import { useEffect, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import CelebrateShaker from './CelebrateShake'
import Popup from './Popup'
import useRoomId from '../Hooks/useRoomId'
import CopyableText from './CopyableText'
import toast from 'react-hot-toast'
import styled from 'styled-components'
import { HiShare } from 'react-icons/hi'

interface Props {
	winnerName: string | undefined
	loserName: string | undefined
	cardsRemaining: number
}

const WinnerPopup = ({ winnerName, loserName, cardsRemaining }: Props) => {
	const navigate = useNavigate()
	const [replayUrl, setReplayUrl] = useState('')
	const [searchParams, setSearchParams] = useSearchParams()

	useEffect(() => {
		let currentRoomId = window.location.pathname.replace('/', '')
		let roomNumber = parseInt(currentRoomId.replace(/\D/g, '')) || 0
		let roomIdWithoutNumbers = currentRoomId.replace(/[0-9]/g, '')
		setReplayUrl(`/${roomIdWithoutNumbers}${roomNumber + 1}${window.location.search}`)
	}, [])

	const onShare = () => {
		let shareText = `Speed Online ♦️\n🥇 ${winnerName}\n🥈 ${loserName}\n${winnerName} beat ${loserName} by ${cardsRemaining} card${cardsRemaining > 1 ? "s" : ""}`
		navigator.clipboard.writeText(shareText)
		toast.success('Share text copied to clipboard!')
	}

	return (
		<Popup id={'WinnerPopup'} onHomeButton={true} customZIndex={62}>
			<h2>Game over</h2>
			<h4
				style={{
					marginTop: 50,
					marginBottom: 0,
				}}
			>
				Winner is:
			</h4>
			<div style={{ display: 'flex', justifyContent: 'center', alignItems: 'end' }}>
				<CelebrateShaker />
				<h1 style={{ margin: '0 25px' }}>{winnerName}</h1>
				<CelebrateShaker startDelay={0.2} />
			</div>

			<BottomButton style={{ marginTop: 25 }} onClick={() => onShare()}>
				Share
				<HiShare style={{ marginBottom: -2, marginLeft: 5 }} />
			</BottomButton>

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

const BottomButton = styled.button`
	height: 30px;
	padding: 0 10px;
	margin: 25px 5px 5px;
`

export default WinnerPopup
