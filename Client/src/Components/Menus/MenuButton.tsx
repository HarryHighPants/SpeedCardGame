import styled from 'styled-components'

interface Props extends React.ButtonHTMLAttributes<HTMLButtonElement> {
    children: React.ReactNode
}

const MenuButton = ({ children, ...props }: Props) => {
    return <MenuButtonComponent {...props}>{children}</MenuButtonComponent>
}

const MenuButtonComponent = styled.button`
    background-color: #f2f1f1;
    padding: 5px 7px;
    border: 1px #555555 solid;
    border-radius: 3px;
    margin: 0 2px;

    &:hover {
        background-color: #e1e1e1;
    }

    &:active {
        background-color: #c5c5c5;
    }
`
export default MenuButton
