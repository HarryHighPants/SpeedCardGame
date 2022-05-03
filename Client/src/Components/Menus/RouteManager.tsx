import React, { useEffect, useState } from 'react'
import { BrowserRouter, Routes, Route, useLocation } from 'react-router-dom'
import MainMenu from './MainMenu'
import JoinGameMenu from './JoinGameMenu'
import Room from './Room'
import GameBoard from '../GameBoard'
import AutomatedGame from '../AutomatedGame'
import { useParams } from 'react-router-dom'
import { AnimatePresence } from 'framer-motion'
import Tutorial from "./Tutorial";

const RouteManager = () => {
	const [gameStarted, setGameStarted] = useState(false)
	let location = useLocation()

	useEffect(() => {
		if (location.pathname === '/' || location.pathname === "") {
			setGameStarted(false)
		}
	}, [location.pathname])

	return (
		<>
			<Routes>
				<Route path="/" element={<MainMenu />} />
				<Route path="/join" element={<JoinGameMenu />} />
				<Route path="/tutorial" element={<Tutorial />} />
				<Route path=":roomId" element={<Room onGameStarted={() => setGameStarted(true)} />} />
			</Routes>
			{!gameStarted && <AutomatedGame />}
		</>
	)
}

export default RouteManager
