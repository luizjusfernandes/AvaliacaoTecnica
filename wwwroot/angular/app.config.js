angular.module('registrationApp').
config(['$routeProvider', function config($routeProvider) {
    $routeProvider.when('/', {
        template: '<thanks></thanks>'
    }).when('/register', {
        template: '<register></register>'
    }).otherwise('/');
}]);