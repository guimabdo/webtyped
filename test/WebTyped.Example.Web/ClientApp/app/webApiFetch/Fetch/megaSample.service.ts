//*Generated with WebTyped*
import { WebApiClient } from '@guimabdo/webtyped-fetch';
export class MegaSampleService extends WebApiClient {
	constructor(baseUrl: string = WebApiClient.baseUrl) {
		super(baseUrl, "api/MegaSample");
	}
	GetThisStringFromQuery = (str: string) : Promise<string> => {
		return this.invokeGet<string>({
				func: this.GetThisStringFromQuery,
				parameters: { str }
			},
			``,
			{ str }
		);
	};
	GetThisStringFromQueryExplicit = (str: string) : Promise<string> => {
		return this.invokeGet<string>({
				func: this.GetThisStringFromQueryExplicit,
				parameters: { str }
			},
			`explicit`,
			{ str }
		);
	};
	GetThisStringFromRoute = (str: string) : Promise<string> => {
		return this.invokeGet<string>({
				func: this.GetThisStringFromRoute,
				parameters: { str }
			},
			`route/${str}`,
			undefined
		);
	};
	GetTheseStrings = (str: string, str2: string) : Promise<Array<string>> => {
		return this.invokeGet<Array<string>>({
				func: this.GetTheseStrings,
				parameters: { str, str2 }
			},
			`these/${str}`,
			{ str2 }
		);
	};
	PostAndReturnThisStringFromQuery = (str: string) : Promise<string> => {
		return this.invokePost<string>({
				func: this.PostAndReturnThisStringFromQuery,
				parameters: { str }
			},
			``,
			null,
			{ str }
		);
	};
	PostAndReturnThisStringFromQueryExplicit = (str: string) : Promise<string> => {
		return this.invokePost<string>({
				func: this.PostAndReturnThisStringFromQueryExplicit,
				parameters: { str }
			},
			`explicit`,
			null,
			{ str }
		);
	};
	PostAndReturnThisStringFromRoute = (str: string) : Promise<string> => {
		return this.invokePost<string>({
				func: this.PostAndReturnThisStringFromRoute,
				parameters: { str }
			},
			`${str}`,
			null,
			undefined
		);
	};
	PostAndReturnTheseStrings = (str: string, str2: string, str3: string) : Promise<Array<string>> => {
		return this.invokePost<Array<string>>({
				func: this.PostAndReturnTheseStrings,
				parameters: { str, str2, str3 }
			},
			`these/${str}`,
			str3,
			{ str2 }
		);
	};
	PostAndReturnModel = (model: Fetch.MegaSampleService.Model) : Promise<Fetch.MegaSampleService.Model> => {
		return this.invokePost<Fetch.MegaSampleService.Model>({
				func: this.PostAndReturnModel,
				parameters: { model }
			},
			`model`,
			model,
			undefined
		);
	};
	PostAndReturnModelSameName = (model: Fetch.ModelA) : Promise<Fetch.ModelA> => {
		return this.invokePost<Fetch.ModelA>({
				func: this.PostAndReturnModelSameName,
				parameters: { model }
			},
			`modelA1`,
			model,
			undefined
		);
	};
	PostAndReturnModelSameName2 = (model: Fetch.OtherModels.ModelA) : Promise<Fetch.OtherModels.ModelA> => {
		return this.invokePost<Fetch.OtherModels.ModelA>({
				func: this.PostAndReturnModelSameName2,
				parameters: { model }
			},
			`modelA2`,
			model,
			undefined
		);
	};
	PostAndReturnTuple_NotWorkingYet = (tuple: {str: string, number: number}) : Promise<{str: string, number: number}> => {
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
