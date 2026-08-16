namespace Bookstore.Api.ErrorHandling;

public sealed class NotFoundException(string message) : Exception(message);
