export class WebTypedUtils {
    private static addQueryParameterValue(path: string, val: any, result: { path: string, val: string }[]) {
        //undefined dont add the parameter to the query
        if (val === undefined) { return; }
        //null add parameter with empty value
        if (val === null) {
            result.push({ path, val: "" });
            return;
        }
        //arrays
        if (Array.isArray(val)) {
            val.forEach((item, index) => {
                WebTypedUtils.addQueryParameterValue(`${path}[${index}]`, item, result);
            });
            return;
        }
        //complex objects
        if (typeof val === "object") {
            WebTypedUtils.resolveQueryParameters(val, result, path);
            return;
        }
        //simple values
        result.push({ path, val });
    }
    public static resolveQueryParameters(obj: any, result?: { path: string, val: string }[], parentField?: string) {
        if (!result) { result = []; }
        for (let field in obj) {
            var val = obj[field];
            var pathElements = [];
            if (parentField) { pathElements.push(parentField); }
            pathElements.push(field);
            var path = pathElements.join('.');
            WebTypedUtils.addQueryParameterValue(path, val, result);
        }
        return result;

    }

    public static resolveQueryParametersString(obj: any): string{
        var params = WebTypedUtils.resolveQueryParameters(obj);
        var result = "";
        return params
            .map(p => `${p.path}=${p.val}`)
            .join('&');
    }
}