import { memo } from 'react'
import { HiOutlineHome } from 'react-icons/hi'
import { useNavigate } from 'react-router-dom'
import styled from 'styled-components'

interface Props {
	onClick?: () => void
}

const HomeButton = ({ onClick }: Props) => {
	const navigate = useNavigate()
	return (
		<StyledHomeButton
			onClick={() => {
				if (!!onClick) {
					onClick()
				}
				navigate('/')
			}}
		/>
	)
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
	z-index: 60;

	&:hover {
		color: #7e7e7e;
	}
`
export default memo(HomeButton)
