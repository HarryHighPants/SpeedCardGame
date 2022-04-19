import { motion } from 'framer-motion'
import styled from 'styled-components'

export type IPlayerInfo = { id: string, message: string; messageType: 'Move' | 'Error' }

interface Props {
	playerInfo: IPlayerInfo
}

const PlayerInfo = ({ playerInfo }: Props) => {
	return (
		<StyledPlayerInfo
			messageType={playerInfo.messageType}
			transition={{ duration: 8, ease: 'easeOut' }}
			initial={{ opacity: 1 }}
			animate={{ opacity: 0 }}
		>
			{playerInfo.message}
		</StyledPlayerInfo>
	)
}

const StyledPlayerInfo = styled(motion.p)<{ messageType: 'Move' | 'Error' }>`
	position: absolute;
	color: ${(p) => (p.messageType === 'Move' ? '#fff' : '#ff2b2b')};
`

export default PlayerInfo
