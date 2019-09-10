import { NgModule, ModuleWithProviders } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { WebTypedNgInvoker } from './webTypedNgInvoker';

@NgModule({
    imports:[ HttpClientModule ]
})
export class WebTypedNgModule { 
    static forRoot(): ModuleWithProviders {
		return {
			ngModule: WebTypedNgModule,
			providers: [
                WebTypedEventEmitterService,
                WebTypedNgInvoker
			]
		};
	}
}
