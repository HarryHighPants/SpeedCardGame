import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import MenuHeader from "./MenuHeader";

const JoinGameMenu = () => {
    const [gameId, setGameId] = useState('')
    const navigate = useNavigate()

    const onJoinGame = (gameId: string) => {
        // Set the game url param
        navigate(`/${gameId}`)
    }

    const onSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault()
        onJoinGame(gameId)
    }

    return (
        <div>
          <MenuHeader/>
          <button onClick={(e) => navigate('/')}>Back</button>
            <form onSubmit={(e) => onSubmit(e)}>
                <input placeholder={'Game1234'} onChange={(e) => setGameId(e.target.value)} />
                <button disabled={gameId.length < 3} type={'submit'}>
                    Join Game
                </button>
            </form>
        </div>
    )
}

export default JoinGameMenu
