import { IPlayer } from '../Interfaces/IPlayer'
import { ICard, IPos } from '../Interfaces/ICard'
import Card from './Card'
import GameBoardLayout from '../Helpers/GameBoardLayout'
import styled from 'styled-components'
import backgroundImg from "../Assets/wood-tiling.jpg";

interface Props {
	player: IPlayer
	onRequestTopUp?: () => void
	onTop: boolean
}

const Player = ({ player, onRequestTopUp, onTop }: Props) => {
	return (
		<PlayerContainer style={{ backgroundImage: `url(${backgroundImg})` }}>
			<AdditionalInfo id={'player-info-' + player.Id} key={'player-info-' + player.Id} topOfBoard={onTop}>
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
	box-shadow: 0px 0px 50px 6px rgba(0, 0, 0, 0.59);
	font-family: Cinzel, 'serif';
	font-weight: 200;
	font-size: large;
	letter-spacing: 1px;

`
const AdditionalInfo = styled.div<{ topOfBoard: boolean }>`
	font-weight: 500;
	font-size: large;
	font-family: 'Roboto Slab', serif;
	position: absolute;
	transform: translateX(-50%);
	left: 50%;
	margin: ${(p) => p.topOfBoard ? '50' : '-60'}px 0 0 0;
`

const RequestTopUpButton = styled.button`
	margin: 15px;
`

const TextMargin = styled.p`
	margin: 15px;
`

export default Player
