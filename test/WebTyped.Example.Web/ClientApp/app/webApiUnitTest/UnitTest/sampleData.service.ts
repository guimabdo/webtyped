//*Generated with WebTyped*
import { Injectable, Inject, forwardRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { WebApiClient, WebApiEventEmmiterService } from '@guimabdo/webtyped-angular';
import { Observable } from 'rxjs';
@Injectable()
export class SampleDataService extends WebApiClient {
	constructor(@Inject('API_BASE_URL') baseUrl: string, httpClient: HttpClient, @Inject(forwardRef(() => WebApiEventEmmiterService)) eventEmmiter: WebApiEventEmmiterService) {
		super(baseUrl, "api/SampleData", httpClient, eventEmmiter);
	}
	WeatherForecasts = () : Observable<Array<UnitTest.SampleDataService.WeatherForecast>> => {
		return this.invokeGet<Array<UnitTest.SampleDataService.WeatherForecast>>({
				func: this.WeatherForecasts,
				parameters: {  }
			},
			`WeatherForecasts`,
			undefined
		);
	};
}
