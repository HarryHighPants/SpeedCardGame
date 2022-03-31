import { useState } from 'react'
import styled from 'styled-components'
import {ICard, CardValue, Suit, IPos} from '../Interfaces/ICard'
import { motion, PanInfo, useDragControls, useMotionValue, useTransform } from 'framer-motion'

const Card = (card: ICard, gameBoardDimensions: IPos
) => {
    const [position, setPosition] = useState({ x: 0, y: 0 })
    const trackPos = (data: any) => {
        setPosition({ x: data.x, y: data.y })
    }


    const logCardPos = (info: PanInfo)=>{
      let posX = (info.point.x / gameBoardDimensions.x).toFixed(2);
      let posY = (info.point.y / window.innerHeight).toFixed(2);

      console.log(`{ x: ${posX}, y: ${posY} }`)
    }
    const dragControls = useDragControls()
    return (
        <CardParent
            dragControls={dragControls}
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
            // dragSnapToOrigin={true} //
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

const CardParent = styled(motion.div)`
    background-color: #84a8e8;
    width: 80px;
    cursor: grab;
    //cursor: pointer;
    //user-select: none;
`

const CardElement = styled.div``

const CardImgSrc = (card: ICard) => {
    return `/Cards/${CardImgName(card)}.png`
}

const CardImgName = (card: ICard) => {
    let valueName = CardValue[card.CardValue]
    if (card.CardValue < 9) {
        valueName = (card.CardValue + 2).toString()
    }
    valueName = valueName.toLowerCase()
    return `${valueName}_of_${Suit[card.Suit].toLowerCase()}`
}

export default Card
