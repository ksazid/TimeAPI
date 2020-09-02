namespace TimeAPI.API
{
    internal class ErrorModel
    {
        public ErrorModel()
        {
        }

        public int StatusCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}