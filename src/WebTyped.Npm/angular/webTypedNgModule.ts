import { NgModule, ModuleWithProviders } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { WebTypedNgInvoker } from './webTypedNgInvoker';
import { WebTypedEventEmitterService } from './webTypedEventEmitter.service';
import { WebTypedInvoker } from '@guimabdo/webtyped-common';

@NgModule({
    imports: [HttpClientModule],
    exports: [HttpClientModule]
    
})
export class WebTypedNgModule { 
    static forRoot(): ModuleWithProviders {
		return {
			ngModule: WebTypedNgModule,
			providers: [
                WebTypedEventEmitterService,
                {
                    provide: WebTypedInvoker,
                    useClass: WebTypedNgInvoker
                }
			]
		};
	}
}
