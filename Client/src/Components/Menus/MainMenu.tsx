import { useState } from 'react'
import { Route, Routes, useNavigate } from 'react-router-dom'
import { uniqueNamesGenerator } from 'unique-names-generator'
import animals from '../../Assets/Animals.json'
import adjectives from '../../Assets/Adjectives.json'
import MenuHeader from './MenuHeader'
import Popup from '../Popup'
import styled from 'styled-components'
import MenuButton from './MenuButton'

interface Props {}

const botDifficulties = ['Easy', 'Medium', 'Hard', 'Impossible']

const MainMenu = (props: Props) => {
    let navigate = useNavigate()

    const OnCreateGame = (bot?: boolean, difficulty?: number) => {
        // Generate a new gameId
        const gameId = uniqueNamesGenerator({
            dictionaries: [adjectives, animals],
            length: 2,
            separator: '-',
        })

        // Set the game url param
        navigate(
            `/${gameId}${bot !== undefined ? '?type=bot' : ''}${bot !== undefined ? '&difficulty=' + difficulty : ''}`
        )
    }

    return (
        <Popup key={'mainMenuPopup'} id={'mainMenuPopup'}>
            <MenuHeader />
            <div>
                <MenuDescription>New to Speed?</MenuDescription>
                <MenuButton onClick={() => navigate('/tutorial')}>How to play</MenuButton>
                <MenuDescription>Daily Challenge</MenuDescription>
                <MenuButton key={`bot-daily`} onClick={() => OnCreateGame(true, 4)}>
                    Verse Daily Challenger
                </MenuButton>
                <MenuDescription>Play against a friend</MenuDescription>
                <MenuButton onClick={() => navigate('/join')}>Join Game</MenuButton>
                <MenuButton onClick={() => OnCreateGame()}>Create Game</MenuButton>
                <MenuDescription>Play against a bot</MenuDescription>
                <div>
                    {botDifficulties.map((bd, i) => (
                        <MenuButton key={`bot-${bd}`} onClick={() => OnCreateGame(true, i)}>
                            {bd}
                        </MenuButton>
                    ))}
                </div>
            </div>
        </Popup>
    )
}

const MenuDescription = styled.h4`
    margin-top: 50px;
    margin-bottom: 5px;
`

export default MainMenu
