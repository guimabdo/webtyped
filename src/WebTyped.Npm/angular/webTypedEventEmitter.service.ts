import { Injectable } from '@angular/core';
import { WebTypedCallInfo } from '@guimabdo/webtyped-common';
import { Subject, Observable } from 'rxjs';
import { filter } from 'rxjs/operators';
@Injectable()
export class WebTypedEventEmitterService {
    private _eventBus: Subject<WebTypedCallInfo<any>> = new Subject<WebTypedCallInfo<any>>();
    constructor() { }

    on = <TResult>(f: Function): Observable<WebTypedCallInfo<TResult>> => {
        var obs:any = this._eventBus.pipe(
            filter(e => {
                return e.func == f;
			})
		);
		return <Observable<WebTypedCallInfo<TResult>>>obs;
    };
    emit = (info: WebTypedCallInfo<any>): void => {
        this._eventBus.next(info);
    };
}