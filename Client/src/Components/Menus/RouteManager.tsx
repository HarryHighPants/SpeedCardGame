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
	const [roomId, setRoomId] = useState('')
	let location = useLocation()

	useEffect(() => {
		if (location.pathname === '/' || location.pathname === "") {
			setGameStarted(false)
		}
	}, [location.pathname])

	return (
		<>
			<AnimatePresence>
				{location.pathname === '/' && <MainMenu key={'main'} />}
				{location.pathname === '/join' && <JoinGameMenu key={'JoinGameMenu'} />}
				{location.pathname === '/tutorial' && <Tutorial key={'Tutorial'} />}
				{location.pathname !== '/join' && location.pathname !== '/' && location.pathname !== '/tutorial' && (
					<Room onGameStarted={() => setGameStarted(true)} />
				)}
			</AnimatePresence>
			{!gameStarted && <AutomatedGame />}
		</>
	)
}

export default RouteManager
