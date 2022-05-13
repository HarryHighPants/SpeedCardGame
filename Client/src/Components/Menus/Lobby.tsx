import * as signalR from '@microsoft/signalr'
import { useEffect, useState } from 'react'
import { useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { GameType, ILobby, IPlayerConnection } from '../../Interfaces/ILobby'
import styled from 'styled-components'
import { HiOutlineDocumentDuplicate } from 'react-icons/hi'
import Popup from '../Popup'
import { IGameState } from '../../Interfaces/IGameState'
import { AnimatePresence, motion } from 'framer-motion'
import LobbyPlayer from './LobbyPlayer'
import CopyableText from '../CopyableText'
import { IDailyResults } from '../../Interfaces/IDailyResults'

interface Props {
    connection: signalR.HubConnection | undefined
    playerId: string | undefined
    roomId: string | undefined
    gameState: IGameState | undefined
    onBack: () => void
}

const Lobby = ({ connection, playerId, roomId, gameState, onBack }: Props) => {
    let navigate = useNavigate()
    const [lobbyData, setLobbyData] = useState<ILobby>()
    const [dailyResults, setDailyResults] = useState<IDailyResults>()
    const [inLobby, setInLobby] = useState<boolean>(false)
    const [myPlayerName, setMyPlayerName] = useState<string>(() =>
        JSON.parse(localStorage.getItem('playerName') ?? `"Player"`)
    )
    const [searchParams, setSearchParams] = useSearchParams()
    const [waitingForPlayers, setWaitingForPlayers] = useState(true)
    const [activePlayers, setActivePlayers] = useState<string[]>([])
    const [spectating, setSpectating] = useState<boolean>(false)

    useEffect(() => {
        if (!connection) return
        console.log("connection.on('UpdateLobbyState', UpdateLobbyData)")
        connection.on('UpdateLobbyState', UpdateLobbyData)
        connection.on('UpdateDailyResults', UpdateDailyResults)

        return () => {
            connection.off('UpdateLobbyState', UpdateLobbyData)
            connection.off('UpdateDailyResults', UpdateDailyResults)
        }
    }, [connection])

    useEffect(() => {
        if (!inLobby) return
        console.log('updating name', myPlayerName)
        // Update our name on the server
        connection?.invoke('UpdateName', roomId, myPlayerName)
    }, [myPlayerName, inLobby])

    useEffect(() => {
        let activePlayers = (gameState?.players.filter((p) => p.idHash !== '0') ?? []).map((p) => p.idHash)
        setActivePlayers(activePlayers)
        setSpectating(activePlayers.length >= 2 && !activePlayers.find((p) => p === playerId))
    }, [gameState])

    const UpdateLobbyData = (updatedLobby: ILobby) => {
        setLobbyData(updatedLobby)
        setInLobby(updatedLobby !== null)
    }

    const UpdateDailyResults = (data: any) => {
        let dailyResultsData: IDailyResults = JSON.parse(data)
        setDailyResults(dailyResultsData)
    }

    useEffect(() => {
        setWaitingForPlayers(lobbyData == null || lobbyData.connections?.length < 2)
    }, [lobbyData])

    const onStartGame = () => {
        connection?.invoke('StartGame', roomId)
    }

    const UpdateName = (newName: string) => {
        // Save it to local storage
        localStorage.setItem('playerName', JSON.stringify(newName))

        // Locally set our name
        setMyPlayerName(newName)
    }

    const ShowLobby = () => {
        return !gameState || !connection || activePlayers.length < 2 || spectating || !lobbyData?.gameStarted
    }

    if (!ShowLobby()) {
        return <></>
    }
    return (
        <Popup
            id={'lobbyPopup'}
            onBackButton={() => {
                onBack()
                navigate(`/`)
            }}
        >
            <LobbyWrapper>
                <Header2>Lobby</Header2>
                <Group>
                    <GameCodeTitle>Game Code:</GameCodeTitle>
                    <CopyableText
                        displayText={roomId}
                        copyText={window.location.href}
                        messageText={'Copied url to clipboard'}
                    />
                </Group>
                <Group>
                    <PlayersTitle>Players:</PlayersTitle>
                    <PlayersContainer>
                        {lobbyData != null && !!playerId ? (
                            <>
                                {lobbyData?.connections?.map((p, i) =>
                                    LobbyPlayer(playerId, myPlayerName, p, UpdateName, i)
                                )}
                            </>
                        ) : (
                            <div>Connecting to room..</div>
                        )}
                    </PlayersContainer>
                </Group>
                {waitingForPlayers && !!lobbyData && (
                    <p style={{ marginTop: -10, height: 0 }}>Waiting for another player to join..</p>
                )}
                {spectating ? (
                    <p>Game in progress</p>
                ) : (
                    lobbyData != null &&
                    !!playerId &&
                    !waitingForPlayers && (
                        <StartButton disabled={waitingForPlayers} onClick={() => onStartGame()}>
                            Start Game
                        </StartButton>
                    )
                )}
            </LobbyWrapper>
        </Popup>
    )
}

export default Lobby

const LobbyWrapper = styled.div`
    display: flex;
    flex-direction: column;
    align-items: center;
`

const StartButton = styled.button`
    margin-top: 20px;
    --green: #1bfd9c;
    font-size: 15px;
    padding: 0.7em 2.7em;
    letter-spacing: 0.06em;
    position: relative;
    font-family: inherit;
    border-radius: 0.6em;
    overflow: hidden;
    transition: all 0.3s;
    line-height: 1.4em;
    border: 2px solid var(--green);
    background: linear-gradient(
        to right,
        rgba(27, 253, 156, 0.1) 1%,
        transparent 40%,
        transparent 60%,
        rgba(27, 253, 156, 0.1) 100%
    );
    color: var(--green);
    box-shadow: inset 0 0 10px rgba(27, 253, 156, 0.4), 0 0 9px 3px rgba(27, 253, 156, 0.1);

    &:hover {
        color: #82ffc9;
        box-shadow: inset 0 0 10px rgba(27, 253, 156, 0.6), 0 0 9px 3px rgba(27, 253, 156, 0.2);
    }

    &:before {
        content: '';
        position: absolute;
        left: -4em;
        width: 4em;
        height: 100%;
        top: 0;
        transition: transform 0.4s ease-in-out;
        pointer-events: none;
        background: linear-gradient(
            to right,
            transparent 1%,
            rgba(27, 253, 156, 0.1) 40%,
            rgba(27, 253, 156, 0.1) 60%,
            transparent 100%
        );
    }

    &:hover:before {
        transform: translateX(15em);
    }

    &:active {
        transform: scale(0.85);
    }
`

const Group = styled.div`
    width: 175px;
    display: flex;
    flex-direction: column;
    margin-bottom: 30px;
    align-items: flex-start;
`

const PlayersTitle = styled.p`
    margin-bottom: 8px;
`
const PlayersContainer = styled.div`
    justify-content: flex-start;
    display: flex;
    flex-direction: column;
    gap: 10px;
    width: 100%;
    min-height: 65px;
`

const GameCodeTitle = styled.p`
    margin-bottom: 5px;
`

const Header2 = styled.h2`
    width: 250px;
    margin-bottom: 0px;
`
