import { HttpClient, HttpParams } from '@angular/common/http';
import { WebTypedEventEmitterService } from './';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/do';	
import { WebTypedCallInfo, WebTypedUtils } from '@guimabdo/webtyped-common';
export class WebTypedClient {

    constructor(
        private baseUrl: string,
        private api: string,
        private httpClient: HttpClient,
        private eventEmitter: WebTypedEventEmitterService) { }

    invokeGet<T>(info: WebTypedCallInfo<T>, action: string, search?: any): Observable<T> {
        return this.invoke(info, action, 'get', null, search);
    }
    invokePatch<T>(info: WebTypedCallInfo<T>, action: string, body?: any, search?: any): Observable<T> {
        return this.invoke(info, action, 'patch', body, search);
    }
    invokePost<T>(info: WebTypedCallInfo<T>, action: string, body?: any, search?: any): Observable<T> {
        return this.invoke(info, action, 'post', body, search);
    }
    invokePut<T>(info: WebTypedCallInfo<T>, action: string, body?: any, search?: any): Observable<T> {
        return this.invoke(info, action, 'put', body, search);
    }
    invokeDelete<T>(info: WebTypedCallInfo<T>, action: string, search?: any): Observable<T> {
        return this.invoke(info, action, 'delete', null, search);
    }
    private addObjectToQueryParams(params: HttpParams, val: any, parentName: string): HttpParams {
        for (let i in val) {
            var pathElements = [];
            if (parentName) { pathElements.push(parentName); }
            pathElements.push(i);
            var path = pathElements.join('.');

            let fVal = val[i];
            if (typeof fVal === "object") {
                this.addObjectToQueryParams(params, fVal, path);
                continue;
            } //Currently just getting first level of fields. TODO: See how asp.net webapi manage nested objects from uri and then adjust here
            if (fVal === undefined) { continue; }
            if (fVal === null) { fVal = ""; }
            params = params.set(path, fVal);
        }
        return params;
    }
    private generateHttpParams(obj: any): HttpParams {
        var params = WebTypedUtils.resolveQueryParameters(obj);
        var httpParams = new HttpParams();
        params.forEach(r => httpParams = httpParams.set(r.path, r.val));
        return httpParams;
    }
    private invoke<T>(info: WebTypedCallInfo<T>, action: string,
        httpMethod: string, body?: any, search?: any): Observable<T> {
        var httpClient = this.httpClient;
        var url = WebTypedUtils.resolveActionUrl(this.baseUrl, this.api, action);

        //var fData = "FormData";
        //var isFormData = body && typeof (body) === fData;

        ////Creating headers
        //var headers = new HttpHeaders();
        //if (!isFormData) { // multiplart header is resolved by the browser
        //    headers.set("Content-Type", "application/json");
        //    //If not formadata, stringify
        //    if (body) {
        //        body = JSON.stringify(body);
        //    }
        //}

        //Creating options
        var options: { params: undefined | HttpParams } = {
            params: undefined
        };

        if (search) {
            options.params = this.generateHttpParams(search);
        }
        
        var httpObservable: Observable<T>;
        switch (httpMethod) {
            case 'get':
                httpObservable = httpClient.get<T>(url, options);
                break;
            case 'put':
                httpObservable = httpClient.put<T>(url, body, options);
                break;
            case 'patch':
                httpObservable = httpClient.patch<T>(url, body, options);
                break;
            case 'delete':
                httpObservable = httpClient.delete<T>(url, options);
                break;
            case 'post':
            default:
                httpObservable = httpClient.post<T>(url, body, options);
                break;
        }

        var coreObs = httpObservable //Emit completed event
            .do(data => {
                info.result = data;
                this.eventEmitter.emit(info);
            },
            r => {
                
            });
        return coreObs;
    }
}
