import styled from 'styled-components'
import { IPos, IRenderableCard } from '../../Interfaces/ICard'
import BaseArea, { BaseAreaProps } from './BaseArea'
import Droppable from '../Droppable'
import GameBoardLayout from '../../Helpers/GameBoardLayout'
import { createRef, useState } from 'react'

interface Props extends BaseAreaProps {
	setIsHighlighted: (highlighted: boolean) => void
}

const DroppableArea = ({ dimensions, text, setIsHighlighted }: Props) => {
	const ref = createRef<HTMLDivElement>()
	const [highlighted, localSetHighlighted] = useState(false)

	const UpdateHighlighted = (distance: number, overlaps?: boolean, delta?: IPos) => {
		setIsHighlighted(overlaps === true)
		localSetHighlighted(overlaps === true)
	}

	return (
			<BaseArea highlight={highlighted} ref={ref} dimensions={dimensions} text={text} />
	)
}

export default DroppableArea
