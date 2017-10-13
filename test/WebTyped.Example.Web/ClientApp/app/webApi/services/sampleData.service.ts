import { Injectable, Inject, forwardRef } from '@angular/core';
import { Http } from '@angular/http';
import { WebApiClient, WebApiEventEmmiterService, WebApiObservable } from '@guimabdo/webtyped-angular';
@Injectable()
export class SampleDataService extends WebApiClient {
	constructor(@Inject('API_BASE_URL') baseUrl: string, http: Http, @Inject(forwardRef(() => WebApiEventEmmiterService)) eventEmmiter: WebApiEventEmmiterService) {
		super(baseUrl, "api/SampleData", http, eventEmmiter);
	}
	WeatherForecasts = () : WebApiObservable<Array<SampleDataService.WeatherForecast>> => { return this.invokeGet<Array<SampleDataService.WeatherForecast>>({ func: this.WeatherForecasts, parameters: {} }, `WeatherForecasts`, undefined); };
}

