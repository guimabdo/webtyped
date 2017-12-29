import { Component, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { MegaSampleService } from '../../webApi/angular';
import { MegaSampleService as JMegaSampleService } from '../../webApiJquery/jQuery';
import { MegaSampleService as FMegaSampleService } from '../../webApiFetch/fetch';
import * as $ from 'jquery';
class Results {
    getThisStringFromQueryResult: string | null = null;
    getThisStringFromQueryExplicitResult: string | null = null;
    getThisStringFromRouteResult: string | null = null;
    getTheseStringsResult: Array<string> | null = null;
    postAndReturnThisStringFromQueryResult: string | null = null;
    postAndReturnThisStringFromQueryResultExplicit: string | null = null;
    postAndReturnThisStringFromRouteResult: string | null = null;
    postAndReturnTheseStringsResult: Array<string> | null = null;
    postAndReturnModelResult: Angular.MegaSampleService.Model | null = null;
    postAndReturnTupleResult: { str: string, number: number } | null = null;
}
@Component({
    selector: 'megaSample',
    templateUrl: './megaSample.component.html'
})
export class MegaSampleComponent {
    json = JSON;
    angular = new Results();
    jquery = new Results();
    fetch = new Results();
    //getThisStringFromQueryResult: string;
    //getThisStringFromQueryExplicitResult: string;
    //getThisStringFromRouteResult: string;
    //getTheseStringsResult: Array<string>;
    //postAndReturnThisStringFromQueryResult: string;
    //postAndReturnThisStringFromQueryResultExplicit: string;
    //postAndReturnThisStringFromRouteResult: string;
    //postAndReturnTheseStringsResult: Array<string>;
    //postAndReturnModelResult: Angular.MegaSampleService.Model;
    //postAndReturnTupleResult: { str: string, number: number };

    constructor(svc: MegaSampleService, cli: HttpClient) {

        //Angular
        svc.getThisStringFromQuery("test").subscribe(s => this.angular.getThisStringFromQueryResult = s, err => console.log(err));
        svc.getThisStringFromQueryExplicit("test").subscribe(s => this.angular.getThisStringFromQueryExplicitResult = s, err => console.log(err));
        svc.getThisStringFromRoute("test").subscribe(s => this.angular.getThisStringFromRouteResult = s, err => console.log(err));
        svc.getTheseStrings("test1", "test2").subscribe(arr => this.angular.getTheseStringsResult = arr, err => console.log(err));
        svc.postAndReturnThisStringFromQuery("test").subscribe(s => this.angular.postAndReturnThisStringFromQueryResult = s, err => console.log(err));
        svc.postAndReturnThisStringFromQueryExplicit("test").subscribe(s => this.angular.postAndReturnThisStringFromQueryResultExplicit = s, err => console.log(err));
        svc.postAndReturnThisStringFromRoute("test").subscribe(s => this.angular.postAndReturnThisStringFromRouteResult = s, err => console.log(err));
        svc.postAndReturnTheseStrings("test1", "test2", "test3").subscribe(s => this.angular.postAndReturnTheseStringsResult = s, err => console.log(err));
        svc.postAndReturnModel({
            number: 3,
            text: "test1"
        }).subscribe(s => this.angular.postAndReturnModelResult = s, err => console.log(err));
        svc.postAndReturnTuple_NotWorkingYet({ str: "test1", number: 3 }).subscribe(s => this.angular.postAndReturnTupleResult = s, err => console.log(err));

        //jQuery
        var jSvc = new JMegaSampleService();
        jSvc.getThisStringFromQuery("test").done(s => this.jquery.getThisStringFromQueryResult = s).fail(err => console.log(err));
        jSvc.getThisStringFromQueryExplicit("test").done(s => this.jquery.getThisStringFromQueryExplicitResult = s).fail(err => console.log(err));
        jSvc.getThisStringFromRoute("test").done(s => this.jquery.getThisStringFromRouteResult = s).fail(err => console.log(err));
        jSvc.getTheseStrings("test1", "test2").done(arr => this.jquery.getTheseStringsResult = arr).fail(err => console.log(err));
        jSvc.postAndReturnThisStringFromQuery("test").done(s => this.jquery.postAndReturnThisStringFromQueryResult = s).fail(err => console.log(err));
        jSvc.postAndReturnThisStringFromQueryExplicit("test").done(s => this.jquery.postAndReturnThisStringFromQueryResultExplicit = s).fail(err => console.log(err));
        jSvc.postAndReturnThisStringFromRoute("test").done(s => this.jquery.postAndReturnThisStringFromRouteResult = s).fail(err => console.log(err));
        jSvc.postAndReturnTheseStrings("test1", "test2", "test3").done(s => this.jquery.postAndReturnTheseStringsResult = s).fail(err => console.log(err));
        jSvc.postAndReturnModel({
            number: 3,
            text: "test1"
        }).done(s => this.jquery.postAndReturnModelResult = s).fail(err => console.log(err));
        jSvc.postAndReturnTuple_NotWorkingYet({ str: "test1", number: 3 }).done(s => this.jquery.postAndReturnTupleResult = s).fail(err => console.log(err));

        //Fetch

        var fSvc = new FMegaSampleService();
        fSvc.getThisStringFromQuery("test").then(s => this.fetch.getThisStringFromQueryResult = s, err => console.log(err));
        fSvc.getThisStringFromQueryExplicit("test").then(s => this.fetch.getThisStringFromQueryExplicitResult = s, err => console.log(err));
        fSvc.getThisStringFromRoute("test").then(s => this.fetch.getThisStringFromRouteResult = s, err => console.log(err));
        fSvc.getTheseStrings("test1", "test2").then(arr => this.fetch.getTheseStringsResult = arr, err => console.log(err));
        fSvc.postAndReturnThisStringFromQuery("test").then(s => this.fetch.postAndReturnThisStringFromQueryResult = s, err => console.log(err));
        fSvc.postAndReturnThisStringFromQueryExplicit("test").then(s => this.fetch.postAndReturnThisStringFromQueryResultExplicit = s, err => console.log(err));
        fSvc.postAndReturnThisStringFromRoute("test").then(s => this.fetch.postAndReturnThisStringFromRouteResult = s, err => console.log(err));
        fSvc.postAndReturnTheseStrings("test1", "test2", "test3").then(s => this.fetch.postAndReturnTheseStringsResult = s, err => console.log(err));
        fSvc.postAndReturnModel({
            number: 3,
            text: "test1"
        }).then(s => this.fetch.postAndReturnModelResult = s, err => console.log(err));
        fSvc.postAndReturnTuple_NotWorkingYet({ str: "test1", number: 3 }).then(s => this.fetch.postAndReturnTupleResult = s, err => console.log(err));

    }
    enumerateResults(r: Results) {
        var arr = [];
        for (var p in r) {
            let anyR: any = r;
            var val = anyR[p];
            arr.push({ name: p, value: val });
        }
        return arr;
    }
}
