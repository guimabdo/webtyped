//import { WebTypedInvoker } from './webTypedInvoker';
//import { WebTypedCallInfo } from './webTypedCallInfo';

//export class WebTypedClient<TResultHolder> {
//    constructor(
//        private baseUrl: string,
//        private api: string,
//        private invoker: WebTypedInvoker
//    ) { }

//    invokeGet<TParameters, TResult>(info: WebTypedCallInfo<TParameters, TResult>, action: string, search?: any): Observable<TResult> {
//        return this.invoker.invoke(info, action, 'get', null, search);
//    }
//    invokePatch<TParameters, TResult>(info: WebTypedCallInfo<TParameters, TResult>, action: string, body?: any, search?: any): Observable<TResult> {
//        return this.invoker.invoke(info, action, 'patch', body, search);
//    }
//    invokePost<TParameters, TResult>(info: WebTypedCallInfo<TParameters, TResult>, action: string, body?: any, search?: any): Observable<TResult> {
//        return this.invoker.invoke(info, action, 'post', body, search);
//    }
//    invokePut<TParameters, TResult>(info: WebTypedCallInfo<TParameters, TResult>, action: string, body?: any, search?: any): Observable<TResult> {
//        return this.invoker.invoke(info, action, 'put', body, search);
//    }
//    invokeDelete<TParameters, TResult>(info: WebTypedCallInfo<TParameters, TResult>, action: string, search?: any): Observable<TResult> {
//        return this.invoker.invoke(info, action, 'delete', null, search);
//    }
//}
