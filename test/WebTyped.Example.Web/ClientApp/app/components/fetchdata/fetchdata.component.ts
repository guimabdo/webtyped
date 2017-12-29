import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';
import { SampleDataService } from '../../webApi/Angular';

@Component({
    selector: 'fetchdata',
    templateUrl: './fetchdata.component.html',
    providers: [SampleDataService]
})
export class FetchDataComponent {
    public forecasts: Angular.SampleDataService.WeatherForecast[];

    constructor(http: Http, svc: SampleDataService, @Inject('BASE_URL') baseUrl: string) {
        svc.weatherForecasts().subscribe(result => this.forecasts = result, error => console.log(error));
    }
}

