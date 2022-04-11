import { useEffect, useState } from 'react'
import styled from 'styled-components'
import { CardLocationType, CardValue, ICard, IPos, IRenderableCard, Suit } from '../Interfaces/ICard'
import GameBoardLayout from '../Helpers/GameBoardLayout'
import { usePrevious } from '../Helpers/UsePrevious'
import Droppable from './Droppable'
import { GetDistance } from '../Helpers/Utilities'
import { motion, PanInfo, Variants } from 'framer-motion'
import { AnimationDefinition } from 'framer-motion/types/render/utils/animation'

export interface Props {
	card: IRenderableCard
	draggingCardUpdated: (card: IRenderableCard) => void
	onDragEnd: (card: IRenderableCard) => void
	cardBeingDragged: IRenderableCard | undefined
}

const Card = ({ card, onDragEnd, draggingCardUpdated, cardBeingDragged }: Props) => {
	const [disablePointerEvents, setDisablePointerEvents] = useState(false)
	const [dragPosDelta, setDragPosDelta] = useState(0)
	const [highlighted, setHighlighted] = useState(false)
	const [horizontalOffset, setHorizontalOffset] = useState(0)
	const [transitionDelay, setTransitionDelay] = useState(card.animateInDelay)

	const UpdateAnimationStates = (distance: number, overlaps?: boolean, delta?: IPos) => {
		if (distance === Infinity) {
			// Reset states
			setHighlighted(false)
			setHorizontalOffset(0)
			return
		}

		// Check if we are a center card that can be dropped onto
		let droppingOntoCenter =
			card.location === CardLocationType.Center && cardBeingDragged?.location === CardLocationType.Hand
		if (droppingOntoCenter) {
			setHighlighted(distance < GameBoardLayout.dropDistance)
		}

		// Check if we are a hand card that can be dragged onto
		let droppingOntoHandCard =
			card.ourCard &&
			card.location === CardLocationType.Hand &&
			cardBeingDragged?.location === CardLocationType.Kitty
		if (droppingOntoHandCard) {
			// We want to animate to either the left or the right on the dragged kitty card
			setHorizontalOffset((!!delta && delta?.X < 0 ? 1 : 0) * 50)
		}
	}

	const OnStartDrag = (panInfo: PanInfo) => {
		draggingCardUpdated({ ...card })
	}

	const OnDrag = (panInfo: PanInfo) => {
		let distance = GetDistance({ X: panInfo.offset.x, Y: panInfo.offset.y }, { X: 0, Y: 0 })
		if (distance.toFixed(0) === dragPosDelta.toFixed(0)) return
		setDragPosDelta(distance)
		draggingCardUpdated({ ...card })
	}

	const OnEndDrag = (panInfo: PanInfo) => {
		setDisablePointerEvents(false)
		onDragEnd({ ...card })
	}

	const transitionDuration = 0.5
	const defaultEase = 'easeOut'
	const defaultTransition = {
		type: 'tween',
		ease: defaultEase,
		duration: transitionDuration,
	}
	const cardVariants: Variants = {
		initial: {
			opacity: card.startTransparent ? 0 : 1,
			zIndex: card.animateInZIndex,
			top: card.pos.Y,
			left: card.pos.X + horizontalOffset + (card.animateInHorizontalOffset ?? 0),
			transition: {
				type: defaultTransition.type,
				ease: defaultTransition.ease,
				duration: defaultTransition.duration,
				delay: transitionDelay
			},
		},
		animate: {
			opacity: 1,
			zIndex: card.zIndex,
			top: card.pos.Y,
			left: card.pos.X + horizontalOffset,
			transition: {
				type: defaultTransition.type,
				ease: defaultTransition.ease,
				duration: defaultTransition.duration,
				delay: transitionDelay
			},
		},
		hovered: card.ourCard
			? {
					scale: 1.03,
					top: card.pos.Y - 20,
					boxShadow: '0px 7px 15px rgba(0,0,0,0.3)',
					transition: defaultTransition,
			  }
			: {},
		dragging: {
			scale: 1.12,
			boxShadow: '0px 15px 30px rgba(0,0,0,0.5)',
			cursor: 'grabbing',
			zIndex: 20,
			top: card.pos.Y - 20,
			transition: defaultTransition,
		},
		exit: {
			opacity: 0,
			zIndex: 0,
			transition: {
				ease: 'easeOut',
				duration: 0.2,
				opacity: {
					delay: 1,
				},
			},
		},
	}

	const AnimEnd = (anim:  AnimationDefinition) => {
		if(card.animateInDelay === transitionDelay){
			setTransitionDelay(0)
			// setTransitionZIndex(card.zIndex)
		}
	}

	return (
		<Droppable
			id={card.Id}
			cardBeingDragged={cardBeingDragged}
			ourRef={card.ref}
			onDistanceUpdated={UpdateAnimationStates}
		>
			<CardParent
				ref={card.ref}
				key={`card-${card.Id}`}
				// layoutId={`card-${card.Id}`}
				variants={cardVariants}
				initial="initial"
				animate="animate"
				whileHover="hovered"
				whileDrag="dragging"
				exit="exit"
				onAnimationComplete={(a)=>AnimEnd(a)}
				$grabCursor={card.ourCard}
				drag={card.ourCard}
				onDrag={(e: any, info: PanInfo) => OnDrag(info)}
				onDragStart={(e: any, info: PanInfo) => OnStartDrag(info)}
				onDragEnd={(e: any, info: PanInfo) => OnEndDrag(info)}
				dragSnapToOrigin={true}
				dragMomentum={false}
				dragElastic={1}
				dragTransition={defaultTransition}
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

const CardParent = styled(motion.div)<{ $grabCursor: boolean }>`
	width: 80px;
	cursor: ${(p) => (p.$grabCursor ? 'grab' : 'default')};
	position: absolute;
`

const CardImg = styled.img<{ highlighted: boolean }>`
	${(p) => (p.highlighted ? 'filter: brightness(0.85)' : '')};
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
