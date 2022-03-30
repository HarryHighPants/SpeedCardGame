// When rendering cards we need a
// A: Give them the cards from state and moved cards
// B:

// Player has a callback for on moveCard and onPlayCard

import { IPlayer } from '../Interfaces/IPlayer'
import { ICard } from '../Interfaces/ICard'
import Card from './RenderCard'
import { IMovedCardPos } from './Game'

interface Props {
    movedCards: IMovedCardPos[]
    player: IPlayer
}

const Player = ({ player, movedCards }: Props) => {
    return (
        <div>
            <div>
                <p>{player.Name}</p>
                {player.RequestingTopUp && <p>Requesting top up</p>}
            </div>
            {RenderHandCards(player.HandCards)}
        </div>
    )
}

const RenderHandCards = (cards: ICard[]) => {
    return <div>{cards.map((c) => Card(c))}</div>
}

export default Player
