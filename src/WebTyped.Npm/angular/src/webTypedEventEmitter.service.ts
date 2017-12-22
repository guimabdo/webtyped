import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { WebTypedCallInfo } from '@guimabdo/webtyped-common';
@Injectable()
export class WebTypedEventEmitterService {
    private _eventBus: Subject<WebTypedCallInfo> = new Subject<WebTypedCallInfo>();
    constructor() { }

    on = (f: Function): Observable<WebTypedCallInfo> => {
        return this._eventBus
            .filter(e => {
                return e.func == f;
            });
    };
    emit = (info: WebTypedCallInfo): void => {
        this._eventBus.next(info);
    };
}