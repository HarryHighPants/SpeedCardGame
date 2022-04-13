import { memo, useCallback, useEffect, useState } from 'react'
import styled from 'styled-components'
import { CardLocationType, CardValue, ICard, IPos, IRenderableCard, Suit } from '../Interfaces/ICard'
import GameBoardLayout from '../Helpers/GameBoardLayout'
import { usePrevious } from '../Helpers/UsePrevious'
import Droppable from './Droppable'
import { GetDistance, GetDistanceRect, Overlaps } from '../Helpers/Utilities'
import { motion, PanInfo, Variants } from 'framer-motion'
import { AnimationDefinition } from 'framer-motion/types/render/utils/animation'

export interface Props {
	card: IRenderableCard
	draggingCardUpdated: (card: IRenderableCard) => void
	onDragEnd: (card: IRenderableCard) => void
}

const Card = memo(({ card, onDragEnd, draggingCardUpdated}: Props) => {
	const [disablePointerEvents, setDisablePointerEvents] = useState(false)
	const [dragPosDelta, setDragPosDelta] = useState(0)
	const [transitionDelay, setTransitionDelay] = useState(card.animateInDelay)
	const [, updateState] = useState({});
	const forceUpdate = useCallback(() => updateState({}), []);

	useEffect(()=>{
		card.forceUpdate = forceUpdate;
	}, [])

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
			left: card.pos.X + card.horizontalOffset + (card.animateInHorizontalOffset ?? 0),
			transition: {
				type: defaultTransition.type,
				ease: defaultTransition.ease,
				duration: defaultTransition.duration,
				delay: transitionDelay,
			},
		},
		animate: {
			opacity: 1,
			zIndex: card.zIndex,
			top: card.pos.Y,
			left: card.pos.X + card.horizontalOffset,
			boxShadow: '0px 0px 10px rgba(0,0,0,0.3)',
			transition: {
				type: defaultTransition.type,
				ease: defaultTransition.ease,
				duration: defaultTransition.duration,
				delay: transitionDelay,
			},
		},
		hovered: card.ourCard
			? {
					scale: 1.03,
					top: card.pos.Y - 20,
					boxShadow: '0px 0px 20px rgba(0,0,0,0.4)',
					transition: defaultTransition,
			  }
			: {},
		dragging: {
			scale: 1.12,
			boxShadow: '0px 30px 30px rgba(0,0,0,0.6)',
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

	const AnimEnd = (anim: AnimationDefinition) => {
		if (card.animateInDelay === transitionDelay) {
			setTransitionDelay(0)
		}
	}

	return (
		<CardParent
			ref={card.ref}
			key={`card-${card.Id}`}
			variants={cardVariants}
			initial="initial"
			animate="animate"
			whileHover="hovered"
			whileDrag="dragging"
			exit="exit"
			onAnimationComplete={(a) => AnimEnd(a)}
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
				highlighted={card.highlighted}
				draggable="false"
				width={80}
				height={116.1}
				key={card.Id}
				src={CardImgSrc(card)}
				alt={CardImgName(card)}
			/>
		</CardParent>
	)
})

const CardParent = styled(motion.div)<{ $grabCursor: boolean }>`
	width: 80px;
	cursor: ${(p) => (p.$grabCursor ? 'grab' : 'default')};
	position: absolute;
	display: flex;
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
