export interface WebTypedCallInfo<TResult> {
    func: Function,
    parameters: any,
    result?: TResult;
}