import React, { useEffect, useState } from 'react'
import './App.css'
import { BrowserRouter, Routes, Route } from 'react-router-dom'
import MainMenu from './Routes/MainMenu/MainMenu'
import Game from './Routes/Game/Game'
import JoinGameMenu from './Routes/JoinMenu/JoinGameMenu'

function App() {
    return (
        <div className="App">
            <BrowserRouter>
                <Routes>
                    <Route path="/" element={<MainMenu />} />
                    <Route path="/join" element={<JoinGameMenu />} />
                    <Route path=":gameId" element={<Game />} />
                </Routes>
            </BrowserRouter>
        </div>
    )
}

export default App
