using AutoMapper;
using ERPWebAppModels.Auth;
using WebApp.Data.Entity;
using WebApp.Model.Auth;
using WebApp.Model.Order;
using WebApp.Model.Product;
using WebApp.Model.Transaction;
namespace WebApp.Data.Automapper
{
    public class Automapping : Profile
    {
        public Automapping()
        {
            CreateMap<SignupRequestModel, User>();
            CreateMap<CreateProductRequestModel, Product>();
            CreateMap<UpdateProductModel, Product>();
            CreateMap<LoanRequestModel, Loan>();
            CreateMap<LoanEMISchedule, LoanEMIScheduleDto>().ReverseMap();
            CreateMap<LoanPayment, LoanPaymentDto>().ReverseMap();
            CreateMap<Loan, LoanDto>();
            CreateMap<CreateOrderRequestModel, OrderHistory>()
           .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Id)).ForMember(dest => dest.Id, opt=> opt.MapFrom(src => Guid.NewGuid()));
        }
    }
}
