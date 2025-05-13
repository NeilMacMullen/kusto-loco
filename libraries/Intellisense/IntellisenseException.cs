namespace Intellisense;

public class IntellisenseException(Exception exception) : Exception("Exception in intellisense service", exception);
