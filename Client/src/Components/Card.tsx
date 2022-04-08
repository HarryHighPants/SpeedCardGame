import { useEffect, useState } from 'react'
import styled from 'styled-components'
import { CardLocationType, CardValue, ICard, IPos, IRenderableCard, Suit } from '../Interfaces/ICard'
import { AnimatePresence, motion, motionValue, PanInfo, useAnimation, useTransform, Variants } from 'framer-motion'
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
	const controls = useAnimation()
	const [shaking, setShaking] = useState(false)
	const [horizontalOffset, setHorizontalOffset] = useState(0)
	const shakingAmount = 10
	const transitionDuration = 0.5

	useEffect(() => {
		controls.start('idle')
	}, [])

	useEffect(() => {
		if (shaking) {
			controls.start('preAnimate').then(() => controls.start('animate'))
		} else {
			controls.start('idle')
		}
	}, [shaking])

	const UpdateAnimationStates = (distance: number, overlaps?: boolean, delta?: IPos) => {
		if (distance === Infinity) {
			// Reset states
			setShaking(false)
			setHighlighted(false)
			setHorizontalOffset(0)
			controls.start('idle')
			return
		}

		// Check if we are a center card that can be dropped onto
		let droppingOntoCenter =
			card.location === CardLocationType.Center && cardBeingDragged?.location === CardLocationType.Hand
		if (droppingOntoCenter) {
			// console.log(distance < GameBoardLayout.dropDistance * 2 ? 0.2 : 999)
			let shouldShake = distance < GameBoardLayout.dropDistance * 2
			setShaking(shouldShake)

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
		// setDragging(true)
		setDraggingCard({ ...card })
	}

	const OnDrag = (panInfo: PanInfo) => {
		// setDragging(true)
		let distance = GetDistance({ X: panInfo.offset.x, Y: panInfo.offset.y }, { X: 0, Y: 0 })
		if (distance.toFixed(0) === dragPosDelta.toFixed(0)) return
		setDragPosDelta(distance)
		setDraggingCard({ ...card })
	}

	const OnEndDrag = (panInfo: PanInfo) => {
		// setDragging(false)
		setDisablePointerEvents(false)
		onDragEnd(card)
	}

	const cardVariants: Variants = {
		initial: {
			// opacity: 0,
			zIndex: card.zIndex,
			rotate: 0,
			top: card.pos.Y,
			left: card.pos.X + horizontalOffset + (card.animateInHorizontalOffset ?? 0),
			transition: { ease: 'linear', duration: transitionDuration },
		},
		idle: {
			// opacity: 1,
			zIndex: card.zIndex,
			top: card.pos.Y,
			left: card.pos.X + horizontalOffset,
			rotate: 0,
			transition: {
				ease: 'linear',
				duration: transitionDuration,
			},
		},
		preAnimate: {
			rotate: -shakingAmount,
			transition: {
				duration: transitionDuration,
			},
		},
		animate: {
			rotate: [-shakingAmount, shakingAmount],
			transition: {
				duration: 0.3,
				repeat: Infinity,
				repeatType: 'reverse',
			},
		},
		hovered: card.ourCard
			? {
					scale: 1.03,
					top: card.pos.Y - 20,
					boxShadow: '0px 7px 15px rgba(0,0,0,0.3)',
					transition: { ease: 'easeOut', duration: transitionDuration },
			  }
			: {},
		dragging: {
			scale: 1.12,
			boxShadow: '0px 15px 30px rgba(0,0,0,0.5)',
			cursor: 'grabbing',
			zIndex: 15,
			top: card.pos.Y - 20,
			transition: { ease: 'easeOut', duration: transitionDuration },
		},
		exit: {
			opacity: 0,
			transition: { ease: 'easeOut', duration: transitionDuration },
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
				layout
				ref={card.ref}
				key={`card-${card.Id}`}
				layoutId={`card-${card.Id}`}
				variants={cardVariants}
				initial="initial"
				animate={controls}
				// animate="idle"
				whileHover="hovered"
				whileDrag="dragging"
				exit="exit"
				$grabCursor={card.ourCard}
				drag={card.ourCard}
				onDrag={(event, info) => OnDrag(info)}
				onDragStart={(e, info) => OnStartDrag(info)}
				onDragEnd={(e, info) => OnEndDrag(info)}
				dragSnapToOrigin={true}
				dragElastic={1}
				// dragConstraints={{ top: 0, right: 0, bottom: 0, left: 0 }}
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

const CardParent = styled(motion.div)<{ $grabCursor: boolean }>`
	width: 80px;
	cursor: ${(p) => (p.$grabCursor ? 'grab' : 'default')};
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
