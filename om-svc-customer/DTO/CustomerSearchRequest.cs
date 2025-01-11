namespace om_svc_customer.DTO
{
    public class CustomerSearchRequest
    {
        private string _firstName; 
        private string _lastName; 
        private string _email; 
        private string _phone; 
        public string FirstName { get { return _firstName; } set { _firstName = value?.ToLower(); } } 
        public string LastName { get { return _lastName; } set { _lastName = value?.ToLower(); } } 
        public string Email { get { return _email; } set { _email = value?.ToLower(); } } 
        public string Phone { get { return _phone; } set { _phone = value?.ToLower(); } }
    }
}
