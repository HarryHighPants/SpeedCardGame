import { memo, useCallback, useEffect, useState } from 'react'
import styled from 'styled-components'
import { CardLocationType, CardValue, ICard, IRenderableCard, Suit } from '../Interfaces/ICard'
import { GetDistance } from '../Helpers/Utilities'
import { motion, PanInfo, Variants } from 'framer-motion'
import { AnimationDefinition } from 'framer-motion/types/render/utils/animation'

export interface Props {
    card: IRenderableCard
    draggingCardUpdated: (card: IRenderableCard) => void
    onDragEnd: (card: IRenderableCard) => void
}

const Card = memo(({ card, onDragEnd, draggingCardUpdated }: Props) => {
    const [dragPosDelta, setDragPosDelta] = useState(0)
    const [transitionDelay, setTransitionDelay] = useState(card.animateInDelay)
    const [, updateState] = useState({})
    const forceUpdate = useCallback(() => updateState({}), [])

    useEffect(() => {
        card.forceUpdate = forceUpdate
    }, [])

    const OnStartDrag = (panInfo: PanInfo) => {
        draggingCardUpdated({ ...card })
    }

    const OnDrag = (panInfo: PanInfo) => {
        let distance = GetDistance({ x: panInfo.offset.x, y: panInfo.offset.y }, { x: 0, y: 0 })
        if (distance.toFixed(0) === dragPosDelta.toFixed(0)) return
        setDragPosDelta(distance)
        draggingCardUpdated({ ...card })
    }

    const OnEndDrag = (panInfo: PanInfo) => {
        onDragEnd({ ...card })
    }

    const transitionDuration = 0.35
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
            top: card.pos.y,
            left: card.pos.x + card.horizontalOffset + (card.animateInHorizontalOffset ?? 0),
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
            top: card.pos.y,
            left: card.pos.x + card.horizontalOffset,
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
                  top: card.pos.y - 20,
                  boxShadow: '0px 0px 20px rgba(0,0,0,0.4)',
                  transition: defaultTransition,
              }
            : {},
        dragging: {
            scale: 1.12,
            boxShadow: '0px 30px 30px rgba(0,0,0,0.6)',
            cursor: 'grabbing',
            zIndex: 40,
            top: card.pos.y - 20,
            transition: defaultTransition,
        },
        exit: {
            opacity: 0,
            zIndex: 0,
            transition: {
                ease: 'easeOut',
                duration: 0.02,
                opacity: {
                    delay: 0,
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
            key={`card-${card.id}`}
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
                draggable="false"
                width={80}
                height={116.1}
                key={card.id}
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

const CardImg = styled.img``

const CardImgSrc = (card: ICard) => {
    return `/Cards/${CardImgName(card)}.png`
}

const CardImgName = (card: ICard) => {
    if (card.cardValue === undefined || card.suit === undefined) {
        return 'card_back'
    }
    let valueName = CardValue[card.cardValue]
    if (card.cardValue < 9) {
        valueName = (card.cardValue + 2).toString()
    }
    valueName = valueName.toLowerCase()
    return `${valueName}_of_${Suit[card.suit].toLowerCase()}`
}

export default Card
