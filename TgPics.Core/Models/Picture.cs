﻿using System.ComponentModel.DataAnnotations;

namespace TgPics.Core.Models;

public class Picture
{
    public Picture() { }

    public int Id { get; set; }
    public string Source { get; set; }
    public int Position { get; set; }
    public byte[]? Data { get; set; }
}