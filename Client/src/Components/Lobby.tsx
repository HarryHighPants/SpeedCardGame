import copyIcon from '../../public/Images/copyIcon.png'

interface Props {
    players: Player[]
    roomId: string
    onStartGame: () => {}
    onUpdateName: (newName: string) => {}
}

class Player {
    public name: string
    public currentPlayer: boolean = false

    constructor(name: string, currentPlayer: boolean) {
        this.name = name
        this.currentPlayer = currentPlayer
    }
}

const Lobby = ({ players, roomId, onStartGame, onUpdateName }: Props) => {
    return (
        <div>
            <h2>Lobby</h2>
            <div>
                <button>
                    {roomId}
                    <img src={copyIcon} alt="Logo" />
                </button>
                <div>
                    <h4>Players</h4>
                    <ul>{players.map((p) => LobbyPlayer(p, onUpdateName))}</ul>
                </div>
                <button disabled={players.length < 2} onClick={onStartGame}>
                    Start Game
                </button>
            </div>
        </div>
    )
}

export default Lobby

const LobbyPlayer = (player: Player, onUpdateName: (newName: string) => {}) => {
    return (
        <li>
            {player.currentPlayer ? (
                <input value={player.name} onChange={(e) => onUpdateName(e.target.value)} />
            ) : (
                player.name
            )}
        </li>
    )
}
