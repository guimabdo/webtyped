import { HttpClient, HttpParams } from '@angular/common/http';
import { WebTypedEventEmitterService } from './';
import { Observable } from 'rxjs';
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
    //private SetValue(path: string, val: any, params: HttpParams) {
    //    if (val === undefined) { return params; }
    //    if (val === null) { return params.set(path, ""); }
    //    if (Array.isArray(val)) {
    //        val.forEach((item, index) => {
    //            params = this.SetValue(`${path}[${index}]`, item, params);
    //        });
    //        return params;
    //    }
    //    if (typeof val === "object") {
    //        return this.GenerateHttpParams(val, params, path);
    //    }
    //    return params.set(path, val);
    //}
    //private GenerateHttpParams(obj: any, params?: HttpParams, parentField?: string): HttpParams {
      
    //    if (!params) { params = new HttpParams(); }
    //    for (let field in obj) {
    //        var val = obj[field];
    //        var pathElements = [];
    //        if (parentField) { pathElements.push(parentField); }
    //        pathElements.push(field);
    //        var path = pathElements.join('.');
    //        params = this.SetValue(path, val, params);

    //        //if (val === undefined) { continue; }
    //        //if (val === null) { params = params.set(path, ""); continue; }
    //        //if (Array.isArray(val)) {
    //        //    val.forEach((item, index) => {
                    
    //        //    });
    //        //    continue;
    //        //}
    //        //if (typeof val === "object") {
    //        //    params = this.GenerateHttpParams(val, params, path);
    //        //    continue;
    //        //}
    //        //params = params.set(path, val);
    //    }
    //    return params;
    //}
    private generateHttpParams(obj: any): HttpParams {
        var params = WebTypedUtils.resolveQueryParameters(obj);
        var httpParams = new HttpParams();
        params.forEach(r => httpParams = httpParams.set(r.path, r.val));
        return httpParams;
    }
    private invoke<T>(info: WebTypedCallInfo<T>, action: string,
        httpMethod: string, body?: any, search?: any): Observable<T> {
        var baseUrl = this.baseUrl || "";
        var httpClient = this.httpClient;
        if (baseUrl.endsWith('/')) { baseUrl = baseUrl.substr(0, baseUrl.length - 1); }
        var url = `${baseUrl}/${this.api}/${action}`;

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
