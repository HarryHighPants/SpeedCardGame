import { memo } from 'react'
import { IconType } from 'react-icons'
import styled from 'styled-components'

interface Props extends React.ButtonHTMLAttributes<HTMLButtonElement> {
    icon: IconType
}

const IconButton = ({ icon, ...props }: Props) => {
    return (
        <StyledButton {...props}>
            <StyledIcon as={icon}/>
        </StyledButton>
    )
}

const StyledButton = styled.button`
    background: none;
    border: none;
`

const StyledIcon = styled.div`
  padding: 15px;
  width: 25px;
  height: 25px;
  color: #d5d5d5;
  background-color: #f0f8ff00;

  &:hover {
    color: #7e7e7e;
  }
`
export default memo(IconButton)
