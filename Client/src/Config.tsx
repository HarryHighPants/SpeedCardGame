export interface IConfig {
	apiGateway: {
		URL: string;
	};
}

const dev = {
	apiGateway: {
		URL: `http://${window.location.hostname}:5169`,
	},
};

const prod = {
	apiGateway: {
		URL: `https://server.harryab.com:10000`,
	},
};

const config: IConfig = process.env.REACT_APP_STAGE === 'prod' ? prod : dev;

export default {
	...config,
};
