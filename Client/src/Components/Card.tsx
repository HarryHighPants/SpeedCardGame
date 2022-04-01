import { MouseEventHandler, Ref, useEffect, useRef, useState } from 'react'
import styled from 'styled-components'
import { ICard, CardValue, Suit, IPos, IRenderableCard, IDraggedRenderableCard } from '../Interfaces/ICard'
import { motion, PanInfo, useDragControls, useMotionValue, useTransform } from 'framer-motion'
import GameBoardLayout from '../Helpers/GameBoardLayout'

export interface Props {
	card: IRenderableCard
	gameBoardDimensions: IPos
	onDragStart: (rect: DOMRect | undefined, card: IRenderableCard) => void
	onDragEnd: (rect: DOMRect | undefined, card: IRenderableCard) => void
	onDrag: (rect: DOMRect | undefined, card: IRenderableCard) => void
	onDroppedOn: (card: IRenderableCard, cardDropped: IDraggedRenderableCard) => void
	draggedCard: IDraggedRenderableCard | undefined
}

const Card = ({ card, gameBoardDimensions, onDragStart, onDrag, onDragEnd, draggedCard, onDroppedOn }: Props) => {
	const [isDragging, setDragging] = useState(false)
	const [hoveredDropTarget, setHoveredDropTarget] = useState(false)
	const dragApplyDistance = useState()
	const inputRef = useRef<HTMLDivElement>(null)

	const OnStartDrag = (panInfo: PanInfo) => {
		setDragging(true)
		onDragStart(inputRef.current?.getBoundingClientRect(), card)
	}

	const OnEndDrag = (panInfo: PanInfo) => {
		setDragging(false)
		onDragEnd(inputRef.current?.getBoundingClientRect(), card)
	}

	const GetDistance = (rect1: DOMRect | undefined, rect2: DOMRect | undefined) => {
		if (!rect1 || !rect2) return Infinity
		let a = rect1.x - rect2.x
		let b = rect1.y - rect2.y
		return Math.sqrt(a * a + b * b)
	}

	// useEffect(() => {
	// 	if (draggedCard?.Id === card.Id) return
	// 	let distance = GetDistance(inputRef.current?.getBoundingClientRect(), draggedCard?.domRect)
	// 	setHoveredDropTarget(distance < GameBoardLayout.dropDistance)
	//
	// 	// onDroppedOn()
	// }, [draggedCard])

	return (
		<CardParent
			hoveredDropTarget={hoveredDropTarget}
			ref={inputRef}
			pos={card.pos}
			card={card}
			onDrag={(e, info) => onDrag(inputRef?.current?.getBoundingClientRect(), card)}
			drag={card.draggable}
			whileHover={
				card.draggable
					? {
							// scale: 1.03,
							top: card.pos.y - 20,
							boxShadow: '0px 5px 10px rgba(0,0,0,0.1)',
					  }
					: {}
			}
			whileTap={
				card.draggable
					? {
							scale: 1.12,
							boxShadow: '0px 5px 20px rgba(0,0,0,0.2)',
							cursor: 'grabbing',
							// pointerEvents: 'none',
							top: card.pos.y - 20,
					  }
					: {}
			}
			onDragStart={(e, info) => OnStartDrag(info)}
			onDragEnd={() => setDragging(false)}
			// dragSnapToOrigin={true} //
			// dragTransition={{ bounceStiffness: 500, bounceDamping: 200 }}
			dragElastic={1}
			dragConstraints={{ top: 0, right: 0, bottom: 0, left: 0 }}
			dragMomentum={false}
		>
			<CardImg
				hoveredDropTarget={hoveredDropTarget}
				card={card}
				draggable="false"
				width={80}
				key={card.Id}
				src={CardImgSrc(card)}
				alt={CardImgName(card)}
			/>
		</CardParent>
	)
}

const CardParent = styled(motion.div)<{ hoveredDropTarget: boolean; card: IRenderableCard; pos: IPos }>`
	width: 80px;
	cursor: ${(p) => (p.card.draggable ? 'grab' : 'default')};
	position: absolute;
	left: ${(p) => p.pos.x}px;
	top: ${(p) => (p.hoveredDropTarget ? p.pos.y + 50 : p.pos.y)}px;
	z-index: ${(p) => p.card.zIndex};
	display: flex;
	user-select: none;
	box-shadow: '0px 10px 40px rgba(0,0,0,${(p) => (p.hoveredDropTarget ? 1 : 0)})',
		${(p) => (p.hoveredDropTarget ? 'filter: brightness(-0.25)' : '')};
	scale: ${(p) => (p.hoveredDropTarget ? 3 : 1)};
`

const CardImg = styled.img<{ hoveredDropTarget: boolean; card: IRenderableCard }>`
	box-shadow: '0px 10px 40px rgba(0,0,0,${(p) => (p.hoveredDropTarget ? 1 : 0)})',
		${(p) => (p.hoveredDropTarget ? 'filter: brightness(-0.25)' : '')};
	scale: ${(p) => (p.hoveredDropTarget ? 3 : 1)};
	user-select: none;
`

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
