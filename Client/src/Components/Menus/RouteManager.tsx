import React, { useEffect, useState } from 'react'
import { BrowserRouter, Routes, Route } from 'react-router-dom'
import MainMenu from './MainMenu'
import JoinGameMenu from './JoinGameMenu'
import Room from './Room'
import GameBoard from "../GameBoard";
import AutomatedGame from "../AutomatedGame";

const RouteManager = () => {
	const [gameStarted, setGameStarted] = useState(false)

	return (
		<div className="App">
			<BrowserRouter>
				<Routes>
					<Route path="/" element={<MainMenu />} />
					<Route path="/join" element={<JoinGameMenu />} />
					<Route path=":roomId" element={<Room onGameStarted={()=>setGameStarted(true)} />} />
				</Routes>
			</BrowserRouter>
			{!gameStarted && <AutomatedGame />}
		</div>
	)
}

export default RouteManager
