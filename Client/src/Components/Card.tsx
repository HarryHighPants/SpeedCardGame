import { MouseEventHandler, useState } from 'react'
import styled from 'styled-components'
import { ICard, CardValue, Suit, IPos, IRenderableCard } from '../Interfaces/ICard'
import { motion, PanInfo, useDragControls, useMotionValue, useTransform } from 'framer-motion'

export interface Props {
	card: IRenderableCard
	gameBoardDimensions: IPos
	onDragStart: (info: PanInfo, card: IRenderableCard) => void
	onDragEnd: (info: PanInfo, card: IRenderableCard) => void
	onMouseEnter: (card: IRenderableCard) => void
	onMouseExit: (card: IRenderableCard) => void
}

const Card = ({ card, gameBoardDimensions, onDragStart, onDragEnd, onMouseEnter, onMouseExit }: Props) => {
	const [position, setPosition] = useState({ x: 0, y: 0 })
	const [isDragging, setDragging] = useState(false)

	const getPosPixels = (): IPos => {
		return {
			x: card.pos!! ? card.pos.x * gameBoardDimensions.x : 0,
			y: card.pos!! ? card.pos.y * gameBoardDimensions.y : 0,
		}
	}

	const logCardPos = (info: PanInfo) => {
		let posX = (info.point.x / gameBoardDimensions.x).toFixed(2)
		let posY = (info.point.y / window.innerHeight).toFixed(2)
	}

	const OnStartDrag = (panInfo: PanInfo) => {
		setDragging(true)
		onDragStart(panInfo, card)
	}

	const OnEndDrag = (panInfo: PanInfo) => {
		setDragging(false)
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

	return (
		<CardParent
			pos={getPosPixels()}
			card={card}
			// dragControls={dragControls}
			drag={card.draggable}
			onDrag={(event, info) => logCardPos(info)}
			whileHover={
				card.draggable
					? {
							scale: 1.03,
							top: getPosPixels().y-20,
							boxShadow: '0px 3px 3px rgba(0,0,0,0.15)',
					  }
					: {}
			}
			whileTap={
				card.draggable
					? {
							scale: 1.12,
							boxShadow: '0px 5px 5px rgba(0,0,0,0.1)',
							cursor: 'grabbing',
							pointerEvents: 'none',
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
			onMouseEnter={(e) => OnMouseEnter(e)}
		>
			<CardElement>
				<img draggable="false" width={80} key={card.Id} src={CardImgSrc(card)} alt={CardImgName(card)} />
			</CardElement>
		</CardParent>
	)
}

const CardParent = styled(motion.div)<{ card: IRenderableCard; pos: IPos }>`
	width: 80px;
	cursor: ${(p) => (p.card.draggable ? 'grab' : 'default')};
	position: absolute;
	left: ${(p) => p.pos.x}px;
	top: ${(p) => p.pos.y}px;
	z-index: ${(p) => p.card.zIndex};
	${(p) => (p.card.hoveredDropTarget ? 'filter: brightness(-0.25)' : '')};
`

const CardElement = styled.div``

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
