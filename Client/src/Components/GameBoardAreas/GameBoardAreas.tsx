import {IGameState} from '../../Interfaces/IGameState'
import BaseArea from './BaseArea'
import GameBoardLayout from '../../Helpers/GameBoardLayout'
import {CardLocationType, IPos, IRenderableCard} from '../../Interfaces/ICard'
import DroppableArea from './DroppableArea'

interface Props {
	gameState: IGameState
	ourId: string | null | undefined
	setHandAreaHighlighted: (higlighted: boolean) => void
	gameBoardLayout: GameBoardLayout | undefined
}

const GameBoardAreas = ({ ourId, setHandAreaHighlighted, gameState, gameBoardLayout }: Props) => {
	if(!gameBoardLayout){
		return <></>
	}
	return (
		<>
			{gameState.Players.map((p, i) => {
				let ourPlayer = p.Id === ourId
				return (
					<div key={`player-${i}`}>
						{ourPlayer ? (
							<DroppableArea
								key={`area-${CardLocationType.Hand}-${i}`}
								dimensions={gameBoardLayout.GetAreaDimensions(
									ourPlayer,
									CardLocationType.Hand,
								)}
								// cardBeingDragged={cardBeingDragged?.location === CardLocationType.Kitty ? cardBeingDragged : undefined}
								setIsHighlighted={setHandAreaHighlighted}
							/>
						) : (
							<BaseArea
								key={`area-${CardLocationType.Hand}-${i}`}
								dimensions={gameBoardLayout.GetAreaDimensions(
									ourPlayer,
									CardLocationType.Hand,
								)}
							/>
						)}
						<BaseArea
							key={`area-${CardLocationType.Kitty}-${i}`}
							dimensions={gameBoardLayout.GetAreaDimensions(
								ourPlayer,
								CardLocationType.Kitty,
							)}
							text={`remaining: ${p.KittyCardsCount}`}
						/>
						<BaseArea
							key={`area-${CardLocationType.Center}-${i}`}
							dimensions={gameBoardLayout.GetAreaDimensions(
								false,
								CardLocationType.Center,
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
