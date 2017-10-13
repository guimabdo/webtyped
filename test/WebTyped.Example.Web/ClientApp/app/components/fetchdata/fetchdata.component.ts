import { Component, Inject } from '@angular/core';
import { Http } from '@angular/http';
import { SampleDataService } from '../../webApi/services';

@Component({
    selector: 'fetchdata',
    templateUrl: './fetchdata.component.html',
    providers: [SampleDataService]
})
export class FetchDataComponent {
    //public forecasts: WeatherForecast[];
    public forecasts: SampleDataService.WeatherForecast[];

    constructor(http: Http, svc: SampleDataService, @Inject('BASE_URL') baseUrl: string) {
        svc.WeatherForecasts().subscribe(result => this.forecasts = result, error => console.log(error));
        //http.get(baseUrl + 'api/SampleData/WeatherForecasts').subscribe(result => {
        //    this.forecasts = result.json() as WeatherForecast[];
        //}, error => console.error(error));
    }
}

//interface WeatherForecast {
//    dateFormatted: string;
//    temperatureC: number;
//    temperatureF: number;
//    summary: string;
//}
