import { HttpClient } from '@angular/common/http';
import { WebApiEventEmitterService } from './';
import { Observable } from 'rxjs';
import { WebApiCallInfo } from '@guimabdo/webtyped-common';
export declare class WebApiClient {
    private baseUrl;
    private api;
    private httpClient;
    private eventEmitter;
    constructor(baseUrl: string, api: string, httpClient: HttpClient, eventEmitter: WebApiEventEmitterService);
    invokeGet<T>(info: WebApiCallInfo, action: string, search?: any): Observable<T>;
    invokePatch<T>(info: WebApiCallInfo, action: string, body?: any, search?: any): Observable<T>;
    invokePost<T>(info: WebApiCallInfo, action: string, body?: any, search?: any): Observable<T>;
    invokePut<T>(info: WebApiCallInfo, action: string, body?: any, search?: any): Observable<T>;
    invokeDelete<T>(info: WebApiCallInfo, action: string, search?: any): Observable<T>;
    private invoke<T>(info, action, httpMethod, body?, search?);
}
