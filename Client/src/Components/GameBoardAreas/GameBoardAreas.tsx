import { IGameState } from '../../Interfaces/IGameState'
import BaseArea from './BaseArea'
import GameBoardLayout from '../../Helpers/GameBoardLayout'
import { CardLocationType, IPos, IRenderableCard } from '../../Interfaces/ICard'
import { AreaLocation, IRenderableArea } from '../../Interfaces/IBoardArea'
import { useEffect, useState } from 'react'

interface Props {
    gameState: IGameState
    ourId: string | null | undefined
    renderableAreas: IRenderableArea[]
}

const GameBoardAreas = ({ ourId, gameState, renderableAreas }: Props) => {
    const [ourKittyCount, setOurKittyCount] = useState(25)
    const [otherKittyCount, setOtherKittyCount] = useState(25)

    useEffect(() => {
        gameState.players.map((p) => {
            p.id === ourId ? setOurKittyCount(p.kittyCardsCount) : setOtherKittyCount(p.kittyCardsCount)
        })
    }, [gameState.players])

    return (
        <>
            {renderableAreas.map((area, i) => {
                area.text =
                    area.location.type === 'Kitty'
                        ? `Remaining: ${area.location.ourPlayer ? ourKittyCount : otherKittyCount}`
                        : ''
                return <BaseArea key={area.key} renderableArea={area} />
            })}
        </>
    )
}

export default GameBoardAreas
