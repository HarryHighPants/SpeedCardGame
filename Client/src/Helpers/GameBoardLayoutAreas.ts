import {CardLocationType, ICard, IMovedCardPos, IPos, IRenderableCard} from '../Interfaces/ICard'
import {IGameState} from '../Interfaces/IGameState'
import React from 'react'
import {AreaDimensions} from '../Components/GameBoardAreas/BaseArea'
import {clamp} from './Utilities'
import gameBoardLayout from "./GameBoardLayout";
import GameBoardLayoutCards from "./GameBoardLayoutCards";
import GameBoardLayout from "./GameBoardLayout";

class GameBoardLayoutArea {
	private layout: GameBoardLayout;
	private cardsLayout: GameBoardLayoutCards;

	constructor(gameBoardLayout: GameBoardLayout, gameBoardLayoutCards: GameBoardLayoutCards) {
		this.layout = gameBoardLayout;
		this.cardsLayout = gameBoardLayoutCards;
	}

	public GetAreaDimensions = (
		ourPlayer: boolean,
		location: CardLocationType,
		centerIndex: number = 0
	): AreaDimensions => {
		switch (location) {
			case CardLocationType.Hand:
				return this.GetHandArea(ourPlayer)
			case CardLocationType.Kitty:
				return this.GetKittyArea(ourPlayer)
			case CardLocationType.Center:
				return this.GetCenterArea(ourPlayer, centerIndex)
			default:
				return this.GetHandArea(ourPlayer)
		}
	}

	 GetHandArea(ourPlayer: boolean): AreaDimensions {
		let areaDimensions = {} as AreaDimensions
		areaDimensions.pos = {
			X: this.cardsLayout.GetHandCardPosition(ourPlayer, ourPlayer ? 0 : GameBoardLayout.maxHandCardCount - 1).X,
			Y: this.cardsLayout.GetHandCardPosition(ourPlayer, 0).Y,
		} as IPos

		areaDimensions.size = {
			X:
				Math.abs(
					this.cardsLayout.GetHandCardPosition(ourPlayer, 0).X -
						this.cardsLayout.GetHandCardPosition(ourPlayer, GameBoardLayout.maxHandCardCount - 1).X
				) + GameBoardLayout.PixelsToPercent(GameBoardLayout.cardWidth, this.layout.gameBoardDimensions.X),
			Y: GameBoardLayout.PixelsToPercent(GameBoardLayout.cardHeight, this.layout.gameBoardDimensions.Y),
		}

		return this.AreaDimensionsToPixels(areaDimensions)
	}

	 GetKittyArea(ourPlayer: boolean): AreaDimensions {
		let areaDimensions = {} as AreaDimensions
		areaDimensions.pos = this.cardsLayout.GetKittyCardPosition(ourPlayer)
		areaDimensions.size = {
			X: GameBoardLayout.PixelsToPercent(GameBoardLayout.cardWidth, this.layout.gameBoardDimensions.X),
			Y: GameBoardLayout.PixelsToPercent(GameBoardLayout.cardHeight, this.layout.gameBoardDimensions.Y),
		}

		return this.AreaDimensionsToPixels(areaDimensions)
	}

	 GetCenterArea(ourPlayer: boolean, centerIndex: number): AreaDimensions {
		let areaDimensions = {} as AreaDimensions
		areaDimensions.pos = this.cardsLayout.GetCenterCardPositions()[centerIndex]
		areaDimensions.size = {
			X: GameBoardLayout.PixelsToPercent(gameBoardLayout.cardWidth, this.layout.gameBoardDimensions.X),
			Y: GameBoardLayout.PixelsToPercent(gameBoardLayout.cardHeight, this.layout.gameBoardDimensions.Y),
		}

		return this.AreaDimensionsToPixels(areaDimensions)
	}

	 AreaDimensionsToPixels = (areaDimensions: AreaDimensions): AreaDimensions => {
		let pixelAreaDimensions = { ...areaDimensions }
		pixelAreaDimensions.pos = GameBoardLayout.GetCardPosPixels(areaDimensions.pos, this.layout.gameBoardDimensions)
		pixelAreaDimensions.size = GameBoardLayout.GetPosPixels(areaDimensions.size, this.layout.gameBoardDimensions)
		return pixelAreaDimensions
	}
}

export default GameBoardLayoutArea
