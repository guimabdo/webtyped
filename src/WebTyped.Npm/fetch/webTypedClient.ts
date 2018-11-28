import { WebTypedCallInfo, WebTypedEventEmitter, WebTypedUtils } from '@guimabdo/webtyped-common';
let param = require('jquery-param');
export class WebTypedClient {
	//Global setting
	public static baseUrl: string = null;
	public static api: string = null;
	constructor(private baseUrl: string = WebTypedClient.baseUrl,
		private api: string = WebTypedClient.api) {
		this.baseUrl = this.baseUrl || "/";
		this.api = this.api || "";
	}
	invokeGet<TParameters, TResult>(info: WebTypedCallInfo<TParameters, TResult>, action: string, search?: any): Promise<TResult> {
		return this.invoke(info, action, 'get', null, search);
	}
	invokePatch<TParameters, TResult>(info: WebTypedCallInfo<TParameters, TResult>, action: string, body?: any, search?: any): Promise<TResult> {
		return this.invoke(info, action, 'patch', body, search);
	}
	invokePost<TParameters, TResult>(info: WebTypedCallInfo<TParameters, TResult>, action: string, body?: any, search?: any): Promise<TResult> {
		return this.invoke(info, action, 'post', body, search);
	}
	invokePut<TParameters, TResult>(info: WebTypedCallInfo<TParameters, TResult>, action: string, body?: any, search?: any): Promise<TResult> {
		return this.invoke(info, action, 'put', body, search);
	}
	invokeDelete<TParameters, TResult>(info: WebTypedCallInfo<TParameters, TResult>, action: string, search?: any): Promise<TResult> {
		return this.invoke(info, action, 'delete', null, search);
	}
	private async invoke<TParameters, TResult>(info: WebTypedCallInfo<TParameters, TResult>, action: string,
		httpMethod: string, body?: any, search?: any): Promise<TResult> {
		if (typeof (fetch) === 'undefined') { return Promise.resolve<TResult>(null); }
		var url = WebTypedUtils.resolveActionUrl(this.baseUrl, this.api, action);
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
			data = await r.json();
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
