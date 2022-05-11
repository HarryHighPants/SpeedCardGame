import { IRankingStats } from '../Interfaces/IRankingStats'
import { Rank, RankColour } from '../Interfaces/ILobby'
import styled from 'styled-components'

interface Props {
    stats: IRankingStats
}

function RankingStats({ stats }: Props) {
    const minElo = (): number => stats.newElo - (stats.newElo % 500)
    const maxElo = (): number => stats.newElo + (500 - (stats.newElo % 500))

    const getCurrentColour = (newRank: Rank, newElo: number): string => {
     return "#ffffff"
        // lerpColor(RankColour[newRank], RankColour[newRank + 1])
    }

    return (
        <div style={{ display: 'flex', flexDirection: 'column' }}>
            {/*<h2 style={{marginBottom: -35}}>Rank</h2>*/}
            <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <RankText rank={stats.newRank} value={minElo()}/>
                <RankText rank={stats.newRank + 1} value={maxElo()}/>
            </div>
            <RankSlider
                $startColour={RankColour[stats.newRank]}
                $endColour={RankColour[stats.newRank + 1]}
                $thumbColour={getCurrentColour(stats.newRank, stats.newElo)}
                type="range"
                min={minElo()}
                max={maxElo()}
                value={stats.newElo}
            />
            <p>{stats.newElo}</p>
        </div>
    )
}

const RankSlider = styled.input<{ $startColour: string; $endColour: string; $thumbColour: string }>`
    -webkit-appearance: none;
    height: 10px;
    background: linear-gradient(to right, ${(p) => p.$startColour}, ${(p) => p.$endColour});
    border-radius: 20px;

    &::-webkit-slider-thumb {
        -webkit-appearance: none;
        width: 7px;
        height: 25px;
        background: ${(p) => p.$thumbColour};
        cursor: pointer;
        border-radius: 30px;
    }
`

const RankText = ({ rank, value }: { rank: Rank, value: number }) => <div>
    <h5 style={{ color: RankColour[rank] }}>{Rank[rank]}</h5>
    <p style={{fontSize: 15}}>{value}</p>
</div>

export default RankingStats
