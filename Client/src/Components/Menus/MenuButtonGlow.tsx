import styled from 'styled-components'

interface Props extends React.ButtonHTMLAttributes<HTMLButtonElement> {
    children: React.ReactNode
    defaultButton: boolean
}

const MenuButtonGlow = ({ defaultButton, children, ...props }: Props) => {
    return (
        <MenuButtonGlowComponent defaultButton={defaultButton} {...props}>
            {children}
        </MenuButtonGlowComponent>
    )
}

const MenuButtonGlowComponent = styled.button<{ defaultButton: boolean }>`
    --main-colour: ${(p) => (p.defaultButton ? '#e8e8e8' : '#ebb763')};
    --glow-colour: ${(p) => (p.defaultButton ? 'rgba(232,232,232,0)' : '#ebb763')};
  color: ${(p) => (p.defaultButton ? '#0E0E0E' : '#272727')};
  font-weight: ${(p) => (p.defaultButton ? '' : 'bolder')};

  padding: 10px 15px;
    border: 2px #232323 solid;
    border-radius: 3px;
    margin: 0 2px;
    -webkit-transition: 0.3s;
    transition: 0.3s;
    background-color: var(--main-colour);

    &:hover {
        box-shadow: 0 0 15px 0 var(--glow-colour);
    }

    &:active {
        box-shadow: 0 0 30px 0 var(--glow-colour);
    }
`
export default MenuButtonGlow
