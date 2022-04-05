import { CardLocationType, ICard, IMovedCardPos, IPos, IRenderableCard } from '../Interfaces/ICard'
import { IGameState } from '../Interfaces/IGameState'
import React from 'react'
import { AreaDimensions } from '../Components/GameBoardAreas/BaseArea'

class GameBoardLayout {
	public static maxWidth = 750
	public static dropDistance = 40
	public static cardWidth = 80
	public static cardHeight = GameBoardLayout.cardWidth * 1.45
	public static maxHandCardCount = 5

	private static playerHeightPadding = 0.25
	private static playerCardSeperation = 0.075
	private static playerHandCardsCenterX = 0.35

	private static playerKittyCenterX = 0.8

	private static centerPilesPadding = 0.1

	private gameBoardDimensions: IPos
	private movedCards: IMovedCardPos[]
	private renderableCards: IRenderableCard[]

	constructor(gameBoardDimensions: IPos, movedCards: IMovedCardPos[], renderableCards: IRenderableCard[]) {
		this.gameBoardDimensions = gameBoardDimensions
		this.movedCards = movedCards
		this.renderableCards = renderableCards
	}

	public static GetAreaDimensions = (
		ourPlayer: boolean,
		location: CardLocationType,
		gameBoardDimensions: IPos,
		centerIndex: number = 0
	): AreaDimensions => {
		switch (location) {
			case CardLocationType.Hand:
				return GameBoardLayout.GetHandArea(ourPlayer, gameBoardDimensions)
			case CardLocationType.Kitty:
				return GameBoardLayout.GetKittyArea(ourPlayer, gameBoardDimensions)
			case CardLocationType.Center:
				return GameBoardLayout.GetCenterArea(ourPlayer, gameBoardDimensions, centerIndex)
			default:
				return GameBoardLayout.GetHandArea(ourPlayer, gameBoardDimensions)
		}
	}

	public GetRenderableCards = (ourId: string | null | undefined, gameState: IGameState): IRenderableCard[] => {
		let newRenderableCards = [] as IRenderableCard[]

		// Add players cards
		gameState.Players.map((p, i) => {
			let ourPlayer = p.Id == ourId

			// Add the players hand cards
			let handCards = p.HandCards.map((hc, cIndex) => {
				return this.GetRenderableCard(hc, cIndex, ourPlayer, CardLocationType.Hand)
			})
			newRenderableCards.push(...handCards)

			// Add the players Kitty card
			let kittyCard = this.GetRenderableCard(
				{ Id: p.TopKittyCardId } as ICard,
				-1,
				ourPlayer,
				CardLocationType.Kitty
			)
			newRenderableCards.push(kittyCard)
		})

		// Add the center piles
		let centerPiles = gameState.CenterPiles.map((cp, cpIndex) => {
			return this.GetRenderableCard(cp, cpIndex, false, CardLocationType.Center)
		})
		newRenderableCards.push(...centerPiles)
		return newRenderableCards
	}

	static FlipPosition(pos: IPos): IPos {
		return { x: Math.abs(pos.x - 1), y: Math.abs(pos.y - 1) } as IPos
	}

	static FlipPositions(positions: IPos[]): IPos[] {
		return positions.map((pos) => {
			return GameBoardLayout.FlipPosition(pos)
		})
	}

	static getPosPixels = (pos: IPos, gameBoardDimensions: IPos): IPos => {
		return {
			x: pos!! ? pos.x * gameBoardDimensions.x : 0,
			y: pos!! ? pos.y * gameBoardDimensions.y : 0,
		}
	}

	static getCardPosPixels = (pos: IPos, gameBoardDimensions: IPos): IPos => {
		let posPixels = this.getPosPixels(pos, gameBoardDimensions)
		return {
			x: posPixels.x - this.cardWidth / 2,
			y: posPixels.y - this.cardHeight,
		}
	}

	static GetCardDefaultPosition(ourPlayer: boolean, location: CardLocationType, index: number): IPos {
		switch (location) {
			case CardLocationType.Hand:
				return GameBoardLayout.GetHandCardPositions(ourPlayer)[index]
			case CardLocationType.Kitty:
				return GameBoardLayout.GetKittyCardPosition(ourPlayer)
			case CardLocationType.Center:
			default:
				return GameBoardLayout.GetCenterCardPositions()[index]
		}
	}

