import React, { useEffect, useState } from 'react'
import './App.css'
import * as signalR from '@microsoft/signalr'
import MainMenu from "./Components/MainMenu";
import Lobby from "./Components/Lobby";

function App() {
    // Sets a client message, sent from the server
    const [clientMessage, setClientMessage] = useState<string | null>(null)
    const [hubConnection, setHubConnection] = useState<signalR.HubConnection>()

    useEffect(() => {
        // Builds the SignalR connection, mapping it to /server
      setHubConnection(new signalR.HubConnectionBuilder()
            .withUrl('https://localhost:7067/server')
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build())

        hubConnection?.start().then((a) => {
            // Once started, invokes the sendConnectionId in our ChatHub inside our ASP.NET Core application.
            if (hubConnection.connectionId) {
                hubConnection.invoke('sendConnectionId', hubConnection.connectionId)
            }
        })

        hubConnection?.on('setClientMessage', (message) => {
            setClientMessage(message)
        })
    }, [])

  const OnJoinGame = (gameId: string) => {
    // Set the game url param

    // Join the game on the server
    hubConnection?.invoke('JoinGame', gameId);
  }

  const OnCreateGame = () => {
    // Generate a new gameId
    // let gameId =
    // hubConnection?.invoke('JoinGame', gameId);
  }

    return (
        <div className="App">
          <div>client message: {clientMessage}</div>
          <MainMenu onJoinGame={OnJoinGame} onCreateGame={OnCreateGame}/>
          {/*<Lobby players={} roomId={} onStartGame={} onUpdateName={}/>*/}
        </div>
    )
}

export default App
