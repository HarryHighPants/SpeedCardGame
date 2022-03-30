import { useState } from 'react'
import styled from 'styled-components'
import {ICard, CardValue, Suit} from "../Interfaces/ICard";

const Card = (card: ICard) => {
  const [position, setPosition] = useState({ x: 0, y: 0 })
  const trackPos = (data: any) => {
    setPosition({ x: data.x, y: data.y })
  }

  return (
      <CardParent>
        <CardElement>
          <img draggable="false" width={80} key={card.Id} src={CardImgSrc(card)} alt={CardImgName(card)} />
        </CardElement>
        <div>
          x: {position.x.toFixed(0)}, y: {position.y.toFixed(0)}
        </div>
      </CardParent>
  )
}

const CardParent = styled.div`
    background-color: #282c34;
    width: 80px;
    cursor: pointer;
    user-select: none;
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
