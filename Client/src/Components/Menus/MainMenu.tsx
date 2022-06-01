import { useNavigate } from 'react-router-dom'
import { uniqueNamesGenerator } from 'unique-names-generator'
import animals from '../../Assets/Animals.json'
import adjectives from '../../Assets/Adjectives.json'
import MenuHeader from './MenuHeader'
import Popup from '../Popup'
import styled from 'styled-components'
import MenuButton from './MenuButton'
import TutorialButton from '../TutorialButton'
import IconButton from '../IconButton'
import { AiFillGithub } from 'react-icons/ai'
import { githubUrl } from '../../Constants'
import MenuButtonGlow from './MenuButtonGlow'
import { useEffect, useState } from 'react'
import config from '../../Config'
import { v4 as uuid } from 'uuid'
import {IDailyResults} from "../../Interfaces/IDailyResults";

interface Props {}

const botDifficulties = ['Easy', 'Medium', 'Hard', 'Impossible']

const MainMenu = (props: Props) => {
    let navigate = useNavigate()
    const [persistentId, setPersistentId] = useState<string>(() => localStorage.getItem('persistentId') ?? uuid())
    const [dailyGameAvailable, setDailyGameAvailable] = useState(false)

    useEffect(() => {
        fetch(`${config.apiGateway.URL}/api/daily-stats/${persistentId}`)
            .then((response) => response.json())
            .then((data: IDailyResults) => setDailyGameAvailable(!data.completedToday))
            .catch((error) => setDailyGameAvailable(false))
    }, [])

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
                <TutorialButton onClick={() => navigate('/tutorial')} />
                <IconButton
                    icon={AiFillGithub}
                    style={{
                        position: 'absolute',
                        top: 0,
                        left: 0,
                    }}
                    onClick={() => window.open(githubUrl, '_blank')}
                />
                <MenuButtonGlow
                    defaultButton={!dailyGameAvailable}
                    key={`bot-daily`}
                    onClick={() => OnCreateGame(true, 4)}
                >
                    Verse Daily Challenger
                </MenuButtonGlow>

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
