using AutoMapper;
using CapyBooks.Application.DTOs;
using CapyBooks.Domain.Entities;

namespace CapyBooks.Application.Mappings;

public class GenreProfile : Profile
{
    public GenreProfile()
    {
        CreateMap<Genre, GenreDto>();
    }
}
