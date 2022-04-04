import { CardLocationType, ICard, IMovedCardPos, IPos, IRenderableCard } from '../Interfaces/ICard'
import { IGameState } from '../Interfaces/IGameState'
import React from 'react'

class GameBoardLayout {
	public static maxWidth = 1000
	public static dropDistance = 40
	private static cardWidth = 80
	private static cardHeight = GameBoardLayout.cardWidth * 1.35

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

	static FlipPositions(positions: IPos[]): IPos[] {
		// return positions
		return positions.map((c) => {
			return { x: Math.abs(c.x - 1) , y: Math.abs(c.y - 1) } as IPos
		})
	}

	static getPosPixels = (pos: IPos, gameBoardDimensions: IPos): IPos => {
		return {
			x: pos!! ? pos.x * gameBoardDimensions.x - (this.cardWidth / 2): 0,
			y: pos!! ? pos.y * gameBoardDimensions.y - this.cardHeight : 0,
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

	static GetHandCardPositions(ourPlayer: boolean): IPos[] {
		let cardPositions = Array(5)
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
			cardPosition = this.FlipPositions([cardPosition])[0]
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
		return GameBoardLayout.getPosPixels(pos, this.gameBoardDimensions)
	}

	GetRenderableCard = (card: ICard, index: number, ourPlayer: boolean, location: CardLocationType) => {
		let movedCard = ourPlayer ? null : this.movedCards.find((c) => c.cardId === card.Id)
		let defaultPos = GameBoardLayout.GetCardDefaultPosition(ourPlayer, location, index)
		let pos = this.getPosPixels(movedCard?.pos ?? defaultPos)
		let zIndex = (!ourPlayer ? Math.abs(index - 4) : index) + (location != CardLocationType.Center ? 5 : 0)
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
