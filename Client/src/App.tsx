import React, {useEffect, useState} from 'react'
import logo from './logo.svg'
import './App.css'
import * as signalR from '@microsoft/signalr'

function App() {

  // Sets a client message, sent from the server
  const [clientMessage, setClientMessage] = useState<string | null>(null);

  useEffect(() => {
    // Builds the SignalR connection, mapping it to /server
    const hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7067/server')
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build()

    hubConnection.start().then((a) => {
      // Once started, invokes the sendConnectionId in our ChatHub inside our ASP.NET Core application.
      if (hubConnection.connectionId) {
        hubConnection.invoke("sendConnectionId", hubConnection.connectionId);
      }
    })

    hubConnection.on("setClientMessage", message => {
      setClientMessage(message);
    });
  }, []);

    return (
        <div className="App">
          <div>client message: {clientMessage}</div>
          {/*<header className="App-header">*/}
          {/*      <img src={logo} className="App-logo" alt="logo" />*/}
          {/*      <p>*/}
          {/*          Edit <code>src/App.tsx</code> and save to reload.*/}
          {/*      </p>*/}
          {/*      <a className="App-link" href="https://reactjs.org" target="_blank" rel="noopener noreferrer">*/}
          {/*          Learn React*/}
          {/*      </a>*/}
          {/*  </header>*/}
        </div>
    )
}

export default App
