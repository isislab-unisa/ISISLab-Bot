namespace ISISLab.HelpDesk.Common
{
  using System;
  using System.Text;
  using System.Text.RegularExpressions;

  public class DescriptionParser
  {
    public string Name { get; internal set; }
    public string Title { get; internal set; }
    public string Summary { get; internal set; }
    public string Speakers { get; internal set; }
    public string Language { get; internal set; }
    public string ImageUrl { get; internal set; }

    internal DescriptionParser()
    { }

    public static DescriptionParser Parse(string description)
    {
      var cleanedDescription = description.Replace("<br>", "\n");
      cleanedDescription = Regex.Replace(cleanedDescription, @"<[^>]*>", "");

      var splitted = cleanedDescription.Split('\n');
      if (splitted.Length < 6) throw new Exception("Unable to parse description. Wrong format");

      return new DescriptionParser
      {
        Name      = parseItem(splitted[0]),
        Title     = parseItem(splitted[1]),
        Summary   = parseItem(splitted[2]),
        Speakers  = parseItem(splitted[3]),
        Language  = parseItem(splitted[4]),
        ImageUrl  = parseItem(splitted[5])
      };

      string parseItem(string item)
      {
        var startIndex = item.IndexOf(':') + 1;
        return item.Substring(startIndex);
      }
    }

    public DescriptionParser WithName(string name)
    {
      Name = name.RemoveNewLine();
      return this;
    }

    public DescriptionParser WithTitle(string title)
    {
      Title = title.RemoveNewLine();
      return this;
    }

    public DescriptionParser WithSummary(string summary)
    {
      Summary = summary.RemoveNewLine();
      return this;
    }

    public DescriptionParser WithSpeakers(string speakers)
    {
      Speakers = speakers.RemoveNewLine();
      return this;
    }

    public DescriptionParser WithLanguage(string language)
    {
      Language = language.RemoveNewLine();
      return this;
    }

    public DescriptionParser WithImageUrl(string imageUrl)
    {
      ImageUrl = imageUrl.RemoveNewLine();
      return this;
    }

    public override string ToString()
    {
      var stringBuilder = new StringBuilder()
        .Append($"Name: {Name}\n")
        .Append($"Title: {Title}\n")
        .Append($"Summary: {Summary}\n")
        .Append($"Speakers: {Speakers}\n")
        .Append($"Language: {Language}\n")
        .Append($"Image Url: {ImageUrl}");

      return stringBuilder.ToString();
    }
  }
}
