//It's not being used for real unit tests yet, just for dev checks/sandbox
//mocha -r ts-node/register tests/general.ts
import { expect } from 'chai';
import 'mocha';
import * as WebTypedCommon from '../common/index';


describe('some func', () => {

    it('should return something', () => {
        var result = WebTypedCommon.WebTypedUtils.resolveQueryParameters({
            a: "hello",
            b: "world",
            c: [1, 2, 3],
            d: { x: 1, y: true, z: "dontknow" }
        });
        console.log(result);
        expect(result[0].val).to.equal('???');
    });

});
