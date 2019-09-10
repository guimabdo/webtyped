import * as extMdl0 from './complexType';
import * as extMdl1 from './complexChildType';
import { WebTypedCallInfo, WebTypedFunction, WebTypedInvoker } from '@guimabdo/webtyped-common';
import { Observable } from 'rxjs';
export class MyService {
	static readonly controllerName = 'MyController';
	private api = 'api/my';
	constructor(private invoker: WebTypedInvoker) {}
	query: MyService.QueryFunction = (obj?: extMdl0.ComplexType) : Observable<Array<any/*object*/>> => {
		return this.invoker.invoke({
				returnTypeName: 'Array<any/*object*/>',
				kind: 'Query',
				func: this.query,
				parameters: { obj, _wtKind: 'Query' }
			},
			this.api,
			``,
			`get`,
			undefined,
			{ ...(obj ? {$skip: obj.skip, $take: obj.take, $orderby: obj.orderBy} : {}) }
		);
	};
	query2: MyService.Query2Function = (obj?: extMdl1.ComplexChildType) : Observable<Array<any/*object*/>> => {
		return this.invoker.invoke({
				returnTypeName: 'Array<any/*object*/>',
				kind: 'Query2',
				func: this.query2,
				parameters: { obj, _wtKind: 'Query2' }
			},
			this.api,
			``,
			`get`,
			undefined,
			{ ...(obj ? { obj: { search: obj.search } } : {}), ...(obj ? {$skip: obj.skip, $take: obj.take, $orderby: obj.orderBy} : {}) }
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
