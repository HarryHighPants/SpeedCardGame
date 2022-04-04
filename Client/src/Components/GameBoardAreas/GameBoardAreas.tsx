import styled from 'styled-components'
import PlayerHandArea from './PlayerHandArea'
import { IGameState } from '../../Interfaces/IGameState'

interface Props {
	gameState: IGameState
	onPickupFromKitty: () => void
}

const GameBoardAreas = ({ onPickupFromKitty, gameState }: Props) => {
	return (
		<>
			<PlayerHandArea onPickupFromKitty={onPickupFromKitty} />
			{/*<KittyArea/>*/}
			{/*<CenterPileArea/>*/}
			{/*<KittyArea/>*/}
		</>
	)
}

export default GameBoardAreas
