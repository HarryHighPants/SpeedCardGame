import styled from 'styled-components'

interface Props extends React.ButtonHTMLAttributes<HTMLButtonElement> {
    children: React.ReactNode
}

const MenuButton = ({ children, ...props }: Props) => {
    return <MenuButtonComponent {...props}>{children}</MenuButtonComponent>
}

const MenuButtonComponent = styled.button`
    will-change: box-shadow, transform;
    outline: none;
    border: 0;
    border-radius: 4px;
    background: radial-gradient(100% 100% at 100% 0%, #545454 0%, #474747 100%);
    padding: 10px 15px;
    margin: 0 3px;
    color: #fff;
    text-shadow: -1px 3px 3px rgb(0 0 0 / 33%);
    transition: box-shadow 0.15s ease, transform 0.15s ease;

    &:hover {
        box-shadow: 0px 0.1em 0.2em rgb(28 28 28 / 40%), 0px 0.4em 0.7em -0.1em rgb(65 65 65 / 30%),
            inset 0px -0.1em 0px #2a2a2a;
        transform: translateY(-0.12em);
    }

    &:active {
        box-shadow: inset 0px 0.1em 0.6em #222222;
        transform: translateY(0em);
    }
`
export default MenuButton
