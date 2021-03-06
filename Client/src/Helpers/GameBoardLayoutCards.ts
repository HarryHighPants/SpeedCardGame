import { CardLocationType, ICard, IMovedCardPos, IPos, IRenderableCard } from '../Interfaces/ICard'
import { IGameState } from '../Interfaces/IGameState'
import React from 'react'
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
        this.gameState.players.forEach((p, i) => {
            let ourPlayer = p.idHash == ourId

            // Add the players hand cards
            let handCards = p.handCards.map((hc, cIndex) => {
                return this.GetRenderableCard(hc, cIndex, ourPlayer, CardLocationType.Hand)
            })
            newRenderableCards.push(...handCards)

            // Add the players Kitty card
            if (p.topKittyCardId != undefined && p.topKittyCardId != -1) {
                let kittyCard = this.GetRenderableCard(
                    { id: p.topKittyCardId } as ICard,
                    -1,
                    ourPlayer,
                    CardLocationType.Kitty
                )
                newRenderableCards.push(kittyCard)
            }
            if (p.kittyCardsCount > 1 && p.topKittyCardId) {
                let kittyCard = this.GetRenderableCard(
                    { id: p.topKittyCardId + 100 } as ICard,
                    -5,
                    ourPlayer,
                    CardLocationType.Kitty
                )
                newRenderableCards.push(kittyCard)
            }
        })

        // Add the center pile cards
        let centerPiles = this.gameState.centerPiles.reduce<IRenderableCard[]>((result, cp, cpIndex) => {
            cp.cards.map((c) => result.push(this.GetRenderableCard(c, cpIndex, false, CardLocationType.Center)))
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
                    x: gameBoardLayout.playerHandCardsCenterX + gameBoardLayout.playerCardSeperation * (i - 2),
                    y: 1 - gameBoardLayout.playerHeightPadding,
                } as IPos
            })

        if (!ourPlayer) {
            cardPositions = GameBoardLayout.FlipPositions(cardPositions)
        }
        return cardPositions[index]
    }

    public GetKittyCardPosition(ourPlayer: boolean): IPos {
        let cardPosition = { x: GameBoardLayout.playerKittyCenterX, y: 1 - GameBoardLayout.playerHeightPadding } as IPos

        if (!ourPlayer) {
            cardPosition = GameBoardLayout.FlipPosition(cardPosition)
        }
        return cardPosition
    }

    public GetCenterCardPositions(): IPos[] {
        let centerPadding = Math.max(
            gameBoardLayout.GetPixelsToPercent(
                gameBoardLayout.minCenterPilesPaddingPixels,
                this.gameBoardLayout.gameBoardDimensions.x
            ),
            gameBoardLayout.centerPilesPadding
        )
        return Array(2)
            .fill(0)
            .map((e, i) => {
                return { x: 0.5 + centerPadding * (i === 0 ? -1 : 1), y: 0.5 } as IPos
            })
    }

    GetRenderableCard = (card: ICard, index: number, ourPlayer: boolean, location: CardLocationType) => {
        // let movedCard = ourPlayer ? null : this.movedCard?.CardId === card.Id ? this.movedCard : null
        let previousCard = this.renderableCards.find((c) => c.id === card.id)

        let defaultPos = this.GetCardDefaultPosition(ourPlayer, location, index)
        let pos = !!previousCard?.isCustomPos ? previousCard?.pos : this.gameBoardLayout.getCardPosPixels(defaultPos)
        let zIndex =
            (!ourPlayer ? Math.abs(index - GameBoardLayout.maxHandCardCount - 1) : index) +
            (location != CardLocationType.Center ? GameBoardLayout.maxHandCardCount : 0) +
            (ourPlayer ? 15 : 0)
        let ref = this.renderableCards.find((c) => c.id === card.id)?.ref

        // Animate in any new center cards from the left or right
        let animateInHorizontalOffset = previousCard?.animateInHorizontalOffset ?? 0
        let animateInDelay = previousCard?.animateInDelay ?? 0
        let animateInZIndex = previousCard?.animateInZIndex ?? zIndex
        let startTransparent = false
        if (location === CardLocationType.Center) {
            animateInHorizontalOffset =
                GameBoardLayout.GetPercentAsPixels(0.6, this.gameBoardLayout.gameBoardDimensions.x) *
                (index === 0 ? -1 : 1)
            animateInDelay =
                this.renderableCards.filter((c) => c.location === CardLocationType.Center).length <= 2 ? 3 : 0
            startTransparent = true
        }
        // Setup the original cards with the correct transition in settings
        if (this.renderableCards.length <= 0) {
            if (location === CardLocationType.Hand) {
                animateInHorizontalOffset = GameBoardLayout.GetPercentAsPixels(
                    this.GetKittyCardPosition(ourPlayer).x - defaultPos.x,
                    this.gameBoardLayout.gameBoardDimensions.x
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
                pileIndex: index,
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
