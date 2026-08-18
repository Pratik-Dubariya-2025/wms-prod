using AutoMapper;
using WMS.Application.Features.Users.DTOs;
using WMS.Domain.Models;

namespace WMS.Application.Features.Users.Mappings
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<User, UserListDto>()
                .ForMember(d => d.DepartmentName, opt => opt.MapFrom(s => s.Department.Name))
                .ForMember(d => d.DesignationName, opt => opt.MapFrom(s => s.Designation.Name))
                .ForMember(d => d.Roles, opt => opt.MapFrom(s =>
                    s.UserRoles.Select(ur => ur.Role.Name).ToList()));

            CreateMap<User, UserDetailDto>()
                .ForMember(d => d.DepartmentName, opt => opt.MapFrom(s => s.Department.Name))
                .ForMember(d => d.DesignationName, opt => opt.MapFrom(s => s.Designation.Name))
                .ForMember(d => d.Roles, opt => opt.MapFrom(s =>
                    s.UserRoles.Select(ur => new UserRoleDto
                    {
                        RoleId = ur.RoleId,
                        RoleName = ur.Role.Name
                    }).ToList()));
        }
    }
}
