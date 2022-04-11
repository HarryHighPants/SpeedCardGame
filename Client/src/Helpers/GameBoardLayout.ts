import { CardLocationType, ICard, IMovedCardPos, IPos, IRenderableCard } from '../Interfaces/ICard'
import { IGameState } from '../Interfaces/IGameState'
import React from 'react'
import { AreaDimensions } from '../Components/GameBoardAreas/BaseArea'
import { clamp } from './Utilities'

class GameBoardLayout {
	public static maxWidth = 750
	public static dropDistance = 50
	public static cardWidth = 80
	public static cardHeight = GameBoardLayout.cardWidth * 1.45
	public static maxHandCardCount = 5

	private static playerHeightPadding = 0.25
	private static playerCardSeperation = 0.075
	private static playerHandCardsCenterX = 0.35

	private static playerKittyCenterX = 0.8

	private static centerPilesPadding = 0.1

	private gameBoardDimensions: IPos
	private movedCard: IMovedCardPos | undefined
	private renderableCards: IRenderableCard[]

	constructor(gameBoardDimensions: IPos, movedCard: IMovedCardPos | undefined, renderableCards: IRenderableCard[]) {
		this.gameBoardDimensions = gameBoardDimensions
		this.movedCard = movedCard
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
			if (p.TopKittyCardId != undefined && p.TopKittyCardId != -1) {
				let kittyCard = this.GetRenderableCard(
					{ Id: p.TopKittyCardId } as ICard,
					-1,
					ourPlayer,
					CardLocationType.Kitty
				)
				newRenderableCards.push(kittyCard)
			}
			if (p.KittyCardsCount > 1 && p.TopKittyCardId) {
				let kittyCard = this.GetRenderableCard(
					{ Id: p.TopKittyCardId + 100 } as ICard,
					-5,
					ourPlayer,
					CardLocationType.Kitty
				)
				newRenderableCards.push(kittyCard)
			}
		})

		// Add the center pile cards
		let centerPiles = gameState.CenterPiles.reduce<IRenderableCard[]>((result, cp, cpIndex) => {
			cp.Cards.map((c) => result.push(this.GetRenderableCard(c, cpIndex, false, CardLocationType.Center)))
			return result
		}, [] as IRenderableCard[])
		newRenderableCards.push(...centerPiles)
		return newRenderableCards
	}

	static FlipPosition(pos: IPos): IPos {
		return { X: Math.abs(pos.X - 1), Y: Math.abs(pos.Y - 1) } as IPos
	}

	static FlipPositions(positions: IPos[]): IPos[] {
		return positions.map((pos) => {
			return GameBoardLayout.FlipPosition(pos)
		})
	}

	static GetRelativeAsPixels = (x: number | undefined, gameBoardLength: number): number => {
		return x!! ? x * gameBoardLength : 0
	}

	static GetPosPixels = (pos: IPos, gameBoardDimensions: IPos): IPos => {
		return {
			X: this.GetRelativeAsPixels(pos.X, gameBoardDimensions.X),
			Y: this.GetRelativeAsPixels(pos.Y, gameBoardDimensions.Y),
		}
	}

	static GetCardPosPixels = (pos: IPos, gameBoardDimensions: IPos): IPos => {
		let posPixels = this.GetPosPixels(pos, gameBoardDimensions)
		return {
			X: posPixels.X - this.cardWidth / 2,
			Y: posPixels.Y - this.cardHeight,
		}
	}

	static GetCardDefaultPosition(ourPlayer: boolean, location: CardLocationType, index: number): IPos {
		switch (location) {
			case CardLocationType.Hand:
				return GameBoardLayout.GetHandCardPosition(ourPlayer, index)
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
			X: this.GetHandCardPosition(ourPlayer, ourPlayer ? 0 : this.maxHandCardCount - 1).X,
			Y: this.GetHandCardPosition(ourPlayer, 0).Y,
		} as IPos

		areaDimensions.size = {
			X:
				Math.abs(
					this.GetHandCardPosition(ourPlayer, 0).X -
						this.GetHandCardPosition(ourPlayer, this.maxHandCardCount - 1).X
				) + this.PixelsToPercent(this.cardWidth, gameBoardDimensions.X),
			Y: this.PixelsToPercent(this.cardHeight, gameBoardDimensions.Y),
		}

		return this.AreaDimensionsToPixels(areaDimensions, gameBoardDimensions)
	}

	static GetKittyArea(ourPlayer: boolean, gameBoardDimensions: IPos): AreaDimensions {
		let areaDimensions = {} as AreaDimensions
		areaDimensions.pos = this.GetKittyCardPosition(ourPlayer)
		areaDimensions.size = {
			X: this.PixelsToPercent(this.cardWidth, gameBoardDimensions.X),
			Y: this.PixelsToPercent(this.cardHeight, gameBoardDimensions.Y),
		}

		return this.AreaDimensionsToPixels(areaDimensions, gameBoardDimensions)
	}

	static GetCenterArea(ourPlayer: boolean, gameBoardDimensions: IPos, centerIndex: number): AreaDimensions {
		let areaDimensions = {} as AreaDimensions
		areaDimensions.pos = this.GetCenterCardPositions()[centerIndex]
		areaDimensions.size = {
			X: this.PixelsToPercent(this.cardWidth, gameBoardDimensions.X),
			Y: this.PixelsToPercent(this.cardHeight, gameBoardDimensions.Y),
		}

		return this.AreaDimensionsToPixels(areaDimensions, gameBoardDimensions)
	}

	static AreaDimensionsToPixels = (areaDimensions: AreaDimensions, gameBoardDimensions: IPos): AreaDimensions => {
		let pixelAreaDimensions = { ...areaDimensions }
		pixelAreaDimensions.pos = this.GetCardPosPixels(areaDimensions.pos, gameBoardDimensions)
		pixelAreaDimensions.size = this.GetPosPixels(areaDimensions.size, gameBoardDimensions)
		return pixelAreaDimensions
	}

	public static PosPixelsToPercent = (posPixels: IPos, gameBoardDimensions: IPos) => {
		return {
			X: this.PixelsToPercent(posPixels.X, gameBoardDimensions.X),
			Y: this.PixelsToPercent(posPixels.Y, gameBoardDimensions.Y),
		} as IPos
	}

	public static CardRectToPercent = (rect: DOMRect, gameBoardDimensions: IPos) => {
		// Need to subtract half of the size of the screen over the actual size of the screen from x
		let excessWidth = clamp(window.innerWidth - gameBoardDimensions.X, 0, Infinity)
		return {
			X: this.PixelsToPercent(rect.x - excessWidth / 2 + this.cardWidth / 2, gameBoardDimensions.X),
			Y: this.PixelsToPercent(rect.y + this.cardHeight / 2, gameBoardDimensions.Y),
		} as IPos
	}

	public static PixelsToPercent = (pixels: number, gameBoardLength: number) => {
		return 1 / (gameBoardLength / pixels)
	}

	static GetHandCardPosition(ourPlayer: boolean, index: number): IPos {
		if (index > 4) {
			return this.GetKittyCardPosition(ourPlayer)
		}
		let cardPositions = Array(this.maxHandCardCount)
			.fill(0)
			.map((e, i) => {
				return {
					X: this.playerHandCardsCenterX + this.playerCardSeperation * (i - 2),
					Y: 1 - this.playerHeightPadding,
				} as IPos
			})

		if (!ourPlayer) {
			cardPositions = this.FlipPositions(cardPositions)
		}
		return cardPositions[index]
	}

	static GetKittyCardPosition(ourPlayer: boolean): IPos {
		let cardPosition = { X: this.playerKittyCenterX, Y: 1 - this.playerHeightPadding } as IPos

		if (!ourPlayer) {
			cardPosition = this.FlipPosition(cardPosition)
		}
		return cardPosition
	}

	static GetCenterCardPositions(): IPos[] {
		return Array(2)
			.fill(0)
			.map((e, i) => {
				return { X: 0.5 + this.centerPilesPadding * (i === 0 ? -1 : 1), Y: 0.5 } as IPos
			})
	}

	getCardPosPixels = (pos: IPos): IPos => {
		return GameBoardLayout.GetCardPosPixels(pos, this.gameBoardDimensions)
	}

	getPosPixels = (pos: IPos): IPos => {
		return GameBoardLayout.GetPosPixels(pos, this.gameBoardDimensions)
	}

	GetRenderableCard = (card: ICard, index: number, ourPlayer: boolean, location: CardLocationType) => {
		let movedCard = ourPlayer ? null : this.movedCard?.CardId === card.Id ? this.movedCard : null
		let defaultPos = GameBoardLayout.GetCardDefaultPosition(ourPlayer, location, index)
		let pos = this.getCardPosPixels(
			movedCard?.Pos !== undefined ? GameBoardLayout.FlipPosition(movedCard.Pos) : defaultPos
		)
		let zIndex =
			(!ourPlayer ? Math.abs(index - GameBoardLayout.maxHandCardCount - 1) : index) +
			(location != CardLocationType.Center ? GameBoardLayout.maxHandCardCount : 0)
		let ref = this.renderableCards.find((c) => c.Id === card.Id)?.ref

		let previousCard = this.renderableCards.find((c) => c.Id === card.Id)

		// Animate in any new center cards from the left or right
		let animateInHorizontalOffset = previousCard?.animateInHorizontalOffset ?? 0
		let animateInDelay = previousCard?.animateInDelay ?? 0
		let animateInZIndex = previousCard?.animateInZIndex ?? zIndex
		// Setup the original cards with the correct transition in settings
		if (this.renderableCards.length <= 0) {
			if (location === CardLocationType.Center) {
				animateInHorizontalOffset =
					GameBoardLayout.GetRelativeAsPixels(0.6, this.gameBoardDimensions.X) * (index === 0 ? -1 : 1)
				animateInDelay = 3
			} else if (location === CardLocationType.Hand) {
				animateInHorizontalOffset = GameBoardLayout.GetRelativeAsPixels(
					GameBoardLayout.GetKittyCardPosition(ourPlayer).X - defaultPos.X,
					this.gameBoardDimensions.X
				)
				animateInDelay = index * 0.5
				animateInZIndex = Math.abs(index - GameBoardLayout.maxHandCardCount - 1) + 20
			}
		}
		return {
			...{
				...card,
				ourCard: ourPlayer,
				location: location,
				pos: pos,
				zIndex: zIndex,
				ref: ref ?? React.createRef<HTMLDivElement>(),
				animateInHorizontalOffset: animateInHorizontalOffset,
				animateInDelay: animateInDelay,
				animateInZIndex: animateInZIndex,
			},
		} as IRenderableCard
	}
}

export default GameBoardLayout
