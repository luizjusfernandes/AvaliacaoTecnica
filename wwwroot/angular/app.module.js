var registrationApp = angular.module('registrationApp', ['ngRoute', 'login', 'thanks', 'register']);

// Make controller for navbar
registrationApp.controller('NavController', ['$http', ($http) => {

    $http.get('/home/isauthenticated').then((response) => {
        let isAuthenticated = response.data;
        const logout = document.getElementById('log-out');
    
        // Hide logout option if user is not registered
        if (isAuthenticated) {
            logout.classList.remove('hidden');
        }
        else {
            logout.classList.add('hidden');
        }
    })

}]);