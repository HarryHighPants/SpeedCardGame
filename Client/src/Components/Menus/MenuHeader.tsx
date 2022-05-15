import styled from 'styled-components'
import heaaderImg from '../../Assets/header.png'
import IconButton from '../IconButton'

const MenuHeader = () => {
    return (
        <div>
            <HeaderImg src={heaaderImg} alt={'Speed card game online logo'} />
        </div>
    )
}

const HeaderImg = styled.img`
    width: 100%;
    margin: -20px 0 20px 0;
    pointer-events: none;
`

export default MenuHeader
