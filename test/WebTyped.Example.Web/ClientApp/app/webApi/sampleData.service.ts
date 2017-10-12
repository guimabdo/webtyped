import { Injectable, Inject, forwardRef } from '@angular/core';
import { Http } from '@angular/http';
import { WebApiClient, WebApiEventEmmiterService, WebApiObservable } from '@guimabdo/webtyped-angular';
@Injectable()
export class SampleDataService extends WebApiClient {
	constructor(http: Http, @Inject(forwardRef(() => WebApiEventEmmiterService)) eventEmmiter: WebApiEventEmmiterService) {
		super("api/SampleData", http, eventEmmiter);
	}
	WeatherForecasts = () : WebApiObservable<Array<SampleDataService.WeatherForecast>> => { return this.invokeGet<Array<SampleDataService.WeatherForecast>>({ func: this.WeatherForecasts, parameters: {} }, `WeatherForecasts`, undefined); };
}
export module SampleDataService {
	export class WeatherForecast {
		DateFormatted: string;
		TemperatureC: number;
		Summary: string;
		TemperatureF: number;
	}

}

