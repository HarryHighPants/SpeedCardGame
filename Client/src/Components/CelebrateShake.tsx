import { motion } from 'framer-motion'

interface ShakerProps {
	startDelay?: number
}
const CelebrateShaker = ({ startDelay }: ShakerProps ) => {
	return (
		<motion.h3
			style={{ transformOrigin: '0% 75%', marginBottom: 5, marginRight: -13 }}
			animate={{
				rotate: [-70, -20, -70],
				transition: { delay: startDelay, repeat: Infinity, duration: 0.75 },
			}}
		>
			🎉
		</motion.h3>
	)
}
export default CelebrateShaker
