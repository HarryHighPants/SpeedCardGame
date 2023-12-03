import { Region } from './Helpers/Region'

export interface IConfig {
    apiGateway: { [Region.AMERICA]: string; [Region.OCEANIA]: string }
}

const devUrl = `http://${window.location.hostname}:5169`
const dev = {
    apiGateway: {
        [Region.OCEANIA]: devUrl,
        [Region.AMERICA]: devUrl,
    },
}

const prod = {
    apiGateway: {
        [Region.OCEANIA]: `https://server.harryab.com:10000`,
        [Region.AMERICA]: `https://falcon.harryab.com`,
    },
}

const config: IConfig = process.env.REACT_APP_STAGE === 'prod' ? prod : dev

export default config
