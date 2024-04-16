using Newtonsoft.Json;
using System.Xml.Serialization;

public class ColiboModel
{
    public XmlInfo? Xml { get; set; }
    public Data? Data { get; set; }
}

public class XmlInfo
{
    public string? Version { get; set; }
    public string? Encoding { get; set; }
}

public class Data
{
    public Persons? Persons { get; set; }
}

public class Persons
{
    public List<Person>? Person { get; set; }
}

public class Person
{
    [JsonProperty("@number")]
    public string? Number { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Mobile { get; set; }
    public string? Title { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
}
