import { WebApiClient } from '@guimabdo/webtyped-fetch';
export class SampleDataService extends WebApiClient {
	constructor(baseUrl: string = WebApiClient.baseUrl) {
		super(baseUrl, "api/SampleData");
	}
	WeatherForecasts = () : Promise<Array<Fetch.SampleDataService.WeatherForecast>> => {
		return this.invokeGet<Array<Fetch.SampleDataService.WeatherForecast>>({
				func: this.WeatherForecasts,
				parameters: {  }
			},
			`WeatherForecasts`,
			undefined
		);
	};
}
