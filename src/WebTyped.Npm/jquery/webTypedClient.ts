import { WebTypedEventEmitter, WebTypedCallInfo, WebTypedUtils } from '@guimabdo/webtyped-common';
import * as $ from 'jquery';
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
	invokeGet<TParameters, TResult>(info: WebTypedCallInfo<TParameters, TResult>, action: string, search?: any): JQuery.jqXHR<TResult> {
        return this.invoke(info, action, 'get', null, search);
    }
	invokePatch<TParameters, TResult>(info: WebTypedCallInfo<TParameters, TResult>, action: string, body?: any, search?: any): JQuery.jqXHR<TResult> {
        return this.invoke(info, action, 'patch', body, search);
    }
	invokePost<TParameters, TResult>(info: WebTypedCallInfo<TParameters, TResult>, action: string, body?: any, search?: any): JQuery.jqXHR<TResult> {
        return this.invoke(info, action, 'post', body, search);
    }
	invokePut<TParameters, TResult>(info: WebTypedCallInfo<TParameters, TResult>, action: string, body?: any, search?: any): JQuery.jqXHR<TResult> {
        return this.invoke(info, action, 'put', body, search);
    }
	invokeDelete<TParameters, TResult>(info: WebTypedCallInfo<TParameters, TResult>, action: string, search?: any): JQuery.jqXHR<TResult> {
        return this.invoke(info, action, 'delete', null, search);
    }
	private invoke<TParameters, TResult>(info: WebTypedCallInfo<TParameters, TResult>, action: string,
		httpMethod: string, body?: any, search?: any): JQuery.jqXHR<TResult> {
        var url = WebTypedUtils.resolveActionUrl(this.baseUrl, this.api, action);
        if (search) {
            if (url.indexOf('?') < 0) {
                url += '?';
            } else {
                url += '&';
            }
            url += WebTypedUtils.resolveQueryParametersString(search); //$.param(search);
        }
        var jqXhr = $.ajax({
            url: url,
            cache: false, //api should not be cached. IE caches it by default
            dataType: 'json',
            contentType: 'application/json',
            data: body ? JSON.stringify(body) : undefined,
            method: httpMethod,
        });
        jqXhr.done(result => {
            var anyWebTyped = <any>WebTypedEventEmitter;
            info.result = result;
            anyWebTyped.single.emit(info);
        });
        return jqXhr;
    }
}
