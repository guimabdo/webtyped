import { Component, Inject } from '@angular/core';
import { MegaSampleService } from '../../webApi/services';

@Component({
    selector: 'megaSample',
    templateUrl: './megaSample.component.html'
})
export class MegaSampleComponent {
    getThisStringFromQueryResult: string;
    getThisStringFromQueryExplicitResult: string;
    getThisStringFromRouteResult: string;
    postAndReturnThisStringFromQueryResult: string;

    constructor(svc: MegaSampleService) {
        svc.GetThisStringFromQuery("test").subscribe(s => this.getThisStringFromQueryResult = s, err => console.log(err));
        svc.GetThisStringFromQueryExplicit("test").subscribe(s => this.getThisStringFromQueryExplicitResult = s, err => console.log(err));
        svc.GetThisStringFromRoute("test").subscribe(s => this.getThisStringFromRouteResult = s, err => console.log(err));
        svc.PostAndReturnThisStringFromQuery("test").subscribe(s => this.postAndReturnThisStringFromQueryResult = s, err => console.log(err));
    }
}
