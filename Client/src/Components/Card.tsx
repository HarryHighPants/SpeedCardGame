import { useEffect, useState } from 'react'
import styled from 'styled-components'
import { CardLocationType, CardValue, ICard, IPos, IRenderableCard, Suit } from '../Interfaces/ICard'
import { motion, PanInfo, Variants } from 'framer-motion'
import GameBoardLayout from '../Helpers/GameBoardLayout'
import { usePrevious } from '../Helpers/UsePrevious'
import Droppable from './Droppable'
import { GetDistance } from '../Helpers/Utilities'

export interface Props {
	card: IRenderableCard
	setDraggingCard: (card: IRenderableCard) => void
	onDragEnd: (card: IRenderableCard) => void
	cardBeingDragged: IRenderableCard | undefined
}

const Card = ({ card, onDragEnd, setDraggingCard, cardBeingDragged }: Props) => {
	const [disablePointerEvents, setDisablePointerEvents] = useState(false)
	const [dragPosDelta, setDragPosDelta] = useState(0)
	const [highlighted, setHighlighted] = useState(false)
	const [shakingAmt, setShakingAmt] = useState(0)
	const [horizontalOffset, setHorizontalOffset] = useState(0)

	const UpdateAnimationStates = (distance: number, overlaps?: boolean, delta?: IPos) => {
		if (distance === Infinity) {
			// Reset states
			setShakingAmt(0)
			setHighlighted(false)
			setHorizontalOffset(0)
			return
		}

		// Check if we are a center card that can be dropped onto
		let droppingOntoCenter =
			card.location === CardLocationType.Center && cardBeingDragged?.location === CardLocationType.Hand
		if (droppingOntoCenter) {
			setShakingAmt((1 / distance) * 100)
			setHighlighted(distance < GameBoardLayout.dropDistance)
		}

		// Check if we are a hand card that can be dragged onto
		let droppingOntoHandCard =
			card.ourCard &&
			card.location === CardLocationType.Hand &&
			cardBeingDragged?.location === CardLocationType.Kitty
		if (droppingOntoHandCard) {
			// We want to animate to either the left or the right on the dragged kitty card
			setHorizontalOffset((!!delta && delta?.x < 0 ? 1 : 0) * 50)
		}
	}

	const OnStartDrag = (panInfo: PanInfo) => {
		// setDragging(true)
		setDraggingCard({ ...card })
	}

	const OnDrag = (panInfo: PanInfo) => {
		// setDragging(true)
		let distance = GetDistance({ x: panInfo.offset.x, y: panInfo.offset.y }, { x: 0, y: 0 })
		if (distance === dragPosDelta) return
		setDragPosDelta(distance)
		setDraggingCard({ ...card })
	}

	const OnEndDrag = (panInfo: PanInfo) => {
		// setDragging(false)
		setDisablePointerEvents(false)
		onDragEnd(card)
	}

	const cardVariants: Variants = {
		initial: (shakingAmt)=>( {
			scale: 1,
			zIndex: card.zIndex,
			top: card.pos.y,
			left: card.pos.x + horizontalOffset,
			rotate: shakingAmt,
			transition: { ease: 'linear' },
		}),
		hovered: card.ourCard
			? {
					scale: 1.03,
					top: card.pos.y - 20,
					boxShadow: '0px 3px 3px rgba(0,0,0,0.15)',
			  }
			: {},
		dragging: {
			scale: 1.12,
			boxShadow: '0px 5px 5px rgba(0,0,0,0.1)',
			cursor: 'grabbing',
			zIndex: 15,
			top: card.pos.y - 20,
		},
	}

	return (
		<Droppable
			id={card.Id}
			cardBeingDragged={cardBeingDragged}
			ourRef={card.ref}
			onDistanceUpdated={UpdateAnimationStates}
		>
			<CardParent
				// animate={{ rotate: [-shakingAmt, shakingAmt], transition: { repeat: Infinity } }}
				custom={shakingAmt}
				layoutId={`card-${card.Id}`}
				pos={card.pos}
				whileInView={'initial'}
				card={card}
				ref={card.ref}
				variants={cardVariants}
				drag={card.ourCard}
				onDrag={(event, info) => OnDrag(info)}
				initial="initial"
				whileHover="hovered"
				whileDrag="dragging"
				onDragStart={(e, info) => OnStartDrag(info)}
				onDragEnd={(e, info) => OnEndDrag(info)}
				dragSnapToOrigin={true}
				dragElastic={1}
				dragConstraints={{ top: 0, right: 0, bottom: 0, left: 0 }}
				dragMomentum={false}
			>
				<CardImg
					highlighted={highlighted}
					draggable="false"
					width={80}
					key={card.Id}
					src={CardImgSrc(card)}
					alt={CardImgName(card)}
				/>
			</CardParent>
		</Droppable>
	)
}

const CardParent = styled(motion.div)<{
	card: IRenderableCard
	pos: IPos
}>`
	width: 80px;
	cursor: ${(p) => (p.card.ourCard ? 'grab' : 'default')};
	position: absolute;
`

const CardImg = styled.img<{ highlighted: boolean }>`
	${(p) => (p.highlighted ? 'filter: brightness(0.9)' : '')};
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
