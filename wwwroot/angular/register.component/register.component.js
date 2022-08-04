// Define the register component
angular.
    module('register').
    component('register', {
        templateUrl: '../angular/register.component/register.template.html',
        controller: ['$http', function RegisterController($http) {
            
            var self = this;

            // Disable website background
            document.body.classList.remove('background');
            
            // Fetch registration data from database
            $http.get('/home/fetch').then((content) => {
                if (content.data !== null)
                {    
                    self.dataModel = content.data;
                }
            });
            
            // Create alert component
            self.makeAlertElement = (type, message) => {
                const alertPlaceholder = document.getElementById('alert-placeholder');
                const wrapper = document.createElement('div')

                wrapper.innerHTML = [
                `<div class="alert alert-${type} alert-dismissible" role="alert">`,
                `   <div>${message}</div>`,
                '   <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>',
                '</div>'
                ].join('')
            
                alertPlaceholder.append(wrapper)
            }

            // Save changes made to model
            self.update = () => {
                $http.post('/home/update', JSON.stringify(self.dataModel), {
                    headers: { "Content-Type": "application/json" }
                }).then((response) => {
                    self.makeAlertElement(response.data.status, response.data.message);
                });
            }
        }]
});