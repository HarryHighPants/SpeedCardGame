import { useEffect, useState } from 'react'
import styled from 'styled-components'
import { CardLocationType, CardValue, ICard, IPos, IRenderableCard, Suit } from '../Interfaces/ICard'
import { motion, PanInfo, Variants } from 'framer-motion'
import GameBoardLayout from '../Helpers/GameBoardLayout'
import {usePrevious} from "../Helpers/UsePrevious";
import {GetDistance, GetDistanceRect} from "../Helpers/Distance";

export interface Props {
	card: IRenderableCard
	onDragStart: (info: PanInfo, card: IRenderableCard) => void
	onDrag: (info: PanInfo, card: IRenderableCard) => void
	onDragEnd: (info: PanInfo, card: IRenderableCard) => void
	draggingCard: IRenderableCard | undefined
}

const Card = ({ card, onDragStart, onDrag, onDragEnd, draggingCard }: Props) => {
	const [disablePointerEvents, setDisablePointerEvents] = useState(false)
	const [dragPosDelta, setDragPosDelta] = useState(0)
	const [highlighted, setHighlighted] = useState(false)
	const [shakingAmt, setShakingAmt] = useState(0)
	const [horizontalOffset, setHorizontalOffset] = useState(0)
	const [initialRect, setInitialRect] = useState<DOMRect | undefined>()

	useEffect(() => {
		setInitialRect(card.ref.current?.getBoundingClientRect())
	}, [card.ref])

	useEffect(() => {
		if (draggingCard?.Id === card.Id) return

		let draggingCardRect = draggingCard?.ref.current?.getBoundingClientRect()
		let ourRect = card.ref.current?.getBoundingClientRect()
		if (!draggingCardRect || !ourRect) {
			// Reset states
			setShakingAmt(0)
			setHighlighted(false)
			setHorizontalOffset(0)
			return
		}

		let distance = GetDistanceRect(draggingCardRect, ourRect)

		// Check if we are a center card that can be dropped onto
		let droppingOntoCenter =
			card.location === CardLocationType.Center && draggingCard?.location === CardLocationType.Hand
		if (droppingOntoCenter) {
			setShakingAmt((1 / distance) * 100)
			setHighlighted(distance < GameBoardLayout.dropDistance)
		}

		// Check if we are a hand card that can be dragged onto
		let droppingOntoHandCard =
			card.ourCard && card.location === CardLocationType.Hand && draggingCard?.location === CardLocationType.Kitty
		if (droppingOntoHandCard) {
			if (!initialRect) return
			// We want to animate to either the left or the right on the dragged kitty card
			setHorizontalOffset((draggingCardRect.x < initialRect.x ? 1 : 0) * 50)
		}
	}, [draggingCard?.ref.current?.getBoundingClientRect().x, draggingCard])

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
			zIndex: card.zIndex,
			top: card.pos.y,
			left: card.pos.x + horizontalOffset,
			rotate: shakingAmt,
			transition: { ease: 'linear' },
		},
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
		<CardParent
			key={`card-${card.Id}`}
			layoutId={card.Id.toString()}
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
