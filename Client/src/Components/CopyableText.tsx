import { HiOutlineDocumentDuplicate } from 'react-icons/hi'
import styled from 'styled-components'
import toast from 'react-hot-toast'

interface Props {
	displayText: string | undefined
	copyText: string | undefined
	messageText: string | undefined
}

function CopyableText({ displayText, copyText, messageText }: Props) {
	return (
		<InputWrapper>
			<input style={{minWidth: 0}} value={displayText} disabled={true} />
			<CopyButton
				onClick={() => {
					if (copyText) {
						navigator.clipboard.writeText(copyText)
						toast.success(messageText ?? 'Copied')
					}
				}}
			/>
		</InputWrapper>
	)
}

const InputWrapper = styled.div`
	display: flex;
	justify-content: flex-start;
	width: 100%;
`

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

export default CopyableText
