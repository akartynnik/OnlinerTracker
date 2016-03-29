﻿'use strict';
app.factory('productsService', ['$http', '$q', '$location', 'ngAuthSettings', function ($http, $q, $location, ngAuthSettings) {
    var serviceBase = ngAuthSettings.apiServiceBaseUri;
    var productsServiceFactory = {};

    var _getProducts = function () {

        return $http.get(serviceBase + 'api/product/getAll').then(function (results) {
            return results;
        });
    };

    productsServiceFactory.getProducts = _getProducts;
    return productsServiceFactory;
}]);