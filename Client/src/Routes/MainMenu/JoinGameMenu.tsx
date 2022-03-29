import {useState} from "react";

interface JoinGameMenuProps {
  onJoinGame: (gameId: string) => void
  onBack: () => void
}

const JoinGameMenu = ({ onJoinGame, onBack }: JoinGameMenuProps) => {
  const [gameId, setGameId] = useState('')

  const onSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault()
    onJoinGame(gameId)
  }

  return (
    <>
      <button onClick={(e) => onBack()}>Back</button>
      <form onSubmit={(e) => onSubmit(e)}>
        <input placeholder={'Game1234'} onChange={(e) => setGameId(e.target.value)} />
        <button disabled={gameId.length < 3} type={'submit'}>
          Join Game
        </button>
      </form>
    </>
  )
}

export default JoinGameMenu
