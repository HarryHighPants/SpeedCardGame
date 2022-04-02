import { MouseEventHandler, useEffect, useState } from 'react'
import styled from 'styled-components'
import { ICard, CardValue, Suit, IPos, IRenderableCard } from '../Interfaces/ICard'
import { motion, PanInfo, useDragControls, useMotionValue, useTransform, Variants } from 'framer-motion'

export interface Props {
	card: IRenderableCard
	gameBoardDimensions: IPos
	onDragStart: (info: PanInfo, card: IRenderableCard) => void
	onDragEnd: (info: PanInfo, card: IRenderableCard) => void
	onMouseEnter: (card: IRenderableCard) => void
	onMouseExit: (card: IRenderableCard) => void
}

const Card = ({ card, gameBoardDimensions, onDragStart, onDragEnd, onMouseEnter, onMouseExit }: Props) => {
	const [disablePointerEvents, setDisablePointerEvents] = useState(false)
	// const [isDragging, setDragging] = useState(false)

	const getPosPixels = (): IPos => {
		return {
			x: card.pos!! ? card.pos.x * gameBoardDimensions.x : 0,
			y: card.pos!! ? card.pos.y * gameBoardDimensions.y : 0,
		}
	}


	const logCardPos = (info: PanInfo) => {
		let distance = GetDistance({ x: info.offset.x, y: info.offset.y }, { x: 0, y: 0 })
		setDisablePointerEvents(distance > 150)
	}

	const GetDistance = (pos1: IPos | undefined, pos2: IPos | undefined) => {
		if (!pos1 || !pos2) return Infinity
		let a = pos1.x - pos2.x
		let b = pos1.y - pos2.y
		return Math.sqrt(a * a + b * b)
	}

	const OnStartDrag = (panInfo: PanInfo) => {
		// setDragging(true)
		onDragStart(panInfo, card)
	}

	const OnEndDrag = (panInfo: PanInfo) => {
		// setDragging(false)
		setDisablePointerEvents(false)
		onDragEnd(panInfo, card)
	}

	const OnMouseEnter = (mouseEvent: React.MouseEvent<HTMLDivElement, MouseEvent>) => {
		if (!card.droppableTarget) return
		onMouseEnter(card)
	}

	const OnMouseExit = (mouseEvent: React.MouseEvent<HTMLDivElement, MouseEvent>) => {
		if (!card.droppableTarget) return
		onMouseExit(card)
	}

	const cardVariants: Variants = {
		initial: {
			scale: 1,
			top: getPosPixels().y,
			boxShadow: '0',
		},
		hovered: card.draggable
			? {
					scale: 1.03,
					top: getPosPixels().y-20,
					boxShadow: '0px 3px 3px rgba(0,0,0,0.15)',
					pointerEvents: 'all',
			  }
			: {},
		dragging:
			card.draggable
				? {
						scale: 1.12,
						boxShadow: '0px 5px 5px rgba(0,0,0,0.1)',
						cursor: 'grabbing',
						zIndex: 15,
						top: getPosPixels().y-20,
						pointerEvents: disablePointerEvents ? 'none' : 'all',
				  }
				: {},
	}

	return (
		<CardParent
			pos={getPosPixels()}
			card={card}
			variants={cardVariants}
			drag={card.draggable}
			onDrag={(event, info) => logCardPos(info)}
			initial="initial"
			whileHover="hovered"
			whileTap="dragging"
			onDragStart={(e, info) => OnStartDrag(info)}
			onDragEnd={(e, info) => OnEndDrag(info)}
			dragSnapToOrigin={true}
			dragElastic={1}
			dragConstraints={{ top: 0, right: 0, bottom: 0, left: 0 }}
			dragMomentum={false}
			onMouseEnter={(e) => OnMouseEnter(e)}
		>
			<img
				draggable="false"
				width={80}
				key={card.Id}
				src={CardImgSrc(card)}
				alt={CardImgName(card)}
			/>
		</CardParent>
	)
}

const CardParent = styled(motion.div)<{ card: IRenderableCard; pos: IPos }>`
	width: 80px;
	cursor: ${(p) => (p.card.draggable ? 'grab' : 'default')};
	position: absolute;
	left: ${(p) => p.pos.x}px;
	top: ${(p) => p.pos.y}px;
	${(p) => (p.card.hoveredDropTarget ? 'filter: brightness(-0.25)' : '')};
`

// const CardImg = styled.img``

const CardImgSrc = (card: ICard) => {
	return `/Cards/${CardImgName(card)}.png`
}

const CardImgName = (card: ICard) => {
	if (card.CardValue === undefined || card.Suit === undefined) {
		return 'card_back'
	}
	let valueName = CardValue[card.CardValue]
	if (card.CardValue < 9) {
		valueName = (card.CardValue + 2).toString()
	}
	valueName = valueName.toLowerCase()
	return `${valueName}_of_${Suit[card.Suit].toLowerCase()}`
}

export default Card
