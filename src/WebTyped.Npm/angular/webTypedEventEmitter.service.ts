import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { WebTypedCallInfo } from '@guimabdo/webtyped-common';
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