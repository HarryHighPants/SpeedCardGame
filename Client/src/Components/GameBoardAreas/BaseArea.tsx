import styled from 'styled-components'
import { IPos } from '../../Interfaces/ICard'

export interface BaseAreaProps {
	dimensions: AreaDimensions
	text?: string | undefined
}

export interface AreaDimensions {
	pos: IPos
	size: IPos
}

const BaseArea = ({ dimensions, text }: BaseAreaProps) => {
	return <BaseAreaDiv padding={6} dimensions={dimensions} >
		<AreaText><b>{text}</b></AreaText>
	</BaseAreaDiv>
}

const BaseAreaDiv = styled.div<{ dimensions: AreaDimensions; padding: number }>`
	border: 3px solid white;
	border-radius: 10px;
	position: absolute;
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: flex-end;
	padding: ${(p) => p.padding}px;
	left: ${(p) => p.dimensions.pos.x - 9}px;
	top: ${(p) => p.dimensions.pos.y - 9}px;
	width: ${(p) => p.dimensions.size.x}px;
	height: ${(p) => p.dimensions.size.y}px;
`

const AreaText = styled.p<{ }>`
	z-index: 50;
	font-size: x-small;
	color: #fff4f0;
	margin: 0 0 -40px;
	height: 25px;
	text-transform: uppercase;
`


export default BaseArea