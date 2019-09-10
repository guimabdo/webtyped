import { WebTypedCallInfo } from './webTypedCallInfo';
export abstract class WebTypedInvoker {
    public abstract invoke<TParameters, TResult>(
        info: WebTypedCallInfo<TParameters, TResult>,
        api: string,
        action: string,
        httpMethod: string,
        body?: any,
        search?: any
    ): any;
}
