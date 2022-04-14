import { CardLocationType, ICard, IMovedCardPos, IPos, IRenderableCard } from '../Interfaces/ICard'
import { IGameState } from '../Interfaces/IGameState'
import React from 'react'
import { clamp } from './Utilities'
import GameBoardLayout from './GameBoardLayout'
import gameBoardLayout from './GameBoardLayout'

class GameBoardLayoutCards {
	private gameBoardLayout: GameBoardLayout
	private gameState: IGameState = {} as IGameState
	private renderableCards: IRenderableCard[] = []
	private movedCard: IMovedCardPos | undefined

	constructor(gameBoardLayout: GameBoardLayout) {
		this.gameBoardLayout = gameBoardLayout
	}

	public GetRenderableCards = (
		ourId: string | null | undefined,
		gameState: IGameState,
		renderableCards: IRenderableCard[],
		movedCard: IMovedCardPos | undefined
	): IRenderableCard[] => {
		this.gameState = gameState
		this.renderableCards = renderableCards
		this.movedCard = movedCard

		let newRenderableCards = [] as IRenderableCard[]

		// Add players cards
		this.gameState.Players.map((p, i) => {
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
		let centerPiles = this.gameState.CenterPiles.reduce<IRenderableCard[]>((result, cp, cpIndex) => {
			cp.Cards.map((c) => result.push(this.GetRenderableCard(c, cpIndex, false, CardLocationType.Center)))
			return result
		}, [] as IRenderableCard[])
		newRenderableCards.push(...centerPiles)
		return newRenderableCards
	}

	public GetCardDefaultPosition(ourPlayer: boolean, location: CardLocationType, index: number): IPos {
		switch (location) {
			case CardLocationType.Hand:
				return this.GetHandCardPosition(ourPlayer, index)
			case CardLocationType.Kitty:
				return this.GetKittyCardPosition(ourPlayer)
			case CardLocationType.Center:
			default:
				return this.GetCenterCardPositions()[index]
		}
	}

	public GetHandCardPosition(ourPlayer: boolean, index: number): IPos {
		if (index > 4) {
			return this.GetKittyCardPosition(ourPlayer)
		}
		let cardPositions = Array(gameBoardLayout.maxHandCardCount)
			.fill(0)
			.map((e, i) => {
				return {
					X: gameBoardLayout.playerHandCardsCenterX + gameBoardLayout.playerCardSeperation * (i - 2),
					Y: 1 - gameBoardLayout.playerHeightPadding,
				} as IPos
			})

		if (!ourPlayer) {
			cardPositions = GameBoardLayout.FlipPositions(cardPositions)
		}
		return cardPositions[index]
	}

	public GetKittyCardPosition(ourPlayer: boolean): IPos {
		let cardPosition = { X: GameBoardLayout.playerKittyCenterX, Y: 1 - GameBoardLayout.playerHeightPadding } as IPos

		if (!ourPlayer) {
			cardPosition = GameBoardLayout.FlipPosition(cardPosition)
		}
		return cardPosition
	}

	public GetCenterCardPositions(): IPos[] {
		return Array(2)
			.fill(0)
			.map((e, i) => {
				return { X: 0.5 + GameBoardLayout.centerPilesPadding * (i === 0 ? -1 : 1), Y: 0.5 } as IPos
			})
	}

	GetRenderableCard = (card: ICard, index: number, ourPlayer: boolean, location: CardLocationType) => {
		// let movedCard = ourPlayer ? null : this.movedCard?.CardId === card.Id ? this.movedCard : null
		let previousCard = this.renderableCards.find((c) => c.Id === card.Id)

		let defaultPos = this.GetCardDefaultPosition(ourPlayer, location, index)
		let pos = !!previousCard?.isCustomPos ? previousCard?.pos : this.gameBoardLayout.getCardPosPixels(defaultPos)
		let zIndex =
			(!ourPlayer ? Math.abs(index - GameBoardLayout.maxHandCardCount - 1) : index) +
			(location != CardLocationType.Center ? GameBoardLayout.maxHandCardCount : 0) + (ourPlayer ? 15 : 0)
		let ref = this.renderableCards.find((c) => c.Id === card.Id)?.ref


		// Animate in any new center cards from the left or right
		let animateInHorizontalOffset = previousCard?.animateInHorizontalOffset ?? 0
		let animateInDelay = previousCard?.animateInDelay ?? 0
		let animateInZIndex = previousCard?.animateInZIndex ?? zIndex
		let startTransparent = false
		if (location === CardLocationType.Center) {
			animateInHorizontalOffset =
				GameBoardLayout.GetRelativeAsPixels(0.6, this.gameBoardLayout.gameBoardDimensions.X) *
				(index === 0 ? -1 : 1)
			animateInDelay =
				this.renderableCards.filter((c) => c.location === CardLocationType.Center).length <= 2 ? 3 : 0
			startTransparent = true
		}
		// Setup the original cards with the correct transition in settings
		if (this.renderableCards.length <= 0) {
			if (location === CardLocationType.Hand) {
				animateInHorizontalOffset = GameBoardLayout.GetRelativeAsPixels(
					this.GetKittyCardPosition(ourPlayer).X - defaultPos.X,
					this.gameBoardLayout.gameBoardDimensions.X
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
				isCustomPos: previousCard?.isCustomPos ?? false,
				zIndex: zIndex,
				ref: ref ?? React.createRef<HTMLDivElement>(),
				animateInHorizontalOffset: animateInHorizontalOffset,
				animateInDelay: animateInDelay,
				animateInZIndex: animateInZIndex,
				startTransparent: startTransparent,
				horizontalOffset: 0,
				forceUpdate: previousCard?.forceUpdate ?? undefined,
			},
		} as IRenderableCard
	}
}

export default GameBoardLayoutCards
