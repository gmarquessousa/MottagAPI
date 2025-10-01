using App.Application.DTOs.Motos;
using App.Application.DTOs.Patios;
using App.Application.DTOs.Tags;
using App.Domain.Entities;
using AutoMapper;

namespace App.Application.Mappings;

public class ApplicationProfile : Profile
{
    public ApplicationProfile()
    {
        // Patio
        CreateMap<CreatePatioDto, Patio>();
        CreateMap<UpdatePatioDto, Patio>();
        CreateMap<Patio, PatioReadDto>();

        // Moto
        CreateMap<CreateMotoDto, Moto>()
            .ForMember(d => d.Status, opt => opt.MapFrom(src => src.Status ?? Domain.Enums.MotoStatus.Disponivel));
        CreateMap<UpdateMotoDto, Moto>()
            .ForMember(d => d.Placa, opt => opt.Ignore())
            .ForMember(d => d.PatioId, opt => opt.Ignore());
        CreateMap<Moto, MotoReadDto>();

        // Tag
        CreateMap<CreateTagDto, Tag>();
        CreateMap<UpdateTagDto, Tag>()
            .ForMember(d => d.Serial, opt => opt.Ignore());
        CreateMap<Tag, TagReadDto>();

    }
}
