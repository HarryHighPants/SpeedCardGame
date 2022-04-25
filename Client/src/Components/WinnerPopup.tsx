import { useEffect, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import CelebrateShaker from './CelebrateShake'
import Popup from './Popup'
import useRoomId from '../Hooks/useRoomId'

interface Props {
	name: string | undefined
}

const WinnerPopup = ({ name }: Props) => {
	const navigate = useNavigate()
	const [replayUrl, setReplayUrl] = useState('')
	const [searchParams, setSearchParams] = useSearchParams()

	useEffect(() => {
		let currentRoomId = window.location.pathname.replace('/', '')
		let roomNumber = parseInt(currentRoomId.replace(/\D/g, '')) || 0
		let roomIdWithoutNumbers = currentRoomId.replace(/[0-9]/g, '')
		setReplayUrl(`/${roomIdWithoutNumbers}${roomNumber + 1}${window.location.search}`)
	}, [])

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
				<h1 style={{ margin: '0 25px' }}>{name}</h1>
				<CelebrateShaker startDelay={0.2} />
			</div>
			<button
				style={{ marginTop: 25 }}
				onClick={() => {
					navigate(replayUrl)
					window.location.reload()
				}}
			>
				Replay
			</button>
		</Popup>
	)
}

export default WinnerPopup
