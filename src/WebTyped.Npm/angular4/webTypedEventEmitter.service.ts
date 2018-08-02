import { Injectable } from '@angular/core';
import { WebTypedCallInfo, WebTypedFunction } from '@guimabdo/webtyped-common';
import { Subject } from 'rxjs/Subject';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/filter';
@Injectable()
export class WebTypedEventEmitterService {
	private _eventBus: Subject<WebTypedCallInfo<any, any>> = new Subject<WebTypedCallInfo<any, any>>();
	constructor() { }

	on = <TParameters, TResult>(f: WebTypedFunction<TParameters, TResult>)
		: Observable<WebTypedCallInfo<TParameters, TResult>> => {
		var obs = this._eventBus
			.asObservable()
			.filter(e => {
				return e.func == f;
			});
		return <Observable<WebTypedCallInfo<TParameters, TResult>>>obs;
	};
	emit = (info: WebTypedCallInfo<any, any>): void => {
		this._eventBus.next(info);
	};
}