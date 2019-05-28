import * as extMdl0 from './cblxSignupResultModel';
import * as extMdl1 from './cblxSignupModel';
import { WebTypedCallInfo, WebTypedFunction } from '@guimabdo/webtyped-common';
import { Injectable, Inject, forwardRef, Optional } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { WebTypedClient, WebTypedEventEmitterService } from '@guimabdo/webtyped-angular';
import { Observable } from 'rxjs';
@Injectable()
export class CblxIdentityService extends WebTypedClient {
	constructor(@Optional() @Inject('API_BASE_URL') baseUrl: string, httpClient: HttpClient, @Inject(forwardRef(() => WebTypedEventEmitterService)) eventEmitter: WebTypedEventEmitterService) {
		super(baseUrl, 'cblx-identity', httpClient, eventEmitter);
	}
	signup: CblxIdentityService.SignupFunction = (model: extMdl1.CblxSignupModel) : Observable<extMdl0.CblxSignupResultModel> => {
		return this.invokePost({
				returnTypeName: 'extMdl0.CblxSignupResultModel',
				kind: 'Signup',
				func: this.signup,
				parameters: { model, _wtKind: 'Signup' }
			},
			`sign-up`,
			model,
			undefined
		);
	};
	resetPassword: CblxIdentityService.ResetPasswordFunction = (email: string) : Observable<void> => {
		return this.invokePost({
				returnTypeName: 'void',
				kind: 'ResetPassword',
				func: this.resetPassword,
				parameters: { email, _wtKind: 'ResetPassword' }
			},
			`password/reset`,
			email,
			undefined
		);
	};
}
export namespace CblxIdentityService {
	export type SignupParameters = {model: extMdl1.CblxSignupModel, _wtKind: 'Signup' };
	export interface SignupCallInfo extends WebTypedCallInfo<SignupParameters, extMdl0.CblxSignupResultModel> { kind: 'Signup'; }
	export type SignupFunctionBase = (model: extMdl1.CblxSignupModel) => Observable<extMdl0.CblxSignupResultModel>;
	export interface SignupFunction extends WebTypedFunction<SignupParameters, extMdl0.CblxSignupResultModel>, SignupFunctionBase {}
	export type ResetPasswordParameters = {email: string, _wtKind: 'ResetPassword' };
	export interface ResetPasswordCallInfo extends WebTypedCallInfo<ResetPasswordParameters, void> { kind: 'ResetPassword'; }
	export type ResetPasswordFunctionBase = (email: string) => Observable<void>;
	export interface ResetPasswordFunction extends WebTypedFunction<ResetPasswordParameters, void>, ResetPasswordFunctionBase {}
}
