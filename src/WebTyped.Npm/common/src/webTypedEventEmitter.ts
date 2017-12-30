import { WebTypedCallInfo } from './webTypedCallInfo';
export class WebTypedEventEmitter {
    private static single = new WebTypedEventEmitter();
    private methods: Array<Function> = [];
    private callbacks: Array<Array<(info: WebTypedCallInfo<any>) => any>> = [];
    constructor() { }
    on = (f: Function, callback: (info: WebTypedCallInfo<any>) => any): WebTypedEventEmitter => {
        var index = this.methods.indexOf(f);
        if (index < 0) {
            index = this.methods.length;
            this.methods.push(f);
            this.callbacks.push([]);
        }
        this.callbacks[index].push(callback);
        return this;
    };
    static on = (f: Function, callback: (info: WebTypedCallInfo<any>) => any): WebTypedEventEmitter => {
        return WebTypedEventEmitter.single.on(f, callback);
    };
    off = (f: Function, callback: (info: WebTypedCallInfo<any>) => any): WebTypedEventEmitter => {
        var index = this.methods.indexOf(f);
        if (index >= 0) {
            var callbackIndex = this.callbacks[index].indexOf(callback);
            if (callbackIndex >= 0) {
                this.callbacks[index].splice(callbackIndex, 1);
            }
        }
        return this;
    };
    static off = (f: Function, callback: (info: WebTypedCallInfo<any>) => any): WebTypedEventEmitter => {
        return WebTypedEventEmitter.single.off(f, callback);
    };
    private emit = (info: WebTypedCallInfo<any>): void => {
        var index = this.methods.indexOf(info.func);
        if (index >= 0) {
            this.callbacks[index].forEach(c => { c(info); });
        }
    };
}