import { Injectable, Inject, forwardRef } from '@angular/core';
import { Http } from '@angular/http';
import { WebApiClient, WebApiEventEmmiterService } from '@guimabdo/webtyped-angular';
@Injectable()
export class ValuesService extends WebApiClient {
    constructor(http: Http,
        @Inject(forwardRef(() => WebApiEventEmmiterService)) eventEmmiter: WebApiEventEmmiterService) {
        super("", http, eventEmmiter);
    }
}
