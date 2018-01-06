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

namespace MiniCSV.Tests
{
  public class TestCommon
  {
    public static string GetPath(string fileName)
    {
      return Path.Combine(Directory.GetCurrentDirectory(), "Csv", fileName);
    }

    public static List<IList<string>> ReadFile(string fileName)
    {
      List<IList<string>> values = new List<IList<string>>();
      using (StreamReader sr = File.OpenText(TestCommon.GetPath(fileName)))
      {
        CsvParser parser = new CsvParser(sr);
        while (parser.TryReadRow(out IList<string> result))
          values.Add(result);
      }

      return values;
    }
  }
}
