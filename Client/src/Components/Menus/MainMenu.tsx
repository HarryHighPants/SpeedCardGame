import { useState } from 'react'
import { Route, Routes, useNavigate } from 'react-router-dom'
import { uniqueNamesGenerator } from 'unique-names-generator'
import animals from '../../Assets/Animals.json'
import adjectives from '../../Assets/Adjectives.json'
import MenuHeader from './MenuHeader'
import Popup from '../Popup'
import styled from 'styled-components'

interface Props {}

const botDifficulties = ['Easy', 'Medium', 'Hard', 'Impossible']

const MainMenu = (props: Props) => {
	let navigate = useNavigate()

	const OnCreateGame = (bot?: boolean, difficulty?: number) => {
		// Generate a new gameId
		const gameId = uniqueNamesGenerator({
			dictionaries: [adjectives, animals],
			length: 2,
			separator: '-',
		})

		// Set the game url param
		navigate(
			`/${gameId}${bot !== undefined ? '?type=bot' : ''}${bot !== undefined ? '&difficulty=' + difficulty : ''}`
		)
	}

	return (
		<Popup key={"mainMenuPopup"} id={"mainMenuPopup"}>
			<MenuHeader />
			<div>
				<MenuDescription>New to Speed?</MenuDescription>
				<button onClick={() => navigate('/tutorial')}>How to play</button>
				<MenuDescription>Daily Challenge</MenuDescription>
				<button key={`bot-daily`} onClick={() => OnCreateGame(true, 4)}>Verse Daily Challenger</button>
				<MenuDescription>Play against a friend</MenuDescription>
				<button onClick={() => navigate('/join')}>Join Game</button>
				<button onClick={() => OnCreateGame()}>Create Game</button>
				<MenuDescription>Play against a bot</MenuDescription>
				<div>
					{botDifficulties.map((bd, i) => (
						<button key={`bot-${bd}`} onClick={() => OnCreateGame(true, i)}>{bd}</button>
					))}
				</div>
			</div>
		</Popup>
	)
}

const MenuDescription = styled.h4`
	margin-top: 50px;
	margin-bottom: 5px;
`

export default MainMenu
