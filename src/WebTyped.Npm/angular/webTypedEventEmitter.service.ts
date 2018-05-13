import { Injectable } from '@angular/core';
import { WebTypedCallInfo } from '@guimabdo/webtyped-common';
import { Subject } from 'rxjs/Subject';
import { Observable } from 'rxjs/Observable';
@Injectable()
export class WebTypedEventEmitterService {
    private _eventBus: Subject<WebTypedCallInfo<any>> = new Subject<WebTypedCallInfo<any>>();
    constructor() { }

    on = <TResult>(f: Function): Observable<WebTypedCallInfo<TResult>> => {
        return this._eventBus
            .filter(e => {
                return e.func == f;
            });
    };
    emit = (info: WebTypedCallInfo<any>): void => {
        this._eventBus.next(info);
    };
}