using AutoMapper;
using QFileServer.Definitions.DTOs;
using QFileServer.Mvc.Helpers;
using QFileServer.Mvc.ViewModels;

namespace QFileServer.Mvc.Configuration
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            CreateMap<QFileServerDTO, FileViewModel>()
                .ForMember(d => d.HRSize, oo => oo.MapFrom(s => QFileServerHelper.FormatBytes(s.Size)));
        }
    }
}
