import { HttpClient, HttpParams } from '@angular/common/http';
import { Optional, Inject } from '@angular/core';
import { WebTypedEventEmitterService } from './';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { WebTypedCallInfo, WebTypedUtils, WebTypedInvoker } from '@guimabdo/webtyped-common';
export class WebTypedNgInvoker extends WebTypedInvoker {

    constructor(
		@Optional() @Inject('API_BASE_URL') private baseUrl: string,
		private httpClient: HttpClient,
		private eventEmitter: WebTypedEventEmitterService
	) {
		super();
	 }

	invokeGet<TParameters, TResult>(info: WebTypedCallInfo<TParameters, TResult>, action: string, search?: any): Observable<TResult> {
		return this.invoke(info, action, 'get', null, search);
	}
	invokePatch<TParameters, TResult>(info: WebTypedCallInfo<TParameters, TResult>, action: string, body?: any, search?: any): Observable<TResult> {
		return this.invoke(info, action, 'patch', body, search);
	}
	invokePost<TParameters, TResult>(info: WebTypedCallInfo<TParameters, TResult>, action: string, body?: any, search?: any): Observable<TResult> {
		return this.invoke(info, action, 'post', body, search);
	}
	invokePut<TParameters, TResult>(info: WebTypedCallInfo<TParameters, TResult>, action: string, body?: any, search?: any): Observable<TResult> {
		return this.invoke(info, action, 'put', body, search);
	}
	invokeDelete<TParameters, TResult>(info: WebTypedCallInfo<TParameters, TResult>, action: string, search?: any): Observable<TResult> {
		return this.invoke(info, action, 'delete', null, search);
	}
	private generateHttpParams(obj: any): HttpParams {
		var params = WebTypedUtils.resolveQueryParameters(obj);
		var httpParams = new HttpParams();
		params.forEach(r => httpParams = httpParams.set(r.path, r.val));
		return httpParams;
	}
	
	public invoke<TParameters, TResult>(
		info: WebTypedCallInfo<TParameters, TResult>, 
		api: string,
		action: string,
		httpMethod: string, 
		body?: any, 
		search?: any
		): Observable<TResult> {
		var httpClient = this.httpClient;
		var url = WebTypedUtils.resolveActionUrl(this.baseUrl, api, action);
	
		//Creating options
		var options: { params: undefined | HttpParams } = {
			params: undefined
		};

		if (search) {
			options.params = this.generateHttpParams(search);
		}

		var httpObservable: Observable<TResult>;
		switch (httpMethod) {
			case 'get':
				httpObservable = httpClient.get<TResult>(url, options);
				break;
			case 'put':
				httpObservable = httpClient.put<TResult>(url, body, options);
				break;
			case 'patch':
				httpObservable = httpClient.patch<TResult>(url, body, options);
				break;
			case 'delete':
				httpObservable = httpClient.delete<TResult>(url, options);
				break;
			case 'post':
			default:
				httpObservable = httpClient.post<TResult>(url, body, options);
				break;
		}

		var coreObs = httpObservable //Emit completed event
			.pipe(
				tap(data => {
					info.result = data;
					this.eventEmitter.emit(info);
				},
					r => {

					})
			);
		return coreObs;
	}
}
