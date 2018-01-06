**MiniCSV: A Simple C# CSV Parser and Object Deserializer**

Alexander Shoulson, Ph.D. - http://ashoulson.com

---

[![Build status](https://ci.appveyor.com/api/projects/status/1dt5aepocun584g4/branch/master?svg=true)](https://ci.appveyor.com/project/ashoulson/miniudp/branch/master)

A simple library to deserialize CSV files into corresponding C# classes. Expects CSV files to conform to [RFC4180](https://tools.ietf.org/html/rfc4180) (tested against Excel and Google Sheets exported CSV files).

**Usage:**

Define a C# class with a tagged constructor, as follows:

```
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
```

Create/export a CSV with corresponding values. This CSV value *must* have a header row that matches the object's parameter names (ignoring case, whitespace, and underscores) in the correct order. This behavior can be altered with the `hasHeader` and `validateHeader` parameters.

```
Name,Age,Percent,Favorite Color,
Alice,32,96.4,Blue,
Bob,25,87.3,Red,(Any values after the parameter data columns are ignored)
Charlie,33,50.2,Green,
```

Then create both a parser and deserializer to parse and produce objects.

```
  List<TestData> results = new List<TestData>();
  using (StreamReader sr = File.OpenText(path))
  {
    CsvDeserializer<TestData> deserializer =
      new CsvDeserializer<TestData>(new CsvParser(sr), false, false);
    while (deserializer.TryProduce(out TestData output))
      results.Add(output);
  }
```
