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

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;

namespace MiniCSV
{
  public class CsvDeserializer<TObject>
    where TObject : class
  {
    private readonly CsvParser parser;
    private readonly ConstructorInfo constructor;
    private readonly ParameterInfo[] parameters;

    public CsvDeserializer(
      CsvParser parser, 
      bool hasHeader = true, 
      bool validateHeader = true)
    {
      this.parser = parser ?? throw new ArgumentNullException("parser");

      this.constructor = this.FindConstructor();
      this.parameters = this.constructor.GetParameters();

      if (hasHeader)
        this.ReadHeader(validateHeader);
    }

    /// <summary>
    /// Produces a C# object from the next row in the CSV file.
    /// </summary>
    public TObject Produce()
    {
      IList<string> entries = this.parser.ReadRow();
      if (entries == null)
        return null;

      if (entries.Count < this.parameters.Length)
        throw new CsvDeserializeException(
          "Csv has fewer rows than required to deserialize: " +
          typeof(TObject).ToString());

      object[] values = new object[this.parameters.Length];
      for (int i = 0; i < this.parameters.Length; i++)
      {
        ParameterInfo paramInfo = this.parameters[i];

        try
        {
          if (paramInfo.ParameterType.IsEnum)
            values[i] = Enum.Parse(paramInfo.ParameterType, entries[i]);
          else
            values[i] = Convert.ChangeType(entries[i], paramInfo.ParameterType);
        }
        catch (Exception inner)
        {
          throw new CsvDeserializeException(
            "Parsing failed for object " + typeof(TObject) +
            " on row " + parser.CurrentRow, 
            inner);
        }
      }

      return (TObject)this.constructor.Invoke(values);
    }

    /// <summary>
    /// Tries to produce an object, returns true iff successful.
    /// </summary>
    public bool TryProduce(out TObject result)
    {
      result = this.Produce();
      return (result != null);
    }

    private void ReadHeader(bool validateHeader)
    {
      IList<string> header = this.parser.ReadRow();
      if (validateHeader == false)
        return;

      // Validate that we have a proper header
      if (header == null)
        throw new CsvDeserializeException(
          "Csv has no header data for: " +
          typeof(TObject).ToString());

      // Validate parameter count (CSV can have extra columns)
      if (header.Count < this.parameters.Length)
        throw new CsvDeserializeException(
          "Csv has fewer parameters in header than required for: " +
          typeof(TObject).ToString());

      // Validate names (case-insensitive)
      for (int i = 0; i < this.parameters.Length; i++)
      {
        string dataName = Regex.Replace(header[i].ToLower(), @"(\s|_)+", "");
        string paramName = Regex.Replace(this.parameters[i].Name.ToLower(), @"(\s|_)+", "");

        if (dataName != paramName)
          throw new CsvDeserializeException(
            "Csv header does not match constructor parameter names for: " +
            typeof(TObject).ToString());
      }
    }

    /// <summary>
    /// Finds the constructor on our object type that has a CsvConstructor
    /// attribute. We expect to find exactly one such constructor.
    /// </summary>
    private ConstructorInfo FindConstructor()
    {
      ConstructorInfo[] constructors = typeof(TObject).GetConstructors();
      ConstructorInfo found = null;

      for (int i = 0; i < constructors.Length; i++)
      {
        if (constructors[i].GetCustomAttribute<CsvConstructorAttribute>() != null)
        {
          if (found != null)
            throw new CsvDeserializeException(
              "Type has multiple CsvConstructor attributes: " + 
              typeof(TObject).ToString());
          found = constructors[i];
        }
      }

      if (found == null)
        throw new CsvDeserializeException(
          "Type has no valid constructor with a CsvConstructor attribute: " +
          typeof(TObject).ToString());
      return found;
    }
  }
}
