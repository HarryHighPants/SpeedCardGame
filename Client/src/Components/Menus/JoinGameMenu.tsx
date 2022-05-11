import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import styled from 'styled-components'
import MenuHeader from './MenuHeader'
import Popup from "../Popup";
import RankingStats from "../RankingStats";
import {IRankingStats} from "../../Interfaces/IRankingStats";

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
		<Popup key={"JoinGamePopup"} id={"JoinGamePopup"} onBackButton={() => navigate('/')}>
			<h2>Join Game</h2>
			<form onSubmit={(e) => onSubmit(e)}>
				<Instructions>Enter game code:</Instructions>
				<StyledInput placeholder={'Game1234'} onChange={(e) => setGameId(e.target.value)} />
				<StyledButton disabled={gameId.length < 3} type={'submit'}>
					Join Game
				</StyledButton>
			</form>
			<RankingStats stats={{oldRank:3,newRank:3,previousElo:1581,newElo:1714} as IRankingStats}/>
		</Popup>
	)
}

const Instructions = styled.p`
margin-bottom: 5px
`

const StyledInput = styled.input`
	margin: 5px;
	height: 25px;
`

const StyledButton = styled.button`
	height: 31px;
`


export default JoinGameMenu
