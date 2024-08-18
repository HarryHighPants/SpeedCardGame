import styled from 'styled-components'
import { HiOutlineChevronLeft } from 'react-icons/hi'
import HomeButton from './HomeButton'
interface Props {
    id: string
    children: JSX.Element | JSX.Element[]
    onBackButton?: () => void | undefined
    onHomeButton?: boolean
    customZIndex?: number
}

const Popup = ({ children, onBackButton, onHomeButton, id, customZIndex }: Props) => {
    const backPressed = () => {
        if (!!onBackButton) {
            onBackButton()
        }
    }

    return (
        <PopupContainer key={id} $customZIndex={customZIndex}>
            <PopupCenter>
                {!!onBackButton && <BackButton onClick={backPressed} />}
                {!!onHomeButton && <HomeButton />}
                {children}
            </PopupCenter>
        </PopupContainer>
    )
}

const PopupCenter = styled.div`
    position: absolute;
    background-color: #2e2e2e;
    border-radius: 12px;
    color: whitesmoke;
    min-height: 200px;
    max-width: 350px;
    box-shadow: rgba(0, 0, 0, 0.56) 0px 40px 200px;

    width: calc(100% - 140px);
    padding: 10px 50px 50px 50px;
    @media (max-width: 450px) {
        width: calc(100% - 70px);
        padding: 10px 20px 30px 20px;
    }
`

const PopupContainer = styled.div<{ $customZIndex?: number }>`
    position: absolute;
    top: 0;
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    flex-direction: column;
    justify-content: center;
    background: rgba(0, 0, 0, 0.2);
    z-index: ${(p) => p.$customZIndex ?? 60};
    backdrop-filter: blur(3px);
`

const BackButton = styled(HiOutlineChevronLeft)`
    position: absolute;
    top: 0;
    left: 0;
    padding: 20px;
    width: 25px;
    height: 25px;
    color: white;
    cursor: pointer;

    &:hover {
        color: #bebebe;
    }
`

export default Popup
