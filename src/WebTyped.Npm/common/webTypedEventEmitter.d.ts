import { WebTypedCallInfo } from './webTypedCallInfo';
export declare class WebTypedEventEmitter {
    private static single;
    private methods;
    private callbacks;
    constructor();
    on: (f: Function, callback: (info: WebTypedCallInfo) => any) => WebTypedEventEmitter;
    static on: (f: Function, callback: (info: WebTypedCallInfo) => any) => WebTypedEventEmitter;
    off: (f: Function, callback: (info: WebTypedCallInfo) => any) => WebTypedEventEmitter;
    static off: (f: Function, callback: (info: WebTypedCallInfo) => any) => WebTypedEventEmitter;
    private emit;
}
