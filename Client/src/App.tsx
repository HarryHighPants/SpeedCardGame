import React from 'react'
import { BrowserRouter } from 'react-router-dom'
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
