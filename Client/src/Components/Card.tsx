import { MouseEventHandler, useEffect, useState } from 'react'
import styled from 'styled-components'
import { ICard, CardValue, Suit, IPos, IRenderableCard } from '../Interfaces/ICard'
import { motion, PanInfo, useDragControls, useMotionValue, useTransform, Variants } from 'framer-motion'
import GameBoardLayout from '../Helpers/GameBoardLayout'

export interface Props {
	card: IRenderableCard
	gameBoardDimensions: IPos
	onDragStart: (info: PanInfo, card: IRenderableCard) => void
	onDrag: (info: PanInfo, card: IRenderableCard) => void
	onDragEnd: (info: PanInfo, card: IRenderableCard) => void
	draggingCard: IRenderableCard | undefined
}

const Card = ({ card, gameBoardDimensions, onDragStart, onDrag, onDragEnd, draggingCard }: Props) => {
	const [disablePointerEvents, setDisablePointerEvents] = useState(false)
	const [dragPosDelta, setDragPosDelta] = useState(0)
	const [distanceFromDraggingCard, setDistanceFromDraggingCard] = useState(Infinity)
	const [isDropTarget, setIsDropTarget] = useState(false)

	useEffect(() => {
		if (draggingCard?.Id === card.Id) return
		let draggingCardRect = draggingCard?.ref.current?.getBoundingClientRect()
		let distance = GetDistanceRect(draggingCardRect, card.ref.current?.getBoundingClientRect())
		setDistanceFromDraggingCard(distance)
		setIsDropTarget(distance < GameBoardLayout.dropDistance)
		if (isDropTarget) {
			console.log(true)
		}
		console.log(card.Id, distance)
	}, [draggingCard?.ref.current?.getBoundingClientRect().x])
	//
	// const logCardPos = (info: PanInfo) => {
	// 	let distance = GetDistance({ x: info.offset.x, y: info.offset.y }, { x: 0, y: 0 })
	// 	setDisablePointerEvents(distance > 150)
	// }

	const GetDistance = (pos1: IPos | undefined, pos2: IPos | undefined) => {
		if (!pos1 || !pos2) return Infinity
		let a = pos1.x - pos2.x
		let b = pos1.y - pos2.y
		return Math.sqrt(a * a + b * b)
	}

	const GetDistanceRect = (rect1: DOMRect | undefined, rect2: DOMRect | undefined) => {
		if (!rect1 || !rect2) return Infinity
		let a = rect1.x - rect2.x
		let b = rect1.y - rect2.y
		return Math.sqrt(a * a + b * b)
	}

	const OnStartDrag = (panInfo: PanInfo) => {
		// setDragging(true)
		onDragStart(panInfo, card)
	}

	const OnDrag = (panInfo: PanInfo) => {
		// setDragging(true)
		let distance = GetDistance({ x: panInfo.offset.x, y: panInfo.offset.y }, { x: 0, y: 0 })
		if (distance === dragPosDelta) return
		setDragPosDelta(distance)
		onDrag(panInfo, card)
	}

	const OnEndDrag = (panInfo: PanInfo) => {
		// setDragging(false)
		setDisablePointerEvents(false)
		onDragEnd(panInfo, card)
	}

	const cardVariants: Variants = {
		initial: {
			scale: 1,
			top: card.pos.y,
			boxShadow: '0',
			zIndex: card.zIndex
		},
		hovered: card.draggable
			? {
					scale: 1.03,
					top: card.pos.y - 20,
					boxShadow: '0px 3px 3px rgba(0,0,0,0.15)',
					pointerEvents: 'all',
			  }
			: {},
		dragging: card.draggable
			? {
					scale: 1.12,
					boxShadow: '0px 5px 5px rgba(0,0,0,0.1)',
					cursor: 'grabbing',
					zIndex: 15,
					top: card.pos.y - 20,
					pointerEvents: disablePointerEvents ? 'none' : 'all',
			  }
			: {},
	}

	return (
		<CardParent
			pos={card.pos}
			card={card}
			ref={card.ref}
			variants={cardVariants}
			drag={card.draggable}
			onDrag={(event, info) => OnDrag(info)}
			initial="initial"
			whileHover="hovered"
			whileTap="dragging"
			onDragStart={(e, info) => OnStartDrag(info)}
			onDragEnd={(e, info) => OnEndDrag(info)}
			dragSnapToOrigin={true}
			dragElastic={1}
			dragConstraints={{ top: 0, right: 0, bottom: 0, left: 0 }}
			dragMomentum={false}
			dropTarget={isDropTarget}
			distanceFromDraggingCard={distanceFromDraggingCard}
		>
			<CardImg dropTarget={isDropTarget} draggable="false" width={80} key={card.Id} src={CardImgSrc(card)} alt={CardImgName(card)} />
		</CardParent>
	)
}

const CardParent = styled(motion.div)<{
	dropTarget: boolean
	distanceFromDraggingCard: number
	card: IRenderableCard
	pos: IPos
}>`
	width: 80px;
	cursor: ${(p) => (p.card.draggable ? 'grab' : 'default')};
	position: absolute;
	left: ${(p) => p.pos.x}px;
	top: ${(p) => p.pos.y}px;
`

const CardImg = styled.img<{dropTarget: boolean}>`
	${(p) => (p.dropTarget ? 'filter: brightness(0.9)' : '')};
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
