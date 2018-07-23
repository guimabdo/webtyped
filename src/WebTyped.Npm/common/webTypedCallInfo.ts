export interface WebTypedCallInfo<TParameters, TResult> {
	func: Function,
	parameters: TParameters,
	result?: TResult;
	kind: string;
}