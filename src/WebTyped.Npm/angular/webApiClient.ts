import { Http } from '@angular/http';
import { WebApiEventEmmiterService, WebApiObservable } from './';
import { WebApiCallInfo } from '@guimabdo/webtyped-common';
export class WebApiClient {

    constructor(
        private baseUrl: string,
        private api: string,
        private http: Http,
        private eventEmmiter: WebApiEventEmmiterService) { }

    invokeGet<T>(info: WebApiCallInfo, action: string, search?: any): WebApiObservable<T> {
        return this.invoke(info, action, 'get', null, search);
    }
    invokePatch<T>(info: WebApiCallInfo, action: string, body?: any, search?: any): WebApiObservable<T> {
        return this.invoke(info, action, 'patch', body, search);
    }
    invokePost<T>(info: WebApiCallInfo, action: string, body?: any, search?: any): WebApiObservable<T> {
        return this.invoke(info, action, 'post', body, search);
    }
    invokePut<T>(info: WebApiCallInfo, action: string, body?: any, search?: any): WebApiObservable<T> {
        return this.invoke(info, action, 'put', body, search);
    }
    invokeDelete<T>(info: WebApiCallInfo, action: string, search?: any): WebApiObservable<T> {
        return this.invoke(info, action, 'delete', null, search);
    }
    private invoke<T>(info: WebApiCallInfo, action: string,
        httpMethod: string, body?: any, search?: any): WebApiObservable<T> {
        return new WebApiObservable<T>(info, this.eventEmmiter, this.http,
            httpMethod,
            this.baseUrl,
            this.api, action, body, search);


    }
}
