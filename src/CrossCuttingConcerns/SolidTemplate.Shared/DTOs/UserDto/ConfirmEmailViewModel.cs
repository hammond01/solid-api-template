﻿using System.ComponentModel.DataAnnotations;
namespace SolidTemplate.Shared.DTOs.UserDto;

public class ConfirmEmailDto
{
    [Required(ErrorMessage = "FieldRequired")]
    public string UserId { get; set; } = default!;

    [Required(ErrorMessage = "FieldRequired")]
    public string Token { get; set; } = default!;
}
