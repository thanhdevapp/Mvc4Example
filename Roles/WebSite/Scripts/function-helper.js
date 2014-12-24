function EditParametersUrl(url, param, newvalue) {
    /*
     * queryParameters -> handles the query string parameters
     * queryString -> the query string without the fist '?' character
     * re -> the regular expression
     * m -> holds the string matching the regular expression
     */
    var queryParameters = {}, queryString = url,
        re = /([^&=]+)=([^&]*)/g, m;

    // Creates a map with the query string parameters
    while (m = re.exec(queryString)) {
        queryParameters[decodeURIComponent(m[1])] = decodeURIComponent(m[2]);
    }
    // Add new parameters or update existing ones
    queryParameters[param] = newvalue;
    /*
     * Replace the query portion of the URL.
     * jQuery.param() -> create a serialized representation of an array or
     *     object, suitable for use in a URL query string or Ajax request.
     */
    return $.param(queryParameters);
}
function ArrayToUrl(queryParameters) {
    return $.param(queryParameters);
}


function getUrlParameter(sParam) {
    var sPageURL = window.location.search.substring(1);
    var sURLVariables = sPageURL.split('&');
    for (var i = 0; i < sURLVariables.length; i++) {
        var sParameterName = sURLVariables[i].split('=');
        if (sParameterName[0] == sParam) {
            return sParameterName[1];
        } else {
            return null;
        }
    }
    return null;
}