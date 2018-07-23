import { WebTypedFunction } from "@guimabdo/webtyped-common";
export interface WebTypedCallInfo<TParameters, TResult> {
	func: WebTypedFunction<TParameters, TResult>,
	parameters: TParameters,
	result?: TResult;
	kind: string;
}