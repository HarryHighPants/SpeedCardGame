import { IPlayer } from '../Interfaces/IPlayer'
import { ICard, IPos } from '../Interfaces/ICard'
import Card from './Card'
import { IMovedCardPos } from './Game'

interface Props {
    player: IPlayer
}

const Player = ({ player}: Props) => {
    return (
        <div>
            <div>
                <p>{player.Name}</p>
                {player.RequestingTopUp && <p>Requesting top up</p>}
            </div>
        </div>
    )
}

export default Player
