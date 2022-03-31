import { useState } from 'react'
import styled from 'styled-components'
import { ICard, CardValue, Suit, IPos, IRenderableCard } from '../Interfaces/ICard'
import { motion, PanInfo, useDragControls, useMotionValue, useTransform } from 'framer-motion'

export interface Props {
    card: IRenderableCard
    gameBoardDimensions: IPos
    highestCardZIndex: number
    onDragStart: (info: PanInfo, card: IRenderableCard) => void
    onDragEnd: (info: PanInfo, card: IRenderableCard) => void
}

const Card = ({ card, gameBoardDimensions, onDragStart, onDragEnd, highestCardZIndex }: Props) => {
    const [position, setPosition] = useState({ x: 0, y: 0 })
    const [isDragging, setDragging] = useState(false)
	const [zIndex, setZIndex] = useState(1)


	const getPosPixels = (): IPos => {
        return {
            x: card.pos!! ? card.pos.x * gameBoardDimensions.x : 0,
            y: card.pos!! ? card.pos.y * gameBoardDimensions.y : 0,
        }
    }

    const logCardPos = (info: PanInfo) => {
        let posX = (info.point.x / gameBoardDimensions.x).toFixed(2)
        let posY = (info.point.y / window.innerHeight).toFixed(2)

        console.log(`{ x: ${posX}, y: ${posY} }`)
        console.log(card.pos)
    }

    const OnStartDrag = (panInfo: PanInfo) => {
        setDragging(true)
		setZIndex(highestCardZIndex);
        onDragStart(panInfo, card)
    }

    const OnEndDrag = (panInfo: PanInfo) => {
        setDragging(false)
        onDragEnd(panInfo, card)
    }

    return (
        <CardParent
            pos={getPosPixels()}
            cardZIndex={zIndex}
            // dragControls={dragControls}
            drag
            onDrag={(event, info) => logCardPos(info)}
            whileHover={{
                scale: 1.03,
                boxShadow: '0px 3px 3px rgba(0,0,0,0.15)',
            }}
            whileTap={{
                scale: 1.12,
                boxShadow: '0px 5px 5px rgba(0,0,0,0.1)',
                cursor: 'grabbing',
            }}
            onDragStart={(e, info) => OnStartDrag(info)}
            onDragEnd={() => setDragging(false)}
            dragSnapToOrigin={true} //
            // dragTransition={{ bounceStiffness: 600, bounceDamping: 20 }}
            // dragElastic={0.7}
            // dragConstraints={{ top: 0, right: 0, bottom: 0, left: 0 }}
            dragMomentum={false}
        >
            <CardElement>
                <img draggable="false" width={80} key={card.Id} src={CardImgSrc(card)} alt={CardImgName(card)} />
            </CardElement>
            <div>{/*x: {x.get().toString()}, y: {scale.get().toString()}*/}</div>
        </CardParent>
    )
}

const CardParent = styled(motion.div)<{ pos: IPos; cardZIndex: number }>`
    background-color: #84a8e8;
    width: 80px;
    cursor: grab;
    position: absolute;
    left: ${(p) => p.pos.x}px;
    top: ${(p) => p.pos.y}px;
    z-index: ${(p) => p.cardZIndex};
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
