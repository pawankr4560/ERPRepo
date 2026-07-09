using AutoMapper;
using ERPWebAppData.Entity;
using ERPWebAppModels.Auth;
using ERPWebAppModels.Dashbord;
using WebApp.Data.Entity;
using WebApp.Model.Auth;
using WebApp.Model.Order;
using WebApp.Model.Product;
using WebApp.Model.Transaction;
namespace WebApp.Data.Automapper
{
    public class Automapping : AutoMapper.Profile
    {
        public Automapping()
        {
            CreateMap<SignupRequestModel, User>();
            CreateMap<CreateProductRequestModel, Product>()
                .ForMember(dest => dest.UnitId, opt => opt.MapFrom(src => src.UnitId != 0 ? src.UnitId : src.UOMIndex))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => Convert.ToDecimal(src.Price)));
            CreateMap<UpdateProductModel, Product>()
                .ForMember(dest => dest.UnitId, opt => opt.MapFrom(src => src.UnitId != 0 ? src.UnitId : src.UOMIndex))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => Convert.ToDecimal(src.Price)));
            CreateMap<LoanRequestModel, Loan>();
            CreateMap<LoanEMISchedule, LoanEMIScheduleDto>().ReverseMap();
            CreateMap<LoanPayment, LoanPaymentDto>().ReverseMap();
            CreateMap<Loan, LoanDto>();
            CreateMap<Notification, NotificationDto>().ReverseMap();
            CreateMap<PreApprovedOffer, PreApprovedOfferDto>().ReverseMap();
            CreateMap<CreatePreApprovedOfferRequestDto, PreApprovedOffer>();
            CreateMap<CreateNotificationRequestDto, Notification>();
            CreateMap<CreateOrderRequestModel, OrderHistory>()
           .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.Id)).ForMember(dest => dest.Id, opt=> opt.MapFrom(src => Guid.NewGuid()));
        }
    }
}

