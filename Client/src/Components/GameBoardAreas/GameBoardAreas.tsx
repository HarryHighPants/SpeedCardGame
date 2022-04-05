import { IGameState } from '../../Interfaces/IGameState'
import BaseArea from './BaseArea'
import GameBoardLayout from '../../Helpers/GameBoardLayout'
import { CardLocationType, IPos } from '../../Interfaces/ICard'

interface Props {
	gameState: IGameState
	ourId: string | null | undefined
	onPickupFromKitty: () => void
	gameBoardDimensions: IPos
}

const GameBoardAreas = ({ ourId, onPickupFromKitty, gameState, gameBoardDimensions }: Props) => {
	return (
		<>
			{gameState.Players.map((p, i) => {
				let ourPlayer = p.Id === ourId
				return (
					<>
						<BaseArea
							dimensions={GameBoardLayout.GetAreaDimensions(
								ourPlayer,
								CardLocationType.Hand,
								gameBoardDimensions
							)}
							droppable={ourPlayer}
						/>
						<BaseArea
							dimensions={GameBoardLayout.GetAreaDimensions(
								ourPlayer,
								CardLocationType.Kitty,
								gameBoardDimensions
							)}
							droppable={false}
							text={`remaining: ${p.KittyCardsCount}`}
						/>
						<BaseArea
							dimensions={GameBoardLayout.GetAreaDimensions(
								false,
								CardLocationType.Center,
								gameBoardDimensions,
								i
							)}
							droppable={false}
						/>
					</>
				)
			})}
		</>
	)
}

export default GameBoardAreas
