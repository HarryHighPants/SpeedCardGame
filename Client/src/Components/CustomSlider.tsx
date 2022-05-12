import styled from 'styled-components'

interface Props {
    backgroundColour: string
    percentRemaining: number
    handleText: string
    handleAdditionalText: string
    additionalTextColour: string
}

const CustomSlider = ({
    backgroundColour,
    percentRemaining,
    handleText,
    handleAdditionalText,
    additionalTextColour,
}: Props) => {
    return (
        <SliderWrapper>
            <SliderEnd style={{width: percentRemaining+'%'}}>
                <SliderHandle >
                    <div style={{ margin: 0, marginTop: 20, position: 'absolute' }}>
                        {handleText}
                        <p
                            style={{
                                margin: 0,
                                marginRight: -5,
                                position: 'absolute',
                                right: 0,
                                top: 0,
                                color: additionalTextColour,
                                textAlign: 'left',
                                transform: 'translateX(100%)',
                            }}
                        >
                            <b>{handleAdditionalText}</b>
                        </p>
                    </div>
                </SliderHandle>
            </SliderEnd>

            <SliderBg style={{background: backgroundColour}} />
        </SliderWrapper>
    )
}

const SliderWrapper = styled.div`
    position: relative;
    width: 100%;
`

const SliderHandle = styled.span`
    width: 5px;
    height: 17px;
    display: flex;
    justify-content: center;
    position: absolute;
    background-color: #efefef;
    left: 0;
    bottom: -5px;
    box-shadow: 0 2px 5px rgba(0, 0, 0, 0.4);
`

const SliderEnd = styled.span`
    position: absolute;
    height: 8px;
    background: #626262;
    right: 0;
    bottom: 0;
    pointer-events: none;
    border-radius: 3px;
`
const SliderBg = styled.div`
    height: 8px;
    width: 100%;
    border-radius: 3px;
    vertical-align: bottom;
    margin: 0;
    box-shadow: 0 2px 6px rgba(0, 0, 0, 0.2);
`

export default CustomSlider
