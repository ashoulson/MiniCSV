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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using MiniCSV;

namespace Tests
{
  [TestClass]
  public class TestParser
  {
    [TestMethod]
    // Make sure we're getting the correct number of rows and columns
    public void TestEntryCounts()
    {
      const int ROWS = 5;
      const int COLUMNS = 4;

      List<IList<string>> values1 = ReadFile("testcsv_base.csv");
      Assert.IsTrue(values1.Count == ROWS);

      for (int i = 0; i < ROWS; i++)
        Assert.IsTrue(values1[i].Count == COLUMNS);       
    }

    [TestMethod]
    // Test to make sure there is exact parity when reading a file with and
    // without a new line at the end of the file (though there should be one)
    public void TestNoNewline()
    {
      List<IList<string>> values1 = ReadFile("testcsv_base.csv");
      List<IList<string>> values2 = ReadFile("testcsv_noNewline.csv");

      Assert.IsTrue(values1.Count == values2.Count);
      for (int i = 0; i < values1.Count; i++)
      {
        Assert.IsTrue(values1[i].Count == values2[i].Count);
        for (int j = 0; j < values1[i].Count; j++)
          Assert.IsTrue(values1[i][j] == values2[i][j]);
      }
    }

    [TestMethod]
    // Explicitly tests the values of the base CSV test file against expected
    public void TestValuesExplicitly()
    {
      List<IList<string>> values1 = ReadFile("testcsv_base.csv");

      Assert.IsTrue(values1[0][0] == "one");
      Assert.IsTrue(values1[0][1] == "2");
      Assert.IsTrue(values1[0][2] == "three");
      Assert.IsTrue(values1[0][3] == "");

      Assert.IsTrue(values1[1][0] == "one");
      Assert.IsTrue(values1[1][1] == "a \"quoted\" thing");
      Assert.IsTrue(values1[1][2] == "end");
      Assert.IsTrue(values1[1][3] == "");

      Assert.IsTrue(values1[2][0] == "one");
      Assert.IsTrue(values1[2][1] == "two,\nthree");
      Assert.IsTrue(values1[2][2] == "four");
      Assert.IsTrue(values1[2][3] == "");

      Assert.IsTrue(values1[3][0] == "one");
      Assert.IsTrue(values1[3][1] == "two\"three");
      Assert.IsTrue(values1[3][2] == "four");
      Assert.IsTrue(values1[3][3] == "");

      Assert.IsTrue(values1[4][0] == "one");
      Assert.IsTrue(values1[4][1] == "two \"three\", four");
      Assert.IsTrue(values1[4][2] == "five");
      Assert.IsTrue(values1[4][3] == "");
    }

    private List<IList<string>> ReadFile(string fileName)
    {
      string filePath = 
        Path.Combine(Directory.GetCurrentDirectory(), fileName);

      List<IList<string>> values = new List<IList<string>>();
      using (StreamReader sr = File.OpenText(filePath))
      {
        CsvParser parser = new CsvParser(sr);
        while (parser.TryReadRow(out IList<string> result))
          values.Add(result);
      }

      return values;
    }
  }
}
