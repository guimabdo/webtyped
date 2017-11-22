import { Component, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { MegaSampleService } from '../../webApi/';

@Component({
    selector: 'megaSample',
    templateUrl: './megaSample.component.html'
})
export class MegaSampleComponent {
    json = JSON;
    getThisStringFromQueryResult: string;
    getThisStringFromQueryExplicitResult: string;
    getThisStringFromRouteResult: string;
    getTheseStringsResult: Array<string>;
    postAndReturnThisStringFromQueryResult: string;
    postAndReturnThisStringFromQueryResultExplicit: string;
    postAndReturnThisStringFromRouteResult: string;
    postAndReturnTheseStringsResult: Array<string>;
    postAndReturnModelResult: MegaSampleService.Model;
    postAndReturnTupleResult: { str: string, number: number };

    constructor(svc: MegaSampleService, cli: HttpClient) {
        svc.GetThisStringFromQuery("test").subscribe(s => this.getThisStringFromQueryResult = s, err => console.log(err));
        svc.GetThisStringFromQueryExplicit("test").subscribe(s => this.getThisStringFromQueryExplicitResult = s, err => console.log(err));
        svc.GetThisStringFromRoute("test").subscribe(s => this.getThisStringFromRouteResult = s, err => console.log(err));
        svc.GetTheseStrings("test1", "test2").subscribe(arr => this.getTheseStringsResult = arr, err => console.log(err));
        svc.PostAndReturnThisStringFromQuery("test").subscribe(s => this.postAndReturnThisStringFromQueryResult = s, err => console.log(err));
        svc.PostAndReturnThisStringFromQueryExplicit("test").subscribe(s => this.postAndReturnThisStringFromQueryResultExplicit = s, err => console.log(err));
        svc.PostAndReturnThisStringFromRoute("test").subscribe(s => this.postAndReturnThisStringFromRouteResult = s, err => console.log(err));
        svc.PostAndReturnTheseStrings("test1", "test2", "test3").subscribe(s => this.postAndReturnTheseStringsResult = s, err => console.log(err));
        svc.PostAndReturnModel({
            Number: 3,
            Text: "test1"
        }).subscribe(s => this.postAndReturnModelResult = s, err => console.log(err));
        svc.PostAndReturnTuple_NotWorkingYet({ str: "test1", number: 3 }).subscribe(s => this.postAndReturnTupleResult = s, err => console.log(err));
    }
}
