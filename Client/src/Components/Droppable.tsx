import { useEffect, useState } from 'react'
import styled from 'styled-components'
import { CardLocationType, CardValue, ICard, IPos, IRenderableCard, Suit } from '../Interfaces/ICard'
import { motion, PanInfo, Variants } from 'framer-motion'
import GameBoardLayout from '../Helpers/GameBoardLayout'
import { usePrevious } from '../Helpers/UsePrevious'
import { GetDistanceRect, Overlaps } from '../Helpers/Utilities'

export interface Props {
	id?: number
	cardBeingDragged: IRenderableCard | undefined
	ourRef: React.RefObject<HTMLDivElement>
	children: JSX.Element
	onDistanceUpdated: (distance: number, overlaps?: boolean, delta?: IPos) => void
}

const Droppable = ({ cardBeingDragged, ourRef, id, children, onDistanceUpdated }: Props) => {
	// cardBeingDragged updated
	useEffect(() => {
		if (cardBeingDragged?.Id === id) return
		let draggingCardRect = cardBeingDragged?.ref.current?.getBoundingClientRect()
		let ourRect = ourRef?.current?.getBoundingClientRect()
		UpdateDistance(ourRect, draggingCardRect)
	}, [cardBeingDragged?.ref.current?.getBoundingClientRect().x, cardBeingDragged])

	const UpdateDistance = (ourRect: DOMRect | undefined, draggingCardRect: DOMRect | undefined) => {
		let distance = GetDistanceRect(draggingCardRect, ourRect)
		let overlaps = Overlaps(ourRect, draggingCardRect)
		let delta =
			!draggingCardRect || !ourRect
				? undefined
				: { x: draggingCardRect.x - ourRect.x, y: draggingCardRect.y - ourRect.y }
		onDistanceUpdated(distance, overlaps, delta)
		// if(id === undefined){
			console.log(id, distance, overlaps, delta)
		// }
	}

	return <>{children}</>
}

export default Droppable
