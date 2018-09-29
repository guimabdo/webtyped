import { Injectable } from '@angular/core';
import { WebTypedCallInfo, WebTypedFunction } from '@guimabdo/webtyped-common';
import { Subject, Observable } from 'rxjs';
import { filter } from 'rxjs/operators';
@Injectable()
export class WebTypedEventEmitterService {
    private _eventBus: Subject<WebTypedCallInfo<any, any>> = new Subject<WebTypedCallInfo<any, any>>();
    constructor() { }

	on = <TParameters, TResult>(f: WebTypedFunction<TParameters, TResult>): Observable<WebTypedCallInfo<TParameters, TResult>> => {
        var obs:any = this._eventBus.pipe(
            filter(e => {
                return e.func == f;
			})
		);
		return <Observable<WebTypedCallInfo<TParameters, TResult>>>obs;
    };
	emit = <TParameters, TResult>(info: WebTypedCallInfo<TParameters, TResult>): void => {
        this._eventBus.next(info);
    };
}