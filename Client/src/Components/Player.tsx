import { IPlayer } from '../Interfaces/IPlayer'
import { ICard, IPos } from '../Interfaces/ICard'
import Card from './Card'
import GameBoardLayout from "../Helpers/GameBoardLayout";
import styled from 'styled-components'

interface Props {
    player: IPlayer
	onRequestTopUp?: ()=>void
}

const Player = ({ player, onRequestTopUp }: Props) => {
    return (
        <PlayerContainer>
			<p>{player.Name}</p>
			{player.RequestingTopUp && <p>Requesting top up</p>}
			{(player.CanRequestTopUp && !player.RequestingTopUp && !!onRequestTopUp) && <button onClick={onRequestTopUp}>Request top up</button>}
		</PlayerContainer>
    )
}

const PlayerContainer = styled.div`
	height: 200px;
	width: 100%;
	flex: 0;
	color: white;
	background-color: #853939;
`

export default Player
