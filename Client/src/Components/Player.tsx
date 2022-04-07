import { IPlayer } from '../Interfaces/IPlayer'
import { ICard, IPos } from '../Interfaces/ICard'
import Card from './Card'
import GameBoardLayout from '../Helpers/GameBoardLayout'
import styled from 'styled-components'

interface Props {
	player: IPlayer
	onRequestTopUp?: () => void
	onTop: boolean
}

const Player = ({ player, onRequestTopUp, onTop }: Props) => {
	return (
		<PlayerContainer>
			<AdditionalInfo id={'player-info-' + player.Id} key={'player-info-' + player.Id} onTop={onTop}>
				{!!player.LastMove && <p>{player.LastMove}</p>}
			</AdditionalInfo>
			<TextMargin>{player.Name}</TextMargin>
			{player.CanRequestTopUp && !player.RequestingTopUp && !!onRequestTopUp && (
				<RequestTopUpButton onClick={onRequestTopUp}>Request top up</RequestTopUpButton>
			)}
		</PlayerContainer>
	)
}

const PlayerContainer = styled.div`
	height: 200px;
	width: 100%;
	display: flex;
	justify-content: center;
	flex: 0;
	color: white;
	background-color: #853939;
`
const AdditionalInfo = styled.div<{ onTop: boolean }>`
	position: absolute;
	transform: translateX(-50%);
	left: 50%;
	margin: ${(p) => !p.onTop && '-'}50px 0 0 0;
`

const RequestTopUpButton = styled.button`
	margin: 15px;
`

const TextMargin = styled.p`
	margin: 15px;
`

export default Player
