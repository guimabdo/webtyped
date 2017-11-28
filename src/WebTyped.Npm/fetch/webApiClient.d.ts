import { WebApiCallInfo } from '@guimabdo/webtyped-common';
export declare class WebApiClient {
    private baseUrl;
    private api;
    static baseUrl: string;
    static api: string;
    constructor(baseUrl?: string, api?: string);
    invokeGet<T>(info: WebApiCallInfo, action: string, search?: any): Promise<T>;
    invokePatch<T>(info: WebApiCallInfo, action: string, body?: any, search?: any): Promise<T>;
    invokePost<T>(info: WebApiCallInfo, action: string, body?: any, search?: any): Promise<T>;
    invokePut<T>(info: WebApiCallInfo, action: string, body?: any, search?: any): Promise<T>;
    invokeDelete<T>(info: WebApiCallInfo, action: string, search?: any): Promise<T>;
    private invoke<T>(info, action, httpMethod, body?, search?);
}
