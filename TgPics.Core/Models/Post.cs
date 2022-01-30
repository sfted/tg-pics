﻿namespace TgPics.Core.Models;

public class Post
{
    public Post() { }
    public int Id { get; set; }
    public string SourceLink { get; set; }
    public string SourceTitle { get; set; }
    public DateTime? PublicationDateTime { get; set; }
    public string? Comment { get; set; }
    public IEnumerable<Picture> Pictures { get; set; }
}
