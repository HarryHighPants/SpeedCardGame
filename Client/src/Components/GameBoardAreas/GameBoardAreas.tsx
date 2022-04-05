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
						/>
						<BaseArea
							dimensions={GameBoardLayout.GetAreaDimensions(
								ourPlayer,
								CardLocationType.Kitty,
								gameBoardDimensions
							)}
							text={`remaining: ${p.KittyCardsCount}`}
						/>
						<BaseArea
							dimensions={GameBoardLayout.GetAreaDimensions(
								false,
								CardLocationType.Center,
								gameBoardDimensions,
								i
							)}
						/>
					</>
				)
			})}
		</>
	)
}

export default GameBoardAreas
