import { WebApiClient } from '@guimabdo/webtyped-jquery';
export class SampleDataService extends WebApiClient {
	constructor(baseUrl: string = WebApiClient.baseUrl) {
		super(baseUrl, "api/SampleData");
	}
	WeatherForecasts = () : JQuery.jqXHR<Array<JQuery.SampleDataService.WeatherForecast>> => {
		return this.invokeGet<Array<JQuery.SampleDataService.WeatherForecast>>({
				func: this.WeatherForecasts,
				parameters: {  }
			},
			`WeatherForecasts`,
			undefined
		);
	};
}
