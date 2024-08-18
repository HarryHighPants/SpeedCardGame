import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import styled from 'styled-components'
import Popup from '../Popup'
import winImg from '../../Assets/tutorial/tutorial-win.png'
import playImg from '../../Assets/tutorial/tutorial-play.png'
import pickupImg from '../../Assets/tutorial/tutorial-pickup.png'
import topupImg from '../../Assets/tutorial/tutorial-topup.png'
import { motion, useMotionValue, useTransform } from 'framer-motion'
import MenuButton from './MenuButton'
import PlayerIdEditor from './PlayerIdEditor'

interface ITutorialSlide {
    title: string
    description: string
    imageSrc: string
}

const tutorialSlides = [
    {
        title: 'Objective',
        description:
            'The goal of this game is to play all of your cards before your opponent.' +
            '<br/>There is no turns in this game so move as quickly as you can',
        imageSrc: winImg,
    },
    {
        title: 'Playing cards',
        description:
            "You can play cards by dragging them onto a center pile if it's value is 1 above or below." +
            '<br/>For example a 6 can be played onto a 5 or 7.' +
            '<br/>Also, an ace can be played onto a 2 or vice versa.',
        imageSrc: playImg,
    },
    {
        title: 'Picking up',
        description:
            'You can pickup cards from your kitty if you have less than 5 cards in your hand.' +
            '<br/>To do this, drag a card from your kitty into your hand.',
        imageSrc: pickupImg,
    },
    {
        title: 'Topping up',
        description:
            "If you can't make any moves, a request top up button will appear." +
            '<br/>When both players have requested top up new cards will be added to the center pile.',
        imageSrc: topupImg,
    },
] as ITutorialSlide[]

const Tutorial = () => {
    const [slideIndex, setSlideIndex] = useState(0)
    const [previousDisabled, setPreviousDisabled] = useState(true)
    const [nextDisabled, setNextDisabled] = useState(false)
    const [showIdModal, setEditIdModal] = useState(false)
    let navigate = useNavigate()
    const offset = useMotionValue(0)
    const scale = useTransform(offset, [-50, 0, 50], [0.8, 1, 0.8])
    const opacity = useTransform(offset, [-50, 0, 50], [0.7, 1, 0.7])

    useEffect(() => {
        setPreviousDisabled(slideIndex === 0)
        setNextDisabled(slideIndex === tutorialSlides.length - 1)
    }, [slideIndex])

    const onDragEnd = () => {
        if (offset.get() < 49 && !nextDisabled) {
            setSlideIndex((s) => s + 1)
        }
        if (offset.get() > -49 && !previousDisabled) {
            setSlideIndex((s) => s - 1)
        }
        offset.set(0)
    }

    return (
        <>
            <Popup key={'JoinGamePopup'} id={'JoinGamePopup'} onBackButton={() => navigate('/')}>
                <h3>Tutorial</h3>
                <p>1.0.2</p>
                <MenuButton onClick={() => setEditIdModal(true)}>Player ID settings</MenuButton>
                <SlideContainer
                    drag={'x'}
                    dragConstraints={{ left: -50, right: 50 }}
                    dragElastic={0.1}
                    dragSnapToOrigin
                    onDrag={(e, info) => offset.set(info.offset.x)}
                    onDragEnd={() => onDragEnd()}
                    style={{ scale, opacity, cursor: 'pointer' }}
                    animate={{ transition: { type: 'ease' } }}
                >
                    <h2>
                        {slideIndex + 1}. {tutorialSlides[slideIndex].title}
                    </h2>
                    <img
                        style={{ width: '100%', pointerEvents: 'none' }}
                        src={tutorialSlides[slideIndex].imageSrc}
                        alt={tutorialSlides[slideIndex].title}
                    />
                    <div>
                        {tutorialSlides[slideIndex].description.split('<br/>').map((d) => (
                            <p key={d}>{d}</p>
                        ))}
                    </div>
                </SlideContainer>
                <div>
                    <NavigateButton
                        style={{ cursor: previousDisabled ? 'default' : 'pointer' }}
                        disabled={previousDisabled}
                        onClick={() => setSlideIndex((s) => s - 1)}
                    >
                        Back
                    </NavigateButton>
                    <NavigateButton
                        style={{ cursor: nextDisabled ? 'default' : 'pointer' }}
                        disabled={nextDisabled}
                        onClick={() => setSlideIndex((s) => s + 1)}
                    >
                        Next
                    </NavigateButton>
                </div>
            </Popup>
            {showIdModal && <PlayerIdEditor onClose={() => setEditIdModal(false)} />}
        </>
    )
}

const NavigateButton = styled.button`
    margin: 0 10px;
    padding: 10px;
    width: 70px;
`

const SlideContainer = styled(motion.div)`
    min-height: 500px;
    margin-top: 40px;
    margin-bottom: 50px;
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    text-align: left;
`

export default Tutorial
