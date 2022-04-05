import copyIcon from '../Assets/copyIcon.png'
import * as signalR from '@microsoft/signalr'
import { useEffect, useState } from 'react'

interface Props {
    connection: signalR.HubConnection | undefined
    roomId: string | undefined
}

interface LobbyData {
    connections: Player[]
}

interface Player {
    connectionId: string
    name: string
}

const Lobby = ({ connection, roomId }: Props) => {
    const [lobbyData, setLobbyData] = useState<LobbyData>()
    const [myPlayerName, setMyPlayerName] = useState<string>('Player')
    const [connectionId, setConnectionId] = useState<string>('')

    useEffect(() => {
        if (!connection) return
        connection.on('UpdateLobbyState', UpdateLobbyData)

        if (connection.connectionId) {
            setConnectionId(connection.connectionId)
        }
        return () => {
            connection.off('UpdateLobbyState', UpdateLobbyData)
        }
    }, [connection])

    const UpdateLobbyData = (data: any) => {
        let parsedData: LobbyData = JSON.parse(data)
        setLobbyData(parsedData)
        let myPlayerInfo = parsedData.connections.find((c) => c.connectionId === connectionId)
        if (myPlayerInfo) {
            setMyPlayerName(myPlayerInfo.name)
        }
    }

    const onStartGame = () => {
        connection?.invoke('StartGame')
    }

    const UpdateName = (newName: string) => {
        // Update our name on the server
        connection?.invoke('UpdateName', newName)

        // Locally set our name
        setMyPlayerName(newName)
    }

    return (
        <div>
            <h2>Lobby</h2>
            <div>
                <div>
                    <p>Invite link:</p>
                    <input value={window.location.href} disabled={true} />
                    <button onClick={() => navigator.clipboard.writeText(window.location.href)}>
                        <img width={10} alt="Copy" src={copyIcon} />
                    </button>
                </div>
                <div>
                    <h4>Players</h4>
                    {lobbyData != null ? (
                        <ul>
                            {lobbyData?.connections?.map((p) => LobbyPlayer(connectionId, myPlayerName, p, UpdateName))}
                        </ul>
                    ) : (
                        <div>Loading</div>
                    )}
                </div>
                <button disabled={lobbyData == null || lobbyData.connections?.length < 2} onClick={onStartGame}>
                    Start Game
                </button>
            </div>
        </div>
    )
}

export default Lobby

const LobbyPlayer = (
    connectionId: string,
    myPlayerName: string,
    player: Player,
    onUpdateName: (newName: string) => void
) => {
    return (
        <li key={player.connectionId}>
            {player.connectionId == connectionId ? (
                <input maxLength={20} value={myPlayerName} onChange={(e) => onUpdateName(e.target.value)} />
            ) : (
                player.name
            )}
        </li>
    )
}
