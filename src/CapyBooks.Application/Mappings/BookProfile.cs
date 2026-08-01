using AutoMapper;
using CapyBooks.Application.DTOs;
using CapyBooks.Domain.Entities;

namespace CapyBooks.Application.Mappings;

public class BookProfile : Profile
{
    public BookProfile()
    {
        CreateMap<Book, BookDto>();
    }
}
