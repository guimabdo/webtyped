import { WebTypedCallInfo, WebTypedUtils, WebTypedEventEmitter, WebTypedInvoker } from '@guimabdo/webtyped-common';
export class WebTypedFetchInvoker extends WebTypedInvoker {
    //Global setting
    public static baseUrl: string = null;
    constructor(
        private baseUrl: string = WebTypedFetchInvoker.baseUrl
    ) {
        super();
    }
    
    public async invoke<TParameters, TResult>(
        info: WebTypedCallInfo<TParameters, TResult>,
        api: string,
        action: string,
        httpMethod: string,
        body?: any,
        search?: any
    ): Promise<TResult> {
        if (typeof (fetch) === 'undefined') { return Promise.resolve<TResult>(null); }
        var url = WebTypedUtils.resolveActionUrl(this.baseUrl, api, action);
        if (search) {
            if (url.indexOf('?') < 0) {
                url += '?';
            } else {
                url += '&';
            }
            url += WebTypedUtils.resolveQueryParametersString(search); //$.param(search);
        }
        var req = fetch(url, {
            body: body ? JSON.stringify(body) : undefined,
            method: httpMethod,
            headers: new Headers({
                'Content-Type': 'application/json'
            })
        });
        let r = await req;

        if (!r.ok) {
            throw r;
        }

        let data: any;
        try {
            if (info.returnTypeName == 'string' && r.headers.get('content-type').indexOf('text/plain') == 0) {
                data = await r.text();
            } else {
                data = await r.json();
            }
        } catch (err) {
            if (err instanceof SyntaxError) {
                data = undefined;
            }
        }
        var anyWebTyped = <any>WebTypedEventEmitter;
        info.result = data;
        anyWebTyped.single.emit(info);
        return data;
    }
}
