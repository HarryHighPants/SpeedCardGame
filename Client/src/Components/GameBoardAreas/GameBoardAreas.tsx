import {IGameState} from '../../Interfaces/IGameState'
import BaseArea from './BaseArea'
import GameBoardLayout from '../../Helpers/GameBoardLayout'
import {CardLocationType, IPos, IRenderableCard} from '../../Interfaces/ICard'
import DroppableArea from './DroppableArea'

interface Props {
	gameState: IGameState
	ourId: string | null | undefined
	setHandAreaHighlighted: (higlighted: boolean) => void
	gameBoardDimensions: IPos
	cardBeingDragged: IRenderableCard | undefined
}

const GameBoardAreas = ({ ourId, setHandAreaHighlighted, gameState, gameBoardDimensions, cardBeingDragged }: Props) => {
	return (
		<>
			{gameState.Players.map((p, i) => {
				let ourPlayer = p.Id === ourId
				return (
					<div key={`player-${i}`}>
						{ourPlayer ? (
							<DroppableArea
								key={`area-${CardLocationType.Hand}-${i}`}
								dimensions={GameBoardLayout.GetAreaDimensions(
									ourPlayer,
									CardLocationType.Hand,
									gameBoardDimensions
								)}
								cardBeingDragged={cardBeingDragged?.location === CardLocationType.Kitty ? cardBeingDragged : undefined}
								setIsHighlighted={setHandAreaHighlighted}
							/>
						) : (
							<BaseArea
								key={`area-${CardLocationType.Hand}-${i}`}
								dimensions={GameBoardLayout.GetAreaDimensions(
									ourPlayer,
									CardLocationType.Hand,
									gameBoardDimensions
								)}
							/>
						)}
						<BaseArea
							key={`area-${CardLocationType.Kitty}-${i}`}
							dimensions={GameBoardLayout.GetAreaDimensions(
								ourPlayer,
								CardLocationType.Kitty,
								gameBoardDimensions
							)}
							text={`remaining: ${p.KittyCardsCount}`}
						/>
						<BaseArea
							key={`area-${CardLocationType.Center}-${i}`}
							dimensions={GameBoardLayout.GetAreaDimensions(
								false,
								CardLocationType.Center,
								gameBoardDimensions,
								i
							)}
						/>
					</div>
				)
			})}
		</>
	)
}

export default GameBoardAreas
