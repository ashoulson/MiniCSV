using System;

public class CsvFormatException : Exception
{
  public CsvFormatException() { }
  public CsvFormatException(string message) : base(message) { }
  public CsvFormatException(string message, Exception inner) : base(message, inner) { }
}