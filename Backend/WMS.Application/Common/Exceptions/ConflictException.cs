namespace WMS.Application.Common.Exceptions
{
    public class ConflictException : Exception
    {
        public ConflictException()
            : base("A conflict occurred with the current state of the resource.")
        {
        }

        public ConflictException(string message)
            : base(message)
        {
        }

        public ConflictException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ConflictException(string name, object key)
            : base($"Entity \"{name}\" ({key}) already exists and caused a conflict.")
        {
        }
    }
}
