import React, { useEffect, useState } from 'react'
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
