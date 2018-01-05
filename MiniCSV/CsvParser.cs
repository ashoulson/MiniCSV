using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MiniCSV
{
  public class CsvParser
  {
    // The separator/delimeter character
    private char separator;
    // The quote/block container character
    private char quote;
    // The current row we're parsing (used for debug output)
    private int rowCount;

    // We're in the very beginning of a new record
    private bool recordStart;
    // We're in a delimited block
    private bool inBlock;
    // We've seen a potential block end but need to verify
    private bool isBlockEnding;

    private readonly TextReader reader;
    private readonly StringBuilder builder;

    public CsvParser(
      TextReader reader, 
      char separator = ',', 
      char quote = '"')
    {
      this.reader = reader;
      this.builder = new StringBuilder();

      this.separator = separator;
      this.quote = quote;
      this.rowCount = 0;

      this.Reset();
    }

    public bool TryReadRow(out IList<string> result)
    {
      this.Reset();
      result = null;

      while (reader.Peek() != -1)
      {
        if (result == null)
          result = new List<string>();
        char current = (char)reader.Read();

        // Ignore carriage returns, use \n only.
        if (current == '\r')
          continue;

        if (current == this.quote)
          this.ReadQuote(result);
        else if (this.ReadOther(result, current))
          break;
      }

      if (this.inBlock)
        throw new CsvFormatException("CSV ends with EOF within quote block");

      // Consume whatever is left over, if anything
      if (this.builder.Length > 0)
        this.Record(result);
      this.rowCount++;
      return (result != null);
    }

    private void ReadQuote(IList<string> result)
    {
      if (this.inBlock)
      {
        if (this.isBlockEnding)
        {
          this.isBlockEnding = false;
          this.Consume(result, this.quote);
        }
        else
        {
          this.isBlockEnding = true;
        }
      }
      else
      {
        if (this.recordStart)
        {
          this.inBlock = true;
        }
        else
        {
          this.builder.Append(this.quote);
        }
      }
    }

    /// <summary>
    /// Returns true iff this is the end of the row.
    /// </summary>
    private bool ReadOther(IList<string> result, char current)
    {
      if (this.inBlock)
      {
        if (this.isBlockEnding)
        {
          this.inBlock = false;
          this.isBlockEnding = false;

          // Quoted blocks can only end on a separator
          if (current != this.separator)
            throw new CsvFormatException(
              "Row ends quoted block prematurely: " + 
              this.rowCount);

          // We know we have a separator, so record the entry
          this.Record(result);
          return false;
        }
        else
        {
          this.Consume(result, current);
          return false;
        }
      }

      if (current == '\n')
      {
        this.Record(result);
        return true;
      }

      if (current == this.separator)
      {
        this.Record(result);
        return false;
      }

      this.Consume(result, current);
      return false;
    }

    private void Reset()
    {
      this.builder.Clear();

      this.recordStart = true;
      this.inBlock = false;
      this.isBlockEnding = false;
    }

    private void Record(IList<string> result)
    {
      result.Add(this.builder.ToString());
      this.Reset();
    }

    private void Consume(IList<string> result, char next)
    {
      this.recordStart = false;
      this.builder.Append(next);
    }
  }
}
