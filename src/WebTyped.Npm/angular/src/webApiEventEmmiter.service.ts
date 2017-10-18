import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { WebApiCallInfo } from '@guimabdo/webtyped-common';
@Injectable()
export class WebApiEventEmmiterService {
    private _eventBus: Subject<WebApiCallInfo> = new Subject<WebApiCallInfo>();
    constructor() { }

    on = (f: Function): Observable<WebApiCallInfo> => {
        return this._eventBus
            .filter(e => {
                return e.func == f;
            });
    };
    emit = (info: WebApiCallInfo): void => {
        this._eventBus.next(info);
    };
}