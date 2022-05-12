import { IRankingStats } from '../Interfaces/IRankingStats'
import { Rank, RankColour, RankTransitionColour } from '../Interfaces/ILobby'
import { percentageInRange } from '../Helpers/Utilities'
import { useSpring } from 'react-spring'
import { useState } from 'react'
import CustomSlider from "./CustomSlider";

interface Props {
    stats: IRankingStats
}

function RankingStats({ stats }: Props) {
    const [animatedElo, setAnimatedElo] = useState(stats.previousElo)

    useSpring({
        from: { number: stats.previousElo },
        to: { number: stats.newElo },
        onChange({ value }) {
            setAnimatedElo(parseInt(value.number))
        },
        config: { friction: 50, tension: 30, mass: 20 },
    })

    const getRank = (): Rank => Math.floor(animatedElo / 500)
    const minElo = (): number => animatedElo - (animatedElo % 500)
    const maxElo = (): number => animatedElo + (500 - (animatedElo % 500))
    const eloDiff = () => animatedElo - stats.previousElo

    const percentageRemaining = () => {
        let progressPercent = percentageInRange(animatedElo, minElo(), maxElo())
        return Math.abs(100 - progressPercent)
    }

    return (
        <div style={{ display: 'flex', flexDirection: 'column', margin: '50px 30px' }}>
            <h4 style={{marginBottom: 10}}>Your Rank Progress:</h4>
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <RankText rank={getRank()} value={minElo()} />
                <RankText rank={getRank() + 1} value={maxElo()} textAlignEnd />
            </div>
            <CustomSlider
                percentRemaining={percentageRemaining()}
                handleText={animatedElo.toString()}
                handleAdditionalText={(eloDiff() < 0 ? '-' : '+') + Math.abs(eloDiff()).toString()}
                additionalTextColour={eloDiff() < 0 ? '#fa3c3c' : '#71ea31'}
                backgroundColour={`linear-gradient(to right, ${RankColour[getRank()]}, ${
                    RankTransitionColour[getRank()]
                }, ${RankColour[getRank() + 1]})`}
            />
        </div>
    )
}

const RankText = ({ rank, value, textAlignEnd }: { rank: Rank; value: number; textAlignEnd?: boolean }) => (
    <div>
        <h5 style={{ color: RankColour[rank], margin: 0 }}>{Rank[rank]}</h5>
        <p style={{ fontSize: 15, margin: 0, textAlign: textAlignEnd ? 'end' : 'start' }}>{value}</p>
    </div>
)

export default RankingStats
