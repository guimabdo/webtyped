import { Injectable, Inject, forwardRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { WebApiClient, WebApiEventEmmiterService } from '@guimabdo/webtyped-angular';
import { Observable } from 'rxjs';
@Injectable()
export class MegaSampleService extends WebApiClient {
	constructor(@Inject('API_BASE_URL') baseUrl: string, httpClient: HttpClient, @Inject(forwardRef(() => WebApiEventEmmiterService)) eventEmmiter: WebApiEventEmmiterService) {
		super(baseUrl, "api/MegaSample", httpClient, eventEmmiter);
	}
	GetThisStringFromQuery = (str: string) : Observable<string> => {
		return this.invokeGet<string>({
				func: this.GetThisStringFromQuery,
				parameters: { str }
			},
			``,
			{ str }
		);
	};
	GetThisStringFromQueryExplicit = (str: string) : Observable<string> => {
		return this.invokeGet<string>({
				func: this.GetThisStringFromQueryExplicit,
				parameters: { str }
			},
			`explicit`,
			{ str }
		);
	};
	GetThisStringFromRoute = (str: string) : Observable<string> => {
		return this.invokeGet<string>({
				func: this.GetThisStringFromRoute,
				parameters: { str }
			},
			`route/${str}`,
			undefined
		);
	};
	GetTheseStrings = (str: string, str2: string) : Observable<Array<string>> => {
		return this.invokeGet<Array<string>>({
				func: this.GetTheseStrings,
				parameters: { str, str2 }
			},
			`these/${str}`,
			{ str2 }
		);
	};
	PostAndReturnThisStringFromQuery = (str: string) : Observable<string> => {
		return this.invokePost<string>({
				func: this.PostAndReturnThisStringFromQuery,
				parameters: { str }
			},
			``,
			null,
			{ str }
		);
	};
	PostAndReturnThisStringFromQueryExplicit = (str: string) : Observable<string> => {
		return this.invokePost<string>({
				func: this.PostAndReturnThisStringFromQueryExplicit,
				parameters: { str }
			},
			`explicit`,
			null,
			{ str }
		);
	};
	PostAndReturnThisStringFromRoute = (str: string) : Observable<string> => {
		return this.invokePost<string>({
				func: this.PostAndReturnThisStringFromRoute,
				parameters: { str }
			},
			`${str}`,
			null,
			undefined
		);
	};
	PostAndReturnTheseStrings = (str: string, str2: string, str3: string) : Observable<Array<string>> => {
		return this.invokePost<Array<string>>({
				func: this.PostAndReturnTheseStrings,
				parameters: { str, str2, str3 }
			},
			`these/${str}`,
			str3,
			{ str2 }
		);
	};
	PostAndReturnModel = (model: UnitTest.MegaSampleService.Model) : Observable<UnitTest.MegaSampleService.Model> => {
		return this.invokePost<UnitTest.MegaSampleService.Model>({
				func: this.PostAndReturnModel,
				parameters: { model }
			},
			`model`,
			model,
			undefined
		);
	};
	PostAndReturnModelSameName = (model: UnitTest.ModelA) : Observable<UnitTest.ModelA> => {
		return this.invokePost<UnitTest.ModelA>({
				func: this.PostAndReturnModelSameName,
				parameters: { model }
			},
			`modelA1`,
			model,
			undefined
		);
	};
	PostAndReturnModelSameName2 = (model: UnitTest.OtherModels.ModelA) : Observable<UnitTest.OtherModels.ModelA> => {
		return this.invokePost<UnitTest.OtherModels.ModelA>({
				func: this.PostAndReturnModelSameName2,
				parameters: { model }
			},
			`modelA2`,
			model,
			undefined
		);
	};
	PostAndReturnTuple_NotWorkingYet = (tuple: {str: string, number: number}) : Observable<{str: string, number: number}> => {
		return this.invokePost<{str: string, number: number}>({
				func: this.PostAndReturnTuple_NotWorkingYet,
				parameters: { tuple }
			},
			`tuple`,
			tuple,
			undefined
		);
	};
}
