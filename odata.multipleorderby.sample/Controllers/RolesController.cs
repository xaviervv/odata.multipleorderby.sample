using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.AspNet.OData;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace OData.MultipleOrderBy.Sample.Controllers
{
    public class RolesDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<RolesUserDto> Users { get; set; }
    }

    public class RolesUserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class RolesProfile : Profile
    {
        public RolesProfile()
        {
            CreateMap<DbRole, RolesDto>()
                .ForMember(dest => dest.Users, opt => opt.MapFrom(src => src.UserRoles));

            CreateMap<DbUserRole, RolesUserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.User.Name.FullName));
        }
    }

    [EnableQuery]
    public class RolesController : ODataController
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public RolesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [EnableQuery]
        public async Task<IActionResult> Get(ODataQueryOptions<RolesDto> options)
        {
            return Ok(await _context.Roles.AsNoTracking().GetAsync(_mapper, options));
        }
    }
}
