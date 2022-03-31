// When rendering cards we need a
// A: Give them the cards from state and moved cards
// B:

// Player has a callback for on moveCard and onPlayCard

import { IPlayer } from '../Interfaces/IPlayer'
import {ICard, IPos} from '../Interfaces/ICard'
import Card from './Card'
import { IMovedCardPos } from './Game'

interface Props {
    movedCards: IMovedCardPos[]
    player: IPlayer
  gameBoardDimensions: IPos
}

const Player = ({ player, movedCards, gameBoardDimensions }: Props) => {
    return (
        <div>
            <div>
                <p>{player.Name}</p>
                {player.RequestingTopUp && <p>Requesting top up</p>}
            </div>
            {RenderHandCards(player.HandCards, gameBoardDimensions)}
        </div>
    )
}

const RenderHandCards = (cards: ICard[], gameBoardDimensions: IPos) => {
    return <div>{cards.map((c) => Card(c, gameBoardDimensions))}</div>
}

export default Player
