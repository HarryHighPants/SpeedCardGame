// import copyIcon from '../../public/Images/copyIcon.png'
import * as signalR from '@microsoft/signalr'
import { useEffect, useState } from 'react'

interface Props {
    connection: signalR.HubConnection | undefined
    gameId: string | undefined
}

interface LobbyData {
    players: Player[]
}

interface Player {
    id: string
    name: string | undefined
    currentPlayer: boolean
}

const Lobby = ({ connection, gameId }: Props) => {
    const [lobbyData, setLobbyData] = useState<LobbyData>()
    const [myPlayerId, setMyPlayerId] = useState<string>()

    useEffect(() => {
        connection?.on('LobbyData', UpdateLobbyData)
    }, [])

    const UpdateLobbyData = (data: LobbyData) => {
        setLobbyData(data)
    }

    const onStartGame = () => {
        connection?.invoke('StartGame')
    }

    const UpdateName = (newName: string) => {
        // Update our name on the server
        connection?.invoke('UpdateName', newName)

        // Locally update our own player name
        let myPlayer = lobbyData?.players.find((p) => p.id == myPlayerId)
        if (myPlayer != null) {
            myPlayer.name = newName
        }
    }

    return (
        <div>
            <h2>Lobby</h2>
            <div>
                <button>
                    {gameId}
                    {/*<img src={copyIcon} alt="Logo" />*/}
                </button>

                <div>
                    <h4>Players</h4>
                    {lobbyData != null ? (
                        <ul>{lobbyData?.players.map((p) => LobbyPlayer(p, UpdateName))}</ul>
                    ) : (
                        <div>Loading</div>
                    )}
                </div>
                <button disabled={lobbyData == null || lobbyData.players.length < 2} onClick={onStartGame}>
                    Start Game
                </button>
            </div>
        </div>
    )
}

export default Lobby

const LobbyPlayer = (player: Player, onUpdateName: (newName: string) => void) => {
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
