using Core;

string xml = File.ReadAllText("../SampleData/clubs.xml");
Dictionary<string, string[]> values = new AttributeValueLoader().Load(xml);

foreach (string field in new[] { "Kind", "Faculty", "Day", "Course" })
    Console.WriteLine($"{field}: {string.Join(", ", values[field])}");

Console.WriteLine($"Department: {values["Department"].Length} значень");
Console.WriteLine($"Supervisor: {values["Supervisor"].Length} значень");