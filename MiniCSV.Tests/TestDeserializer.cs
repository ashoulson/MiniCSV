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

namespace MiniCSV.Tests
{
  internal enum Color
  {
    Red,
    Green,
    Blue,
    Yellow,
    Orange,
    Pink,
    Purple,
  }

  internal class TestData
  {
    public string Name { get; }
    public int Age { get; }
    public float Percent { get; }
    public Color FavoriteColor { get; }

    [CsvConstructor]
    public TestData(
      string name, 
      int age, 
      float percent, 
      Color favoriteColor)
    {
      this.Name = name;
      this.Age = age;
      this.Percent = percent;
      this.FavoriteColor = favoriteColor;
    }
  }

  [TestClass]
  public class TestDeserializer
  {
    [TestMethod]
    // Make sure we're getting the correct number of rows and columns
    public void TestDeserialize()
    {
      string path = TestCommon.GetPath("testcsv_deserialize.csv");
      List<TestData> results = new List<TestData>();

      using (StreamReader sr = File.OpenText(path))
      {
        CsvDeserializer<TestData> deserializer = 
          new CsvDeserializer<TestData>(new CsvParser(sr));
        while (deserializer.TryProduce(out TestData output))
          results.Add(output);
      }

      Assert.IsTrue(results.Count == 3);

      Assert.IsTrue(results[0].Name == "Alice");
      Assert.IsTrue(results[0].Age == 32);
      Assert.IsTrue(results[0].Percent == 96.4f);
      Assert.IsTrue(results[0].FavoriteColor == Color.Blue);

      Assert.IsTrue(results[1].Name == "Bob");
      Assert.IsTrue(results[1].Age == 25);
      Assert.IsTrue(results[1].Percent == 87.3f);
      Assert.IsTrue(results[1].FavoriteColor == Color.Red);

      Assert.IsTrue(results[2].Name == "Charlie");
      Assert.IsTrue(results[2].Age == 33);
      Assert.IsTrue(results[2].Percent == 50.2f);
      Assert.IsTrue(results[2].FavoriteColor == Color.Green);
    }
  }
}
