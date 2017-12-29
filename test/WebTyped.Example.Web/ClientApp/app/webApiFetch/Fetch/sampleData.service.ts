//*Generated with WebTyped*
import { WebTypedClient } from '@guimabdo/webtyped-fetch';
export class SampleDataService extends WebTypedClient {
	constructor(baseUrl: string = WebTypedClient.baseUrl) {
		super(baseUrl, "api/sampleData");
	}
	weatherForecasts = () : Promise<Array<Fetch.SampleDataService.WeatherForecast>> => {
		return this.invokeGet<Array<Fetch.SampleDataService.WeatherForecast>>({
				func: this.weatherForecasts,
				parameters: {  }
			},
			`weatherForecasts`,
			undefined
		);
	};
}