	static GetHandArea(ourPlayer: boolean, gameBoardDimensions: IPos): AreaDimensions {
		let areaDimensions = {} as AreaDimensions
		areaDimensions.pos = {
			x: this.GetHandCardPositions(ourPlayer)[ourPlayer ? 0 : this.maxHandCardCount - 1].x,
			y: this.GetHandCardPositions(ourPlayer)[0].y,
		} as IPos

		areaDimensions.size = {
			x:
				Math.abs(
					this.GetHandCardPositions(ourPlayer)[0].x -
						this.GetHandCardPositions(ourPlayer)[this.maxHandCardCount - 1].x
				) + this.pixelsToPercent(this.cardWidth, gameBoardDimensions.x),
			y: this.pixelsToPercent(this.cardHeight, gameBoardDimensions.y),
		}

		return this.AreaDimensionsToPixels(areaDimensions, gameBoardDimensions)
	}

	static GetKittyArea(ourPlayer: boolean, gameBoardDimensions: IPos): AreaDimensions {
		let areaDimensions = {} as AreaDimensions
		areaDimensions.pos = this.GetKittyCardPosition(ourPlayer)
		areaDimensions.size = {
			x: this.pixelsToPercent(this.cardWidth, gameBoardDimensions.x),
			y: this.pixelsToPercent(this.cardHeight, gameBoardDimensions.y),
		}

		return this.AreaDimensionsToPixels(areaDimensions, gameBoardDimensions)
	}

	static GetCenterArea(ourPlayer: boolean, gameBoardDimensions: IPos, centerIndex: number): AreaDimensions {
		let areaDimensions = {} as AreaDimensions
		areaDimensions.pos = this.GetCenterCardPositions()[centerIndex]
		areaDimensions.size = {
			x: this.pixelsToPercent(this.cardWidth, gameBoardDimensions.x),
			y: this.pixelsToPercent(this.cardHeight, gameBoardDimensions.y),
		}

		return this.AreaDimensionsToPixels(areaDimensions, gameBoardDimensions)
	}

	static AreaDimensionsToPixels = (areaDimensions: AreaDimensions, gameBoardDimensions: IPos): AreaDimensions => {
		let pixelAreaDimensions = { ...areaDimensions }
		pixelAreaDimensions.pos = this.getCardPosPixels(areaDimensions.pos, gameBoardDimensions)
		pixelAreaDimensions.size = this.getPosPixels(areaDimensions.size, gameBoardDimensions)
		return pixelAreaDimensions
	}

	static pixelsToPercent = (pixels: number, gameBoardLength: number) => {
		return 1 / (gameBoardLength / pixels)
	}

	static GetHandCardPositions(ourPlayer: boolean): IPos[] {
		let cardPositions = Array(this.maxHandCardCount)
			.fill(0)
			.map((e, i) => {
				return {
					x: this.playerHandCardsCenterX + this.playerCardSeperation * (i - 2),
					y: 1 - this.playerHeightPadding,
				} as IPos
			})

		if (!ourPlayer) {
			cardPositions = this.FlipPositions(cardPositions)
		}
		return cardPositions
	}

	static GetKittyCardPosition(ourPlayer: boolean): IPos {
		let cardPosition = { x: this.playerKittyCenterX, y: 1 - this.playerHeightPadding } as IPos

		if (!ourPlayer) {
			cardPosition = this.FlipPosition(cardPosition)
		}
		return cardPosition
	}

	static GetCenterCardPositions(): IPos[] {
		return Array(2)
			.fill(0)
			.map((e, i) => {
				return { x: 0.5 + this.centerPilesPadding * (i === 0 ? -1 : 1), y: 0.5 } as IPos
			})
	}

	getPosPixels = (pos: IPos): IPos => {
		return GameBoardLayout.getCardPosPixels(pos, this.gameBoardDimensions)
	}

	GetRenderableCard = (card: ICard, index: number, ourPlayer: boolean, location: CardLocationType) => {
		let movedCard = ourPlayer ? null : this.movedCards.find((c) => c.cardId === card.Id)
		let defaultPos = GameBoardLayout.GetCardDefaultPosition(ourPlayer, location, index)
		let pos = this.getPosPixels(movedCard?.pos ?? defaultPos)
		let zIndex =
			(!ourPlayer ? Math.abs(index - GameBoardLayout.maxHandCardCount - 1) : index) +
			(location != CardLocationType.Center ? GameBoardLayout.maxHandCardCount : 0)
		let ref = this.renderableCards.find((c) => c.Id === card.Id)?.ref
		return {
			...{
				...card,
				ourCard: ourPlayer,
				location: location,
				pos: pos,
				zIndex: zIndex,
				ref: ref ?? React.createRef<HTMLDivElement>(),
			},
		} as IRenderableCard
	}
}

export default GameBoardLayout
