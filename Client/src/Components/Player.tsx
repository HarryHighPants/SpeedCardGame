import { IPlayer } from '../Interfaces/IPlayer'
import { ICard, IPos } from '../Interfaces/ICard'
import Card from './Card'
import GameBoardLayout from '../Helpers/GameBoardLayout'
import styled from 'styled-components'
import backgroundImg from '../Assets/wood-tiling.jpg'
import { useEffect, useState } from 'react'
import { HubConnection, HubConnectionState } from '@microsoft/signalr'
import PlayerInfo, { IPlayerInfo } from './PlayerInfo'
import { AnimatePresence, AnimateSharedLayout } from 'framer-motion'
import { v4 as uuid } from 'uuid'

interface Props {
    player: IPlayer
    onRequestTopUp?: () => void
    onTop: boolean
    connection: HubConnection | undefined
    mustTopUp: boolean
}

const Player = ({ player, onRequestTopUp, onTop, connection, mustTopUp }: Props) => {
    const [additionalInfo, setAdditionalInfo] = useState<IPlayerInfo>()

    useEffect(() => {
        if (!!player.lastMove) {
            setAdditionalInfo({ id: uuid(), message: player.lastMove, messageType: 'Move' })
        }
    }, [player.lastMove])

    useEffect(() => {
        if (onTop) {
            return
        }
        if (connection?.state !== HubConnectionState.Connected) {
            return
        }
        connection.on('ErrorMessage', ReceivedMessaged)

        return () => {
            connection.off('ErrorMessage', ReceivedMessaged)
        }
    }, [connection, onTop])

    const ReceivedMessaged = (message: string) => {
        setAdditionalInfo({ id: uuid(), message: message, messageType: 'Error' })
    }

    return (
        <PlayerContainer style={{ backgroundImage: `url(${backgroundImg})` }}>
            <AdditionalInfo id={'player-info-' + player.idHash} key={'player-info-' + player.idHash} topOfBoard={onTop}>
                {!!additionalInfo && <PlayerInfo key={additionalInfo.id} playerInfo={additionalInfo} />}
            </AdditionalInfo>
            <PlayerName>{player.name}</PlayerName>
            {mustTopUp && !player.requestingTopUp && !!onRequestTopUp && (
                <RequestTopUpButton onClick={onRequestTopUp}>Request top up</RequestTopUpButton>
            )}
        </PlayerContainer>
    )
}

const PlayerContainer = styled.div`
    height: 200px;
    width: 100%;
    display: flex;
    justify-content: center;
    flex: 0;
    color: white;
    background-color: #853939;
    box-shadow: 0px 0px 50px 6px rgba(0, 0, 0, 0.59);
    font-family: Cinzel, 'serif';
    font-weight: 200;
    font-size: large;
    letter-spacing: 1px;
    user-select: none;
    z-index: 50;

    @media (max-height: 400px) {
        display: none;
    }
`

const AdditionalInfo = styled.div<{ topOfBoard: boolean }>`
    font-weight: 500;
    font-family: 'Roboto Slab', serif;
    position: absolute;
    transform: translateX(-50%);
    left: 50%;
    width: 100%;
    display: flex;
    align-items: ${(p) => (p.topOfBoard ? 'flex-start' : 'flex-end')};
    justify-content: center;
    height: 60px;
    margin: ${(p) => (p.topOfBoard ? '50' : '-60')}px 0 0 0;
    user-select: none;

    font-size: large;
    @media (max-width: 450px) {
        font-size: small;
    }
`

const RequestTopUpButton = styled.button`
    @keyframes wiggle {
        0% {
            transform: rotate(0deg);
        }
        80% {
            transform: rotate(0deg);
        }
        85% {
            transform: rotate(5deg);
        }
        95% {
            transform: rotate(-5deg);
        }
        100% {
            transform: rotate(0deg);
        }
    }

    animation: wiggle 2.5s infinite;
    position: absolute;
    top: 58%;
    height: 40px;
    width: 135px;
    z-index: 52;

  background-color: #f2f1f1;
  padding: 5px 7px;
  border: 1px #555555 solid;
  border-radius: 3px;
  margin: 0 2px;

  &:hover {
    background-color: #e1e1e1;
  }

  &:active {
    background-color: #c5c5c5;
  }
`

const PlayerName = styled.p`
    margin: 15px;
`

export default Player
