import { CardLocationType, ICard, IMovedCardPos, IPos, IRenderableCard } from '../Interfaces/ICard'
import { IGameState } from '../Interfaces/IGameState'
import React from 'react'
import { clamp, GetOffsetInfo } from './Utilities'
import gameBoardLayout from './GameBoardLayout'
import GameBoardLayoutCards from './GameBoardLayoutCards'
import GameBoardLayout from './GameBoardLayout'
import { AreaDimensions, AreaLocation, IRenderableArea } from '../Interfaces/IBoardArea'

class GameBoardLayoutArea {
	public playerAreaTypes = ['Hand', 'Kitty']
	public centerAreaCount = 2

	layout: GameBoardLayout
	private cardsLayout: GameBoardLayoutCards
	private previousRenderableAreas: IRenderableArea[]

	constructor(
		gameBoardLayout: GameBoardLayout,
		gameBoardLayoutCards: GameBoardLayoutCards,
		previousRenderableAreas: IRenderableArea[]
	) {
		this.layout = gameBoardLayout
		this.cardsLayout = gameBoardLayoutCards
		this.previousRenderableAreas = previousRenderableAreas
	}

	public GetBoardAreas = (): IRenderableArea[] => {
		let renderableAreas = [] as IRenderableArea[]
		this.playerAreaTypes.forEach((type, i) => {
			let ourPlayerArea = { type: type, ourPlayer: true } as AreaLocation
			renderableAreas.push(this.GetRenderableArea(ourPlayerArea, this.GetAreaDimensions(ourPlayerArea)))
			let otherPlayerArea = { type: type, ourPlayer: false } as AreaLocation
			renderableAreas.push(this.GetRenderableArea(otherPlayerArea, this.GetAreaDimensions(otherPlayerArea)))
		})
		for (let i = 0; i < this.centerAreaCount; i++) {
			let loc = { type: 'Center', index: i } as AreaLocation
			renderableAreas.push(this.GetRenderableArea(loc, this.GetAreaDimensions(loc)))
		}
		return renderableAreas
	}

	public static GetPlayableAreas = (
		renderableAreas: IRenderableArea[],
		draggingCard: IRenderableCard | undefined
	): IRenderableArea[] => {
		let draggingCardRect = draggingCard?.ref.current?.getBoundingClientRect()
		if (!draggingCardRect) {
			return []
		}

		let playableAreas = [] as IRenderableArea[]
		let closestCenterArea = { area: {} as IRenderableArea, distance: Infinity }
		for (let i = 0; i < renderableAreas.length; i++) {
			let renderableArea = renderableAreas[i]
			let ourRect = renderableArea.ref?.current?.getBoundingClientRect()
			let offsetInfo = GetOffsetInfo(ourRect, draggingCardRect)
			if (offsetInfo.overlaps) {
				if (renderableArea.location.type === 'Center' && this.CanDropOntoCenter(renderableArea, draggingCard)) {
					//Get the closest center pile
					if (closestCenterArea.distance > offsetInfo.distance)
						closestCenterArea = { area: renderableArea, distance: offsetInfo.distance }
				}
				if (this.CanDropOntoHand(renderableArea, draggingCard)) {
					playableAreas.push(renderableArea)
				}
			}
		}
		if (closestCenterArea.distance !== Infinity) {
			playableAreas.push(closestCenterArea.area)
		}
		return playableAreas
	}

	static CanDropOntoCenter = (ra: IRenderableArea, c: IRenderableCard | undefined) => {
		return ra.location.type === 'Center' && c?.location === CardLocationType.Hand
	}

	static CanDropOntoHand = (ra: IRenderableArea, c: IRenderableCard | undefined) => {
		return ra.location.type === 'Hand' && c?.location === CardLocationType.Kitty
	}

	public GetAreaDimensions = (areaLocation: AreaLocation): AreaDimensions => {
		switch (areaLocation.type) {
			case 'Hand':
				return this.GetHandArea(areaLocation.ourPlayer)
			case 'Kitty':
				return this.GetKittyArea(areaLocation.ourPlayer)
			case 'Center':
				return this.GetCenterArea(areaLocation.index)
			default:
				return this.GetHandArea(areaLocation)
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

	GetCenterArea(centerIndex: number): AreaDimensions {
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

	GetRenderableArea = (areaLocation: AreaLocation, areaDimensions: AreaDimensions): IRenderableArea => {
		let previousArea = this.previousRenderableAreas.find((area) => area.key === JSON.stringify(areaLocation))
		return {
			key: JSON.stringify(areaLocation),
			location: areaLocation,
			ref: previousArea?.ref ?? React.createRef<HTMLDivElement>(),
			dimensions: areaDimensions,
			highlight: previousArea?.highlight ?? false,
			highlightZIndex: areaLocation.type === 'Center' ? 40 : 1,
			forceUpdate: previousArea?.forceUpdate ?? undefined,
		} as IRenderableArea
	}
}

export default GameBoardLayoutArea
