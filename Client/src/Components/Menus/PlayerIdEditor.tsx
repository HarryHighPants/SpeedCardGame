import styled from 'styled-components'
import usePersistentState from '../../Hooks/usePersistentState'
import Popup from '../Popup'
import { v4 as uuid } from 'uuid'
import { HiOutlineDocumentDuplicate } from 'react-icons/hi'
import toast from 'react-hot-toast'

interface Props {
    onClose: () => void
}
const PlayerIdEditor = ({ onClose }: Props) => {
    const [playerId, setPlayerId] = usePersistentState('persistentId', uuid(), false)

    return (
        <Popup key={'PlayerIdEditor'} id={'PlayerIdEditor'} onBackButton={() => onClose()}>
            <div style={{ display: 'flex', flexDirection: 'column' }}>
                <h3>Player ID</h3>
                <p>This is your unqiue player identifier. This is used to save and load your progress.</p>
                <p>
                    To keep your progress synced between devices update each device so that they all share the same id.
                </p>
                <p style={{ marginBottom: 0 }}>Player ID:</p>
                <div style={{ display: 'flex', flexDirection: 'row', alignItems: 'center' }}>
                    <StyledInput value={playerId} onChange={(e) => setPlayerId(e.target.value)} style={{ flex: 1 }} />
                    <CopyButton
                        onClick={() => {
                            navigator.clipboard.writeText(playerId)
                            toast.success('Copied')
                        }}
                        style={{ height: '25px' }}
                    >
                        Copy
                    </CopyButton>
                </div>
                <a
                    target="_blank"
                    href="https://learn.microsoft.com/en-us/dotnet/api/system.guid.newguid?view=net-8.0"
                    rel="noreferrer"
                    style={{ fontSize: '0.8em', color: 'gray' }}
                >
                    Must be a valid GUID
                </a>
            </div>
        </Popup>
    )
}

const CopyButton = styled(HiOutlineDocumentDuplicate)`
    width: 25px;
    height: 25px;
    min-width: 25px;
    color: white;
    cursor: pointer;

    &:hover {
        color: #bebebe;
    }
`

const StyledInput = styled.input`
    margin: 5px;
    height: 25px;
`

export default PlayerIdEditor
