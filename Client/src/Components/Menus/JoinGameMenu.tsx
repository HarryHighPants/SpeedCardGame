import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import styled from 'styled-components'
import Popup from '../Popup'
import MenuButton from './MenuButton'

const JoinGameMenu = () => {
    const [gameId, setGameId] = useState('')
    const navigate = useNavigate()

    const onJoinGame = (gameId: string) => {
        // Set the game url param
        navigate(`/${gameId.toLowerCase()}`)
    }

    const onSubmit = (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault()
        onJoinGame(gameId)
    }

    return (
        <Popup key={'JoinGamePopup'} id={'JoinGamePopup'} onBackButton={() => navigate('/')}>
            <h2>Join Game</h2>
            <form onSubmit={(e) => onSubmit(e)}>
                <Instructions>Enter game code:</Instructions>
                <StyledInput placeholder={'Game1234'} onChange={(e) => setGameId(e.target.value)} />
                <MenuButton disabled={gameId.length < 3} type={'submit'}>
                    Join Game
                </MenuButton>
            </form>
        </Popup>
    )
}

const Instructions = styled.p`
    margin-bottom: 5px;
`

const StyledInput = styled.input`
    margin: 5px;
    height: 25px;
`

export default JoinGameMenu
