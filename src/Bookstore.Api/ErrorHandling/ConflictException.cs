namespace Bookstore.Api.ErrorHandling;

public sealed class ConflictException(string message) : Exception(message);
