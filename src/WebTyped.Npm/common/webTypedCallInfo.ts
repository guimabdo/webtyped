import { WebTypedFunction } from "./webTypedFunction";
export interface WebTypedCallInfo<TParameters, TResult> {
	func: WebTypedFunction<TParameters, TResult>,
	parameters: TParameters,
	result?: TResult;
    kind: string;
    returnTypeName: 'string' | 'number' | 'boolean' | 'undefined' | 'object';
}
