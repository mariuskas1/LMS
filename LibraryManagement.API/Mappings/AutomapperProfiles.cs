using AutoMapper;
using LibraryManagement.API.Models.Domain;
using LibraryManagement.API.Models.DTOs;

namespace LibraryManagement.API.Mappings;

public class AutomapperProfiles : Profile {
    
    public AutomapperProfiles() {
        CreateMap<OpenLibrarySubjectWorkDto, Book>().ReverseMap();
        CreateMap<OpenLibraryWorkAuthorDto, Author>().ReverseMap();
    }
}