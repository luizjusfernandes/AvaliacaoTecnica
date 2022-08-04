// Define the login component
angular.
    module('thanks').
    component('thanks', {
        templateUrl: '../angular/thanks.component/thanks.template.html',
        controller: function LoginController() {
            // Enable website background
            document.body.classList.add('background');
        }
});