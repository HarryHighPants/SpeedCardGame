import { useState } from 'react'
import { Route, Routes, useNavigate } from 'react-router-dom'
import { uniqueNamesGenerator } from 'unique-names-generator'
import animals from '../../Assets/Animals.json'
import adjectives from '../../Assets/Adjectives.json'
import MenuHeader from "../../Components/MenuHeader";

interface Props {}

const MainMenu = (props: Props) => {
    let navigate = useNavigate()

    const OnCreateGame = () => {
        // Generate a new gameId
        const gameId = uniqueNamesGenerator({
            dictionaries: [adjectives, animals],
            length: 2,
            separator: '-',
        })

      // Set the game url param
      navigate(`/${gameId}`)
    }

    return (
        <div>
            <MenuHeader/>
            <div>
                <button onClick={() => navigate('/join')}>Join Game</button>
                <button onClick={() => OnCreateGame()}>Create Game</button>
            </div>
        </div>
    )
}

export default MainMenu
