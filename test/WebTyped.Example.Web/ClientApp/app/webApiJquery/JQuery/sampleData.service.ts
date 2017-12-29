//*Generated with WebTyped*
import { WebTypedClient } from '@guimabdo/webtyped-jquery';
export class SampleDataService extends WebTypedClient {
	constructor(baseUrl: string = WebTypedClient.baseUrl) {
		super(baseUrl, "api/sampleData");
	}
	weatherForecasts = () : JQuery.jqXHR<Array<JQuery.SampleDataService.WeatherForecast>> => {
		return this.invokeGet<Array<JQuery.SampleDataService.WeatherForecast>>({
				func: this.weatherForecasts,
				parameters: {  }
			},
			`weatherForecasts`,
			undefined
		);
	};
}
