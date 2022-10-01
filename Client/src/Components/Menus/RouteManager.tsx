import React, { useEffect, useState } from 'react'
import { Routes, Route, useLocation } from 'react-router-dom'
import MainMenu from './MainMenu'
import JoinGameMenu from './JoinGameMenu'
import Room from './Room'
import AutomatedGame from '../AutomatedGame'
import Tutorial from './Tutorial'
import {Leaderboard} from "./Leaderboard";

const RouteManager = () => {
    const [gameStarted, setGameStarted] = useState(false)
    let location = useLocation()

    useEffect(() => {
        if (location.pathname === '/' || location.pathname === '' || location.pathname === '/leaderboard') {
            setGameStarted(false)
        }
    }, [location.pathname])

    return (
        <>
            <Routes>
                <Route path="/" element={<MainMenu />} />
                <Route path="/join" element={<JoinGameMenu />} />
                <Route path="/tutorial" element={<Tutorial />} />
                <Route path="/leaderboard" element={<Leaderboard />} />
                <Route path=":roomId" element={<Room onGameStarted={() => setGameStarted(true)} />} />
            </Routes>
            {!gameStarted && <AutomatedGame />}
        </>
    )
}

export default RouteManager
