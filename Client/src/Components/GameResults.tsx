import CelebrateShaker from './CelebrateShake'
import RankingStats from './RankingStats'

const GameResults = ({ persistentId, winnerName }: { persistentId: string; winnerName: string }) => {
    return (
        <>
            <h5
                style={{
                    marginTop: 40,
                    marginBottom: -10,
                }}
            >
                Winner is:
            </h5>
            <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'end', marginBottom: 50 }}>
                <CelebrateShaker />
                <h2 style={{ margin: '0 25px' }}>{winnerName}</h2>
                <CelebrateShaker startDelay={0.2} />
            </div>

            <RankingStats persistentId={persistentId} />
        </>
    )
}
export default GameResults
