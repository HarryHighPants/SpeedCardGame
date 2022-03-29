import { useState } from 'react'
import JoinGameMenu from './JoinGameMenu'
import { useNavigate } from 'react-router-dom'

interface Props {}

const MainMenu = (props: Props) => {
    const [joiningGame, setJoiningGame] = useState(false)
    let navigate = useNavigate()
    // const namor = require('namor')

    const OnJoinGame = (gameId: string) => {
        // Set the game url param
        navigate('/game/' + gameId)
    }

    const OnCreateGame = () => {
        // Generate a new gameId
        // let gameId = namor.generate({ words: 2, saltLength: 0 })
        let gameId = 'test'

        // Join the game
        OnJoinGame(gameId)
    }

    return (
        <div>
            {/* Todo: Update to header logo */}
            <h1>Speed Card Game</h1>
            <div>
                {joiningGame ? (
                    <JoinGameMenu onBack={() => setJoiningGame(false)} onJoinGame={OnJoinGame} />
                ) : (
                    <>
                        <button onClick={(e) => setJoiningGame(true)}>Join Game</button>
                        <button onClick={() => OnCreateGame()}>Create Game</button>
                    </>
                )}
            </div>
        </div>
    )
}

export default MainMenu
