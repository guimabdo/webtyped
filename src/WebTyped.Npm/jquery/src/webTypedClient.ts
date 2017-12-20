import { WebTypedEventEmitter, WebTypedCallInfo } from '@guimabdo/webtyped-common';
import * as $ from 'jquery';
class FakeXhr<T> extends Promise<T> {
    state: () => "pending" | "resolved" | "rejected" = () => "pending";
    statusCode = () => 0;
    always = () => this;
    fail = () => this;
    done = () => this;
    progress = () => this;
    promise = () => this;
    constructor() {
        super((res, rej) => res(null));
    }
}
var any$ = <any>$;
any$.webtyped = new WebTypedEventEmitter();
export class WebTypedClient {
    //Global setting
    public static baseUrl: string = null;
    public static api: string = null;
    constructor(private baseUrl: string = WebTypedClient.baseUrl,
        private api: string = WebTypedClient.api) {
        this.baseUrl = this.baseUrl || "/";
        this.api = this.api || "";
    }
    invokeGet<T>(info: WebTypedCallInfo, action: string, search?: any): JQuery.jqXHR<T> {
        return this.invoke(info, action, 'get', null, search);
    }
    invokePatch<T>(info: WebTypedCallInfo, action: string, body?: any, search?: any): JQuery.jqXHR<T> {
        return this.invoke(info, action, 'patch', body, search);
    }
    invokePost<T>(info: WebTypedCallInfo, action: string, body?: any, search?: any): JQuery.jqXHR<T> {
        return this.invoke(info, action, 'post', body, search);
    }
    invokePut<T>(info: WebTypedCallInfo, action: string, body?: any, search?: any): JQuery.jqXHR<T> {
        return this.invoke(info, action, 'put', body, search);
    }
    invokeDelete<T>(info: WebTypedCallInfo, action: string, search?: any): JQuery.jqXHR<T> {
        return this.invoke(info, action, 'delete', null, search);
    }
    private invoke<T>(info: WebTypedCallInfo, action: string,
        httpMethod: string, body?: any, search?: any): JQuery.jqXHR<T> {
        if (typeof ($.ajax) === 'undefined') {
            var anyFake: any = new FakeXhr<T>();
            return <JQuery.jqXHR<T>>anyFake;
        };
        
        var baseUrl = this.baseUrl;
        if (baseUrl.endsWith('/')) { baseUrl = baseUrl.substr(0, baseUrl.length - 1); }
        var url = `${baseUrl}/${this.api}/${action}`;
        if (search) {
            if (url.indexOf('?') < 0) {
                url += '?';
            } else {
                url += '&';
            }
            url += $.param(search);
        }
        var jqXhr = $.ajax({
            url: url,
            dataType: 'json',
            contentType: 'application/json',
            data: body ? JSON.stringify(body) : undefined,
            method: httpMethod,
        });
        jqXhr.done(result => {
            var anyWebTyped = <any>WebTypedEventEmitter;
            anyWebTyped.single.emit(info);
        });
        return jqXhr;
    }
}
