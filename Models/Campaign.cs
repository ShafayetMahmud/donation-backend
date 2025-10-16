// Models/Campaign.cs
public class Campaign
{
    public string? Id { get; set; }  // GUID
    public string Name { get; set; }
    public string Why { get; set; }
    public string Goals { get; set; }
    public string Method { get; set; }
    public string Quote { get; set; }
    public string Missionquote { get; set; }
    public string Descriptionone { get; set; }
    public string Descriptiontwo { get; set; }
    public string Descriptionthree { get; set; }
    public string Descriptionfour { get; set; }
    public string WhatFor { get; set; }
    public string How { get; set; }
    public string Contact { get; set; }
    public List<string> Gallery { get; set; } = new List<string>();
    public string? Subdomain { get; set; }  // e.g., save-the-forest
}
