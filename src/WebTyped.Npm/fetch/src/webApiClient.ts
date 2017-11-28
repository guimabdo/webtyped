import { WebApiCallInfo } from '@guimabdo/webtyped-common';
let param = require('jquery-param');
export class WebApiClient {
    //Global setting
    public static baseUrl: string = null;
    public static api: string = null;
    constructor(private baseUrl: string = WebApiClient.baseUrl,
        private api: string = WebApiClient.api) {
        this.baseUrl = this.baseUrl || "/";
        this.api = this.api || "";
    }
    invokeGet<T>(info: WebApiCallInfo, action: string, search?: any): Promise<T> {
        return this.invoke(info, action, 'get', null, search);
    }
    invokePatch<T>(info: WebApiCallInfo, action: string, body?: any, search?: any): Promise<T> {
        return this.invoke(info, action, 'patch', body, search);
    }
    invokePost<T>(info: WebApiCallInfo, action: string, body?: any, search?: any): Promise<T> {
        return this.invoke(info, action, 'post', body, search);
    }
    invokePut<T>(info: WebApiCallInfo, action: string, body?: any, search?: any): Promise<T> {
        return this.invoke(info, action, 'put', body, search);
    }
    invokeDelete<T>(info: WebApiCallInfo, action: string, search?: any): Promise<T> {
        return this.invoke(info, action, 'delete', null, search);
    }
    private invoke<T>(info: WebApiCallInfo, action: string,
        httpMethod: string, body?: any, search?: any): Promise<T> {
        if (typeof (fetch) === 'undefined') { return Promise.resolve<T>(null); }
        var baseUrl = this.baseUrl;
        if (baseUrl.endsWith('/')) { baseUrl = baseUrl.substr(0, baseUrl.length - 1); }
        var url = `${baseUrl}/${this.api}/${action}`;
        if (search) {
            if (url.indexOf('?') < 0) {
                url += '?';
            } else {
                url += '&';
            }
            url += param(search);
        }
        var req = fetch(url, {
            body: body ? JSON.stringify(body) : undefined,
            method: httpMethod,
            headers: new Headers({
                'Content-Type': 'application/json'
            })
        });
        var promise = new Promise<T>((resolve, reject) => {
            req.then(r => resolve(r.json()), reason => reject(reason));
        });
        return promise;
    }
}
