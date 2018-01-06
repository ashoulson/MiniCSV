/*
 *  MiniCSV - A Simple C# CSV Parser and Object Deserializer
 *  Copyright (c) 2018 - Alexander Shoulson - http://ashoulson.com
 *
 *  This software is provided 'as-is', without any express or implied
 *  warranty. In no event will the authors be held liable for any damages
 *  arising from the use of this software.
 *  Permission is granted to anyone to use this software for any purpose,
 *  including commercial applications, and to alter it and redistribute it
 *  freely, subject to the following restrictions:
 *  
 *  1. The origin of this software must not be misrepresented; you must not
 *     claim that you wrote the original software. If you use this software
 *     in a product, an acknowledgment in the product documentation would be
 *     appreciated but is not required.
 *  2. Altered source versions must be plainly marked as such, and must not be
 *     misrepresented as being the original software.
 *  3. This notice may not be removed or altered from any source distribution.
 */

using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MiniCSV
{
  public class CsvParser
  {
    public int CurrentRow { get { return this.rowCount; } }

    private readonly TextReader reader;
    private readonly StringBuilder builder;
    
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

    /// <summary>
    /// Reads the next row in the CSV file and returns it as a list of strings.
    /// </summary>
    public IList<string> ReadRow()
    {
      this.Reset();

      IList<string> result = null;
      bool complete = false;

      while ((complete == false) && (reader.Peek() != -1))
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
          complete = true;
      }

      if (this.inBlock)
        throw new CsvFormatException("CSV ends with EOF within quote block");

      // Consume whatever is left over, if anything
      if (this.builder.Length > 0)
        this.Record(result);
      // Special case for EOF with no newline
      else if ((result != null) && (complete == false) && (reader.Peek() == -1))
        this.Record(result);

      this.rowCount++;
      return result;
    }

    /// <summary>
    /// Tries to read a row, returns true iff successful.
    /// </summary>
    public bool TryReadRow(out IList<string> result)
    {
      result = this.ReadRow();
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

      // Read special characters
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
