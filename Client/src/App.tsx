import React, { useEffect, useState } from 'react'
import './App.css'
import { BrowserRouter, Routes, Route } from 'react-router-dom'
import RouteManager from "./Components/Menus/RouteManager";

function App() {
    return (
        <div className="App">
			<BrowserRouter>
				<RouteManager/>
			</BrowserRouter>
        </div>
    )
}

export default App
