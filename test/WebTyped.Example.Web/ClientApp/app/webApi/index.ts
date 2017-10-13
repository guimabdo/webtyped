import * as services from './services';
import { WebApiEventEmmiterService } from '@guimabdo/webtyped-angular';
export var providers = [
	WebApiEventEmmiterService,
	...services.all
];
