using AutoMapper;
using LoanApplicationPlatform.API.Entities;
using LoanApplicationPlatform.API.Models;

namespace LoanApplicationPlatform.API.Profiles
{
    public class LoanApplicationProfile : Profile
    {
        public LoanApplicationProfile()
        {
            CreateMap<LoanApplication, LoanApplicationDto>();
            CreateMap<LoanApplicationForCreationDto, LoanApplication>();
            CreateMap<PaymentSchedule, PaymentScheduleDto>();
        }
    }
}
