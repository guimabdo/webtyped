import { Component, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { MegaSampleService } from '../../webApi/Angular';
import { MegaSampleService as JMegaSampleService } from '../../webApiJquery/JQuery';
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
        svc.GetThisStringFromQuery("test").subscribe(s => this.angular.getThisStringFromQueryResult = s, err => console.log(err));
        svc.GetThisStringFromQueryExplicit("test").subscribe(s => this.angular.getThisStringFromQueryExplicitResult = s, err => console.log(err));
        svc.GetThisStringFromRoute("test").subscribe(s => this.angular.getThisStringFromRouteResult = s, err => console.log(err));
        svc.GetTheseStrings("test1", "test2").subscribe(arr => this.angular.getTheseStringsResult = arr, err => console.log(err));
        svc.PostAndReturnThisStringFromQuery("test").subscribe(s => this.angular.postAndReturnThisStringFromQueryResult = s, err => console.log(err));
        svc.PostAndReturnThisStringFromQueryExplicit("test").subscribe(s => this.angular.postAndReturnThisStringFromQueryResultExplicit = s, err => console.log(err));
        svc.PostAndReturnThisStringFromRoute("test").subscribe(s => this.angular.postAndReturnThisStringFromRouteResult = s, err => console.log(err));
        svc.PostAndReturnTheseStrings("test1", "test2", "test3").subscribe(s => this.angular.postAndReturnTheseStringsResult = s, err => console.log(err));
        svc.PostAndReturnModel({
            Number: 3,
            Text: "test1"
        }).subscribe(s => this.angular.postAndReturnModelResult = s, err => console.log(err));
        svc.PostAndReturnTuple_NotWorkingYet({ str: "test1", number: 3 }).subscribe(s => this.angular.postAndReturnTupleResult = s, err => console.log(err));

        //jQuery
        if ('ajax' in $) { //jquery wont work server side
            var jSvc = new JMegaSampleService();
            jSvc.GetThisStringFromQuery("test").done(s => this.jquery.getThisStringFromQueryResult = s).fail(err => console.log(err));
            jSvc.GetThisStringFromQueryExplicit("test").done(s => this.jquery.getThisStringFromQueryExplicitResult = s).fail( err => console.log(err));
            jSvc.GetThisStringFromRoute("test").done(s => this.jquery.getThisStringFromRouteResult = s).fail( err => console.log(err));
            jSvc.GetTheseStrings("test1", "test2").done(arr => this.jquery.getTheseStringsResult = arr).fail( err => console.log(err));
            jSvc.PostAndReturnThisStringFromQuery("test").done(s => this.jquery.postAndReturnThisStringFromQueryResult = s).fail( err => console.log(err));
            jSvc.PostAndReturnThisStringFromQueryExplicit("test").done(s => this.jquery.postAndReturnThisStringFromQueryResultExplicit = s).fail( err => console.log(err));
            jSvc.PostAndReturnThisStringFromRoute("test").done(s => this.jquery.postAndReturnThisStringFromRouteResult = s).fail( err => console.log(err));
            jSvc.PostAndReturnTheseStrings("test1", "test2", "test3").done(s => this.jquery.postAndReturnTheseStringsResult = s).fail( err => console.log(err));
            jSvc.PostAndReturnModel({
                Number: 3,
                Text: "test1"
            }).done(s => this.jquery.postAndReturnModelResult = s).fail( err => console.log(err));
            jSvc.PostAndReturnTuple_NotWorkingYet({ str: "test1", number: 3 }).done(s => this.jquery.postAndReturnTupleResult = s).fail( err => console.log(err));
        }
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
