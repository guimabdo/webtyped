//*Generated with WebTyped*
import { NgModule, ModuleWithProviders } from '@angular/core';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { WebTypedEventEmitterService, WebTypedInterceptor } from '@guimabdo/webtyped-angular';
import * as mdl0 from './angular'
export var serviceTypes = [
	mdl0.MegaSampleService,
	mdl0.SampleDataService
]
@NgModule({
	imports: [ HttpClientModule ]
})
export class WebTypedGeneratedModule {
	static forRoot(): ModuleWithProviders {
		return {
			ngModule: WebTypedGeneratedModule,
            providers: [
                 {
                    provide: HTTP_INTERCEPTORS,
                    useClass: WebTypedInterceptor,
                    multi: true,
                },
				WebTypedEventEmitterService,
				...serviceTypes
			]
		};
	}
}
