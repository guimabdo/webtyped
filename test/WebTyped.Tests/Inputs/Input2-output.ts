import * as extMdl0 from './complexType';
import * as extMdl1 from './complexChildType';
import { WebTypedCallInfo, WebTypedFunction } from '@guimabdo/webtyped-common';
import { Injectable, Inject, forwardRef, Optional } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { WebTypedClient, WebTypedEventEmitterService } from '@guimabdo/webtyped-angular';
import { Observable } from 'rxjs';
@Injectable()
export class MyService extends WebTypedClient {
	constructor(@Optional() @Inject('API_BASE_URL') baseUrl: string, httpClient: HttpClient, @Inject(forwardRef(() => WebTypedEventEmitterService)) eventEmitter: WebTypedEventEmitterService) {
		super(baseUrl, 'api/my', httpClient, eventEmitter);
	}
	query: MyService.QueryFunction = (obj?: extMdl0.ComplexType) : Observable<Array<any/*object*/>> => {
		return this.invokeGet({
				kind: 'Query',
				func: this.query,
				parameters: { obj, _wtKind: 'Query' }
			},
			``,
			{ ...(obj ? {$skip: obj.skip, $take: obj.take, $orderby: obj.orderBy} : {}) }
		);
	};
	query2: MyService.Query2Function = (obj?: extMdl1.ComplexChildType) : Observable<Array<any/*object*/>> => {
		return this.invokeGet({
				kind: 'Query2',
				func: this.query2,
				parameters: { obj, _wtKind: 'Query2' }
			},
			``,
			{ obj: { search: obj.search }, ...(obj ? {$skip: obj.skip, $take: obj.take, $orderby: obj.orderBy} : {}) }
		);
	};
}
export namespace MyService {
	export type QueryParameters = {obj?: extMdl0.ComplexType, _wtKind: 'Query' };
	export interface QueryCallInfo extends WebTypedCallInfo<QueryParameters, Array<any/*object*/>> { kind: 'Query'; }
	export type QueryFunctionBase = (obj?: extMdl0.ComplexType) => Observable<Array<any/*object*/>>;
	export interface QueryFunction extends WebTypedFunction<QueryParameters, Array<any/*object*/>>, QueryFunctionBase {}
	export type Query2Parameters = {obj?: extMdl1.ComplexChildType, _wtKind: 'Query2' };
	export interface Query2CallInfo extends WebTypedCallInfo<Query2Parameters, Array<any/*object*/>> { kind: 'Query2'; }
	export type Query2FunctionBase = (obj?: extMdl1.ComplexChildType) => Observable<Array<any/*object*/>>;
	export interface Query2Function extends WebTypedFunction<Query2Parameters, Array<any/*object*/>>, Query2FunctionBase {}
}
