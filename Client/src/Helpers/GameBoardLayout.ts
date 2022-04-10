import { CardLocationType, ICard, IMovedCardPos, IPos, IRenderableCard } from '../Interfaces/ICard'
import { IGameState } from '../Interfaces/IGameState'
import React from 'react'
import { AreaDimensions } from '../Components/GameBoardAreas/BaseArea'

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
			if (p.TopKittyCardId != undefined && p.TopKittyCardId != -1) {
				let kittyCard = this.GetRenderableCard(
					{ Id: p.TopKittyCardId } as ICard,
					-1,
					ourPlayer,
					CardLocationType.Kitty
				)
				newRenderableCards.push(kittyCard)
			}
		})

		// Add the center pile cards
		let centerPiles = gameState.CenterPiles.reduce<IRenderableCard[]>((result, cp, cpIndex) => {
			cp.Cards.map((c)=>result.push(this.GetRenderableCard(c, cpIndex, false, CardLocationType.Center)))
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
				) + this.pixelsToPercent(this.cardWidth, gameBoardDimensions.X),
			Y: this.pixelsToPercent(this.cardHeight, gameBoardDimensions.Y),
		}

		return this.AreaDimensionsToPixels(areaDimensions, gameBoardDimensions)
	}

	static GetKittyArea(ourPlayer: boolean, gameBoardDimensions: IPos): AreaDimensions {
		let areaDimensions = {} as AreaDimensions
		areaDimensions.pos = this.GetKittyCardPosition(ourPlayer)
		areaDimensions.size = {
			X: this.pixelsToPercent(this.cardWidth, gameBoardDimensions.X),
			Y: this.pixelsToPercent(this.cardHeight, gameBoardDimensions.Y),
		}

		return this.AreaDimensionsToPixels(areaDimensions, gameBoardDimensions)
	}

	static GetCenterArea(ourPlayer: boolean, gameBoardDimensions: IPos, centerIndex: number): AreaDimensions {
		let areaDimensions = {} as AreaDimensions
		areaDimensions.pos = this.GetCenterCardPositions()[centerIndex]
		areaDimensions.size = {
			X: this.pixelsToPercent(this.cardWidth, gameBoardDimensions.X),
			Y: this.pixelsToPercent(this.cardHeight, gameBoardDimensions.Y),
		}

		return this.AreaDimensionsToPixels(areaDimensions, gameBoardDimensions)
	}

	static AreaDimensionsToPixels = (areaDimensions: AreaDimensions, gameBoardDimensions: IPos): AreaDimensions => {
		let pixelAreaDimensions = { ...areaDimensions }
		pixelAreaDimensions.pos = this.GetCardPosPixels(areaDimensions.pos, gameBoardDimensions)
		pixelAreaDimensions.size = this.GetPosPixels(areaDimensions.size, gameBoardDimensions)
		return pixelAreaDimensions
	}

	static pixelsToPercent = (pixels: number, gameBoardLength: number) => {
		return 1 / (gameBoardLength / pixels)
	}

	static GetHandCardPosition(ourPlayer: boolean, index: number): IPos {
		if(index > 4){
			return this.GetKittyCardPosition(ourPlayer);
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

	getPosPixels = (pos: IPos): IPos => {
		return GameBoardLayout.GetCardPosPixels(pos, this.gameBoardDimensions)
	}

	GetRenderableCard = (card: ICard, index: number, ourPlayer: boolean, location: CardLocationType) => {
		let movedCard = ourPlayer ? null : this.movedCards.find((c) => c.cardId === card.Id)
		let defaultPos = GameBoardLayout.GetCardDefaultPosition(ourPlayer, location, index)
		let pos = this.getPosPixels(movedCard?.pos ?? defaultPos)
		let zIndex =
			(!ourPlayer ? Math.abs(index - GameBoardLayout.maxHandCardCount - 1) : index) +
			(location != CardLocationType.Center ? GameBoardLayout.maxHandCardCount : 0)
		let ref = this.renderableCards.find((c) => c.Id === card.Id)?.ref
		// Animate in any new center cards from the left or right
		let animateInHorizontalOffset =
			location === CardLocationType.Center
				? GameBoardLayout.GetRelativeAsPixels(0.6, this.gameBoardDimensions.X) * (index === 0 ? -1 : 1)
				: 0
		return {
			...{
				...card,
				ourCard: ourPlayer,
				location: location,
				pos: pos,
				zIndex: zIndex,
				ref: ref ?? React.createRef<HTMLDivElement>(),
				animateInHorizontalOffset: animateInHorizontalOffset,
			},
		} as IRenderableCard
	}
}

export default GameBoardLayout
