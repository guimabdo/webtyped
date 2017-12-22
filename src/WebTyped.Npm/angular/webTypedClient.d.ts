import { HttpClient } from '@angular/common/http';
import { WebTypedEventEmitterService } from './';
import { Observable } from 'rxjs';
import { WebTypedCallInfo } from '@guimabdo/webtyped-common';
export declare class WebApiClient {
    private baseUrl;
    private api;
    private httpClient;
    private eventEmitter;
    constructor(baseUrl: string, api: string, httpClient: HttpClient, eventEmitter: WebTypedEventEmitterService);
    invokeGet<T>(info: WebTypedCallInfo, action: string, search?: any): Observable<T>;
    invokePatch<T>(info: WebTypedCallInfo, action: string, body?: any, search?: any): Observable<T>;
    invokePost<T>(info: WebTypedCallInfo, action: string, body?: any, search?: any): Observable<T>;
    invokePut<T>(info: WebTypedCallInfo, action: string, body?: any, search?: any): Observable<T>;
    invokeDelete<T>(info: WebTypedCallInfo, action: string, search?: any): Observable<T>;
    private invoke<T>(info, action, httpMethod, body?, search?);
}
