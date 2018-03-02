﻿import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponse, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
@Injectable()
export class WebTypedInterceptor implements HttpInterceptor {
    constructor() { }

    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        var body = req.body;
        //Stringify strings
        if ((typeof body) === "string") {
            body = JSON.stringify(body);
        }
        //Force always application/json (otherwise contenttype will be text/plain for strings)
        var headers = req.headers;
        headers = headers.set("Content-Type", "application/json");

        var clonedRequest = req.clone({
            responseType: 'text',
            body: body,
            headers: headers
        });
       
        return next.handle(clonedRequest)
            .map((event: HttpEvent<any>) => {
                //Manage response so void and "string" responses wont produces parse exception
                if (event instanceof HttpResponse) {
                    var body: any;
                    try {
                        //(Accepting json strings)
                        body = JSON.parse(event.body);
                    } catch (err) {
                        //For actions that returns a string or empty(for void)
                        //asp.net will send the string without quotes
                        //and parse will fail. (Unless you use attr [Produces("application/json")])
                        body = event.body;
                    }
                    
                    return event.clone({
                        body: body,
                    });
                }
                return event;
            })
            //.catch((error: HttpErrorResponse) => {
            //    const parsedError = Object.assign({}, error, { error: JSON.parse(error.error) });
            //    return Observable.throw(new HttpErrorResponse(parsedError));
            //});
    }
}