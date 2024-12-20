﻿namespace SolidTemplate.Shared.DTOs.EmailDto;

public class EmailAddressDto
{
    private string _name;

    public EmailAddressDto(string name, string address)
    {
        _name = name;
        Address = address;
    }

    public string Name
    {
        get
        {
            if (string.IsNullOrEmpty(_name))
            {
                return Address;
            }
            return _name;
        }
        set => _name = value;
    }
    public string Address { get; set; }
}
