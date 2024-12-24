using Api.DTO.Requests;
using Api.DTO.Responses;
using AutoMapper;
using Domain.Models;

namespace Api;

public class AutoMappingProfile : Profile
{
    public AutoMappingProfile()
    {
        CreateMap<UserAuthRequest, UserCredentials>();
        CreateMap<UserRegisterRequest, User>();
        CreateMap<UpdateUserPersonalDataRequest, PersonalData>();

        CreateMap<UserAuth, UserAuthResponse>();
    }
}