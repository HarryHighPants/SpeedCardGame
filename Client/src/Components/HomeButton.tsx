import { HiOutlineHome } from 'react-icons/hi'
import { useNavigate } from 'react-router-dom'
import styled from 'styled-components'

const HomeButton = () => {
	const navigate = useNavigate()
	return <StyledHomeButton onClick={() => navigate('/')} />
}

const StyledHomeButton = styled(HiOutlineHome)`
	position: absolute;
	top: 0;
	left: 0;
	padding: 15px;
	width: 25px;
	height: 25px;
	color: #d5d5d5;
	cursor: pointer;

	&:hover {
		color: #7e7e7e;
	}
`
export default HomeButton
