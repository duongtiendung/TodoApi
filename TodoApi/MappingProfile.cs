using AutoMapper;
using TodoApi.DTOs;
using TodoApi.Models;

namespace TodoApi
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TodoRequest, Todo>();
        }
    }
}
