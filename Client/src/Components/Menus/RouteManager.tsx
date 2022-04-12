import React, { useEffect, useState } from 'react'
import { BrowserRouter, Routes, Route, useLocation } from 'react-router-dom'
import MainMenu from './MainMenu'
import JoinGameMenu from './JoinGameMenu'
import Room from './Room'
import GameBoard from '../GameBoard'
import AutomatedGame from '../AutomatedGame'
import { useParams } from 'react-router-dom'

const RouteManager = () => {
	const [gameStarted, setGameStarted] = useState(false)
	let location = useLocation()

	useEffect(() => {
		if(location.pathname === "/"){
			setGameStarted(false)
		}
	}, [location])

	return (
		<>
			<Routes>
				<Route path="/" element={<MainMenu />} />
				<Route path="/join" element={<JoinGameMenu />} />
				<Route path=":roomId" element={<Room onGameStarted={() => setGameStarted(true)} />} />
			</Routes>
			{!gameStarted && <AutomatedGame />}
		</>
	)
}

export default RouteManager
