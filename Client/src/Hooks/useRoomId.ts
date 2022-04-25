import { useEffect } from 'react'
import useStateRef from 'react-usestateref'

// @ts-ignore
const useRoomId = (): [roomId: string, roomIdRef:  ReadOnlyRefObject<string>] => {
	const [roomId, setRoomId, roomIdRef] = useStateRef('')

	useEffect(() => {
		let newRoomId = window.location.pathname.replace('/', '')
		setRoomId(newRoomId)
	}, [window.location.pathname])

	return [roomId, roomIdRef]
}
export default useRoomId
