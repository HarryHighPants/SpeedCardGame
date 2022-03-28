import { useState } from 'react'

interface Props {
    onJoinGame: (gameId: string) => void
    onCreateGame: () => void
}

const MainMenu = ({ onJoinGame, onCreateGame }: Props) => {
    const [joiningGame, setJoiningGame] = useState(false)
    return (
        <div>
            {/* Todo: Update to header logo */}
            <h1>Speed Card Game</h1>
            <div>
                {joiningGame ? (
                    <JoinGameMenu onBack={() => setJoiningGame(false)} onJoinGame={onJoinGame} />
                ) : (
                    <>
                        <button onClick={(e) => setJoiningGame(true)}>Join Game</button>
                        <button onClick={onCreateGame}>Create Game</button>
                    </>
                )}
            </div>
        </div>
    )
}

export default MainMenu

interface JoinGameMenuProps {
    onJoinGame: (gameId: string) => void
    onBack: () => void
}

const JoinGameMenu = ({ onJoinGame, onBack }: JoinGameMenuProps) => {
    const [gameId, setGameId] = useState('')

    const onSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault()
        onJoinGame(gameId)
    }

    return (
        <>
            <button onClick={(e) => onBack()}>Back</button>
            <form onSubmit={(e) => onSubmit(e)}>
                <input placeholder={'Game1234'} onChange={(e) => setGameId(e.target.value)} />
                <button disabled={gameId.length < 3} type={'submit'}>Join Game</button>
            </form>
        </>
    )
}
