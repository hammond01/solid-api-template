﻿using System.ComponentModel.DataAnnotations;
namespace SolidTemplate.Shared.DTOs.UserDto;

public class LoginRequest : AccountFormModel
{
    public required string UserName { get; set; }

    [DataType(DataType.Password)]
    public required string Password { get; set; }
}