import { useState } from 'react'
import { Route, Routes, useNavigate } from 'react-router-dom'
import { uniqueNamesGenerator } from 'unique-names-generator'
import animals from '../../Assets/Animals.json'
import adjectives from '../../Assets/Adjectives.json'
import MenuHeader from '../../Components/MenuHeader'

interface Props {}

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
		navigate(`/${gameId}${bot&&"?type=bot"}${bot&&"&difficulty="+difficulty}`)
	}

	return (
		<div>
			<MenuHeader />
			<div>
				<h4>Play against a friend</h4>
				<button onClick={() => navigate('/join')}>Join Game</button>
				<button onClick={() => OnCreateGame()}>Create Game</button>
				<h4>Play against a bot</h4>
				<div>
					<button onClick={() => OnCreateGame(true, 0)}>Easy</button>
					<button onClick={() => OnCreateGame(true, 1)}>Medium</button>
					<button onClick={() => OnCreateGame(true, 2)}>Hard</button>
					<button onClick={() => OnCreateGame(true, 3)}>Impossible</button>
				</div>
			</div>
		</div>
	)
}

export default MainMenu
