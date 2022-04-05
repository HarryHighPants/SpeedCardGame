import { useEffect, useState } from 'react'
import styled from 'styled-components'
import { CardLocationType, CardValue, ICard, IPos, IRenderableCard, Suit } from '../Interfaces/ICard'
import { motion, PanInfo, Variants } from 'framer-motion'
import GameBoardLayout from '../Helpers/GameBoardLayout'
import { usePrevious } from '../Helpers/UsePrevious'
import { GetDistance, GetDistanceRect } from '../Helpers/Distance'

export interface Props {
	id?: number
	cardBeingDragged: IRenderableCard | undefined
	ourRef: React.RefObject<HTMLDivElement>
	children: JSX.Element
	onDistanceUpdated: (distance: number, draggableIsRight: boolean) => void
}

const Droppable = ({ cardBeingDragged, ourRef, id, children, onDistanceUpdated }: Props) => {
	const [distance, setDistance] = useState(Infinity)

	// cardBeingDragged updated
	useEffect(() => {
		if (cardBeingDragged?.Id === id) return
		let draggingCardRect = cardBeingDragged?.ref.current?.getBoundingClientRect()
		let ourRect = ourRef?.current?.getBoundingClientRect()
		console.log(ourRect)
		UpdateDistance(ourRect, draggingCardRect)
	}, [cardBeingDragged?.ref.current?.getBoundingClientRect().x, cardBeingDragged])

	const UpdateDistance = (ourRect: DOMRect | undefined, draggingCardRect: DOMRect | undefined) => {
		let distance = !draggingCardRect || !ourRect ? Infinity: GetDistanceRect(draggingCardRect, ourRect)
		setDistance(distance)
		onDistanceUpdated(distance, !draggingCardRect || !ourRect ? false : draggingCardRect.x < ourRect.x)
	}

	return <>{children}</>
}

export default Droppable
