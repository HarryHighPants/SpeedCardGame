import { forwardRef, useCallback, useEffect, useState } from 'react'
import styled from 'styled-components'
import { CardLocationType, IPos } from '../../Interfaces/ICard'
import { AreaDimensions, IRenderableArea } from '../../Interfaces/IBoardArea'

interface Props {
	renderableArea: IRenderableArea
}

const BaseArea = ({ renderableArea }: Props) => {
	const [, updateState] = useState({})
	const forceUpdate = useCallback(() => updateState({}), [])

	useEffect(() => {
		renderableArea.forceUpdate = forceUpdate
	}, [])

	return (
		<BaseAreaDiv
			highlight={renderableArea.highlight}
			ref={renderableArea.ref}
			padding={6}
			dimensions={renderableArea.dimensions}
		>
			<AreaText>
				<b>{renderableArea.text}</b>
			</AreaText>
		</BaseAreaDiv>
	)
}

const BaseAreaDiv = styled.div<{ highlight: boolean | undefined; dimensions: AreaDimensions; padding: number }>`
	${(p) => (p.highlight ? 'background-color: #00000047;' : '')}
	border: 3px solid white;
	border-radius: 10px;
	position: absolute;
	display: flex;
	flex-direction: column;
	align-items: center;
	justify-content: flex-end;
	padding: ${(p) => p.padding}px;
	left: ${(p) => p.dimensions.pos.X - 9}px;
	top: ${(p) => p.dimensions.pos.Y - 9}px;
	width: ${(p) => p.dimensions.size.X}px;
	height: ${(p) => p.dimensions.size.Y}px;
`

const AreaText = styled.p<{}>`
	z-index: 50;
	font-size: x-small;
	font-weight: bolder;
	color: #fff4f0;
	margin: 0 0 -40px;
	height: 25px;
	text-transform: uppercase;
`

export default BaseArea
