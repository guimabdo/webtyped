import { WebTypedCallInfo } from '@guimabdo/webtyped-common';
export declare class WebTypedClient {
    private baseUrl;
    private api;
    static baseUrl: string;
    static api: string;
    constructor(baseUrl?: string, api?: string);
    invokeGet<T>(info: WebTypedCallInfo, action: string, search?: any): Promise<T>;
    invokePatch<T>(info: WebTypedCallInfo, action: string, body?: any, search?: any): Promise<T>;
    invokePost<T>(info: WebTypedCallInfo, action: string, body?: any, search?: any): Promise<T>;
    invokePut<T>(info: WebTypedCallInfo, action: string, body?: any, search?: any): Promise<T>;
    invokeDelete<T>(info: WebTypedCallInfo, action: string, search?: any): Promise<T>;
    private invoke<T>(info, action, httpMethod, body?, search?);
}
