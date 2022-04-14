import { CardLocationType, ICard, IMovedCardPos, IPos, IRenderableCard } from '../Interfaces/ICard'
import { IGameState } from '../Interfaces/IGameState'
import React from 'react'
import { clamp } from './Utilities'
import GameBoardLayoutCards from './GameBoardLayoutCards'
import {IRenderableArea} from "../Interfaces/IBoardArea";
import GameBoardLayoutAreas from "./GameBoardLayoutAreas";

class GameBoardLayout {
	public static maxWidth = 750
	public static dropDistance = 100
	public static cardWidth = 80
	public static cardHeight = GameBoardLayout.cardWidth * 1.45
	public static maxHandCardCount = 5

	public static playerHeightPadding = 0.25
	public static playerCardSeperation = 0.075
	public static playerHandCardsCenterX = 0.35

	public static playerKittyCenterX = 0.8

	public static centerPilesPadding = 0.1

	public gameBoardDimensions: IPos

	constructor(gameBoardDimensions: IPos) {
		this.gameBoardDimensions = gameBoardDimensions
	}

	public GetRenderableCards = (
		ourId: string | null | undefined,
		gameState: IGameState,
		renderableCards: IRenderableCard[],
	): IRenderableCard[] => {
		let layoutCards = new GameBoardLayoutCards(this)
		return layoutCards.GetRenderableCards(ourId, gameState, renderableCards, undefined)
	}

	public GetCardDefaultPosition(ourPlayer: boolean, location: CardLocationType, index: number): IPos {
		let layoutCards = new GameBoardLayoutCards(this)
		return layoutCards.GetCardDefaultPosition(ourPlayer, location, index)
	}

	public GetBoardAreas = (renderAbleAreas: IRenderableArea[]): IRenderableArea[] => {
		let layoutAreas = new GameBoardLayoutAreas(this, new GameBoardLayoutCards(this), renderAbleAreas)
		return layoutAreas.GetBoardAreas()
	}

	public static FlipPosition(pos: IPos): IPos {
		return { X: Math.abs(pos.X - 1), Y: Math.abs(pos.Y - 1) } as IPos
	}

	public static FlipPositions(positions: IPos[]): IPos[] {
		return positions.map((pos) => {
			return GameBoardLayout.FlipPosition(pos)
		})
	}

	public static CardRectToPercent = (rect: DOMRect, gameBoardDimensions: IPos) => {
		// Need to subtract half of the size of the screen over the actual size of the screen from x
		let excessWidth = clamp(window.innerWidth - gameBoardDimensions.X, 0, Infinity)
		return {
			X: this.PixelsToPercent(rect.x - excessWidth / 2 + this.cardWidth / 2, gameBoardDimensions.X),
			Y: this.PixelsToPercent(rect.y + this.cardHeight / 2, gameBoardDimensions.Y),
		} as IPos
	}

	public static PosPixelsToPercent = (posPixels: IPos, gameBoardDimensions: IPos) => {
		return {
			X: this.PixelsToPercent(posPixels.X, gameBoardDimensions.X),
			Y: this.PixelsToPercent(posPixels.Y, gameBoardDimensions.Y),
		} as IPos
	}

	public static PixelsToPercent = (pixels: number, gameBoardLength: number) => {
		return 1 / (gameBoardLength / pixels)
	}

	public static GetCardPosPixels = (pos: IPos, gameBoardDimensions: IPos): IPos => {
		let posPixels = this.GetPosPixels(pos, gameBoardDimensions)
		return {
			X: posPixels.X - this.cardWidth / 2,
			Y: posPixels.Y - this.cardHeight,
		}
	}

	public static GetPosPixels = (pos: IPos, gameBoardDimensions: IPos): IPos => {
		return {
			X: this.GetRelativeAsPixels(pos.X, gameBoardDimensions.X),
			Y: this.GetRelativeAsPixels(pos.Y, gameBoardDimensions.Y),
		}
	}

	public static GetRelativeAsPixels = (x: number | undefined, gameBoardLength: number): number => {
		return x!! ? x * gameBoardLength : 0
	}

	public getCardPosPixels = (pos: IPos): IPos => {
		return GameBoardLayout.GetCardPosPixels(pos, this.gameBoardDimensions)
	}

	public getPosPixels = (pos: IPos): IPos => {
		return GameBoardLayout.GetPosPixels(pos, this.gameBoardDimensions)
	}
}

export default GameBoardLayout
