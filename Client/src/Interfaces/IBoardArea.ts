import {IPos} from "./ICard";

export type AreaLocation = { type: 'Hand', ourPlayer: boolean } | { type: 'Kitty', ourPlayer: boolean } | { type: 'Center'; index: number }

export interface IRenderableArea {
	key: string
	dimensions: AreaDimensions
	text?: string | undefined
	highlight?: boolean
	highlightZIndex: number
	location: AreaLocation
	ref: React.RefObject<HTMLDivElement>
	forceUpdate: () => void
}

export interface AreaDimensions {
	pos: IPos
	size: IPos
}
