using AutoMapper;
using CapyBooks.Application.DTOs;
using CapyBooks.Domain.Entities;

namespace CapyBooks.Application.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>();
    }
}
