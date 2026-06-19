using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.CustomAttribute;
using Data.DbContext;
using Data.Repositories;
using Data.Seeds;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Services.ApiServices.ConcreteApi.SmsApi.RayganSms;
using Services.ApiServices.ConcreteApi.SmsApi.SmsOnlineService;
using Services.ApiServices.ConcreteApi.SmsApi.TrezRayan;
using Services.PublicService;
using Services.PublicService.Abstract;
using Services.PublicService.Abstract.ContractSms;
using Services.PublicService.Abstract.SmsSenderService;
using Services.PublicService.Abstract.SmsSenderService.KaveNegar;
using Services.PublicService.Abstract.SmsSenderService.LinePayamak;
using Services.PublicService.Abstract.SmsSenderService.Rahyab;
using Services.PublicService.Abstract.SmsSenderService.RayganSms;
using Services.PublicService.Abstract.SmsSenderService.SmsOnline;
using Services.PublicService.Abstract.SmsSenderService.TrezRayan;
using Services.PublicService.Concrete;
using Services.PublicService.Concrete.ContractSms;
using Services.PublicService.Concrete.SmsSenderService;
using Services.PublicService.Concrete.SmsSenderService.KaveNegar;
using Services.PublicService.Concrete.SmsSenderService.LinePayamak;
using Services.PublicService.Concrete.SmsSenderService.Rahyab;
using Services.WebService;
using Services.WebService.Abstract;
using Services.WebService.Abstract.Personel;
using Services.WebService.Abstract.SendSms;
using Services.WebService.Concrete;
using Services.WebService.Concrete.Personel;
using Services.WebService.Concrete.SendSms;
using Services.WebService.Concrete.Tariff.ContractPeriod.CopyStrategies;
using WebFramework.Configuration;
using WebFramework.CustomMapping;
using WebFramework.Filters;
using WebFramework.Middlewares;
using WebUI.TagHelpers;

namespace WebPanel
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _siteSetting = configuration.GetSection(nameof(SiteSettings)).Get<SiteSettings>();
        }

        public IConfiguration Configuration { get; }
        public ILifetimeScope AutofacContainer { get; private set; }

        private readonly SiteSettings _siteSetting;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.InitializeAutoMapper();
            services.Configure<SiteSettings>(Configuration.GetSection(nameof(SiteSettings)));
            services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = int.MaxValue;
                options.MultipartHeadersLengthLimit = int.MaxValue;
            });

            services.AddMyPagination();
            services.AddDistributedMemoryCache();
            services.AddSession();

            services.AddMemoryCache();
            services.AddScoped<IPortActiveCacheService, PortActiveCacheService>();

            services.AddControllersWithViews(option =>
            {
                option.Filters.Add(new AuthorizeFilter());
                option.Filters.Add(new LogsExceptionFilterAttribute());
            });

            services.AddDbContextPool<AppDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("SqlServerConnection"), x => x.UseNetTopologySuite());
            });
            // Factory for creating DbContext instances to allow safe parallel queries
            services.AddDbContextFactory<AppDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("SqlServerConnection"), x => x.UseNetTopologySuite());
            });
            services.AddSingleton<IValidationAttributeAdapterProvider, CustomValidationAttributeAdapterProvider>();
            services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();
            services.AddCustomIdentity(_siteSetting.IdentitySettings);
            services.ConfigureApplicationCookie(opt =>
            {
                opt.LoginPath = "/Account/Login";
                opt.AccessDeniedPath = "/Home/AccessDenied";
                opt.ExpireTimeSpan = TimeSpan.FromMinutes(40);
                opt.Cookie.HttpOnly = true;
                opt.SlidingExpiration = true;
            });
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                //options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            services.AddHttpContextAccessor();

            services.AddHttpClient();
            services.AddScoped<IMyHttpClient, MyHttpClient>();

            #region Sms Sender
            services.AddHttpClient<ISendSmsRahyabService, SendSmsRahyabService>();
            services.AddHttpClient<IRayganSmsService, RayganSmsService>();
            services.AddHttpClient<ITrezRayanService, TrezRayanService>();
            services.AddScoped<ISendSmsLinePayamakService, SendSmsLinePayamakService>();
            services.AddScoped<ISmsOnlineService, SmsOnlineService>();
            services.AddScoped<ISmsService, SmsService>();
            #endregion

            services.AddScoped<IKaveNegarRestService, KaveNegarRestService>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            services.AddScoped<IAccountPanelService, AccountPanelService>();
            services.AddScoped<IPasswordHasher<IdentityUser>, PasswordHasher<IdentityUser>>();

            services.AddScoped<ICurrentUserService<UserPanelInfoDto>, CurrentUserService>();
            services.AddScoped<
                ICurrentUserService<Entities.DTOs.Api.AccountDto.RequestingUserDto>,
                Services.ApiServices.ConcreteApi.CurrentUserApiService
            >();

            services.AddScoped<IPersonelService, PersonelService>();
            services.AddScoped<IPersonelPublicService, PersonelPublicService>();
            services.AddScoped<IExcelToolsService, ExcelToolsService>();

            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<IPortService, PortService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IVesselAllocationNoticeService, VesselAllocationNoticeService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IProvinceService, ProvinceService>();
            services.AddScoped<ICityService, CityService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<IWaterfrontService, WaterfrontService>();
            services.AddScoped<IEquipmentService, EquipmentService>();
            services.AddScoped<IShipService, ShipService>();
            services.AddScoped<ITransportationService, TransportationService>();
            services.AddScoped<ISeoService, SeoService>();
            services.AddScoped<IPublicSeoService, PublicSeoService>();
            services.AddScoped<ISendSmsKaveNegarService, SendSmsKaveNegarService>();
            services.AddScoped<ISeoReciveService, SeoReciveService>();
            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<ISeoBarshomarService, SeoBarshomarService>();
            services.AddScoped<ISeoWarehouseService, SeoWarehouseService>();
            services.AddScoped<ISeoCompletedService, SeoCompletedService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<ISeoStopServiceLogService, SeoStopServiceLogService>();
            services.AddScoped<ISeoPublicService, SeoPublicService>();
            services.AddScoped<IBaskoolService, BaskoolService>();

            services.AddScoped<IAreaService, AreaService>();

            services.AddScoped<IImasService, ImasService>();
            services.AddScoped<IImasVeseelService, ImasVeseelService>();
            services.AddScoped<IImasVeseelReportService, ImasVeseelReportService>();

            services.AddScoped<ISimcardService, SimcardService>();
            services.AddScoped<IAllocationEquiPreFactorService, AllocationEquiPreFactorService>();
            services.AddScoped<IAllocationEquFactorService, AllocationEquFactorService>();
            services.AddScoped<IAllocationEquReportService, AllocationEquReportService>();
            services.AddScoped<IAllocationEquFileService, AllocationEquFileService>();
            services.AddScoped<IAllocationEquChangeStateService, AllocationEquChangeStateService>();
            services.AddScoped<IAllocationEquToEndService, AllocationEquToEndService>();
            services.AddScoped<IAllocationEquDocFileService, AllocationEquDocFileService>();
            services.AddScoped<IAllocationEquPaymentService, AllocationEquPaymentService>();
            services.AddScoped<IAllocationEquPublicService, AllocationEquPublicService>();
            services.AddScoped<IEncryptDeFileService, EncryptDeFileService>();
            services.AddScoped<IVesselService, VesselService>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<IEquipmentLogService, EquipmentLogService>();
            services.AddScoped<ILogShipmentService, LogShipmentService>();
            services.AddScoped<ISeoCompanyService, SeoCompanyService>();

            services.AddScoped<IStimulSoftReportService, StimulSoftReportService>();
            services.AddScoped<ISeoReportService, SeoReportService>();
            services.AddScoped<IShipInLangargahService, ShipInLangargahService>();

            services.AddScoped<IOtherIncomeInvoiceService, OtherIncomeInvoiceService>();

            services.AddScoped<IOtherIncomeInvoiceReportService, OtherIncomeInvoiceReportService>();
            services.AddScoped<IOtherInvoiceFileService, OtherInvoiceFileService>();
            services.AddScoped<IOtherInvoicePublicService, OtherInvoicePublicService>();
            services.AddScoped<IOtherIncomeInvoiceDocFileService, OtherIncomeInvoiceDocFileService>();

            services.AddScoped<IVoyageService, VoyageService>();
            services.AddScoped<IVoyageDeletionService, VoyageDeletionService>();
            services.AddScoped<IVoyageTaliGroupService, VoyageTaliGroupService>();
            services.AddScoped<IVoyageReportService, VoyageReportService>();
            services.AddScoped<IProductOwnerImasVesselService, ProductOwnerImasVesselService>();
            services.AddScoped<IProductPackingService, ProductPackingService>();

            services.AddScoped<ITaliCommonService, TaliCommonService>();
            services.AddScoped<ITaliService, TaliService>();
            services.AddScoped<ITaliWeightService, TaliWeightService>();

            services.AddScoped<ISeoWarehouseReceiptService, SeoWarehouseReceiptService>();
            services.AddScoped<IWarehouseReceiptManagerService, WarehouseReceiptManagerService>();
            services.AddScoped<ISeoWarehouseReceiptReportService, SeoWarehouseReceiptReportService>();
            services.AddScoped<ISplitSeoWarehouseReceiptService, SplitSeoWarehouseReceiptService>();
            services.AddScoped<IStripContainerSeoWarehouseReceiptService, StripContainerSeoWarehouseReceiptService>();
            services.AddScoped<IMergeSeoWarehouseReceiptService, MergeSeoWarehouseReceiptService>();
            services.AddScoped<IChangeOwnershipSeoWarehouseReceiptService, ChangeOwnershipSeoWarehouseReceiptService>();

            services.AddScoped<IWarehouseReceiptOutboundManagerService, WarehouseReceiptOutboundManagerService>();
            services.AddScoped<IWarehouseReceiptOutboundReportService, WarehouseReceiptOutboundReportService>();
            services.AddScoped<IWarehouseReceiptRemainingService, WarehouseReceiptRemainingService>();

            services.AddScoped<IGearTypeService, GearTypeService>();
            services.AddScoped<IGearStateService, GearStateService>();
            services.AddScoped<IGearProduceService, GearProduceService>();
            services.AddScoped<IGearService, GearService>();
            services.AddScoped<IGearStateTrackerService, GearStateTrackerService>();
            services.AddScoped<IGearStateTrackerReportService, GearStateTrackerReportService>();

            services.AddScoped<IGearWorkSessionService, GearWorkSessionService>();
            services.AddScoped<IGearDailyInspectionAssignmentService, GearDailyInspectionAssignmentService>();

            services.AddScoped<IPaymentReceiptReportService, PaymentReceiptReportService>();

            services.AddScoped<ICheckListService, CheckListService>();
            services.AddScoped<ICheckListFactorService, CheckListFactorService>();

            services.AddScoped<IDocumentDirectoryService, DocumentDirectoryService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<ISeoInsuranceService, SeoInsuranceService>();

            services.AddScoped<IFactorPublicService, FactorPublicService>();
            services.AddScoped<ISeoFactorService, SeoFactorService>();
            services.AddScoped<ISeoFactorItemService, SeoFactorItemService>();
            services.AddScoped<ISeoFactorFileService, SeoFactorFileService>();
            services.AddScoped<ISeoFactorReportService, SeoFactorReportService>();
            services.AddScoped<ISeoFactorDeleteService, SeoFactorDeleteService>();

            services.AddScoped<IVoyageFactorService, VoyageFactorService>();
            services.AddScoped<IVoyageFactorReportService, VoyageFactorReportService>();
            services.AddScoped<IVoyageFactorFileService, VoyageFactorFileService>();

            services.AddScoped<IOtherFactorService, OtherFactorService>();
            services.AddScoped<IOtherFactorReportService, OtherFactorReportService>();
            services.AddScoped<IOtherFactorFileService, OtherFactorFileService>();

            services.AddScoped<ISeoCustomsService, SeoCustomsService>();

            services.AddScoped<IGateService, GateService>();

            services.AddScoped<IContainerGroupService, ContainerGroupService>();
            services.AddScoped<IContainerTypeService, ContainerTypeService>();
            services.AddScoped<IContainerPackingTypeService, ContainerPackingTypeService>();

            // Contract
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<IContractCommonService, ContractCommonService>();
            services.AddScoped<IFactorNumberService, FactorNumberService>();

            services.AddScoped<IAppVersionWebService, AppVersionWebService>();
            services.AddScoped<ISystemSettingService, SystemSettingService>();

            services.AddScoped<IDapperService, DapperService>();

            services.AddScoped<ILaborWorkingGroupService, LaborWorkingGroupService>();
            services.AddScoped<ITruckInfoSokhtService, TruckInfoSokhtService>();
            services.AddScoped<IContractPeriodService, ContractPeriodService>();

            services.AddScoped<ISpecialGearTariffRuleService, SpecialGearTariffRuleService>();
            services.AddScoped<ISpecialGearTariffService, SpecialGearTariffService>();
            services.AddScoped<ISpecialGearTariffPriceCalculatorService, SpecialGearTariffPriceCalculatorService>();

            services.AddScoped<IGeneralCargoProductService, GeneralCargoProductService>();
            services.AddScoped<IGeneralCargoProductTariffService, GeneralCargoProductTariffService>();
            services.AddScoped<IGeneralCargoProductTariffCalculatorService, GeneralCargoProductTariffCalculatorService>();

            services.AddScoped<ICargoHandlingTariffService, BaseCargoHandlingTariffService>();

            services.AddScoped<INonContainerWarehouseProductService, NonContainerWarehouseProductService>();
            services.AddScoped<INonContainerWarehouseProductTariffService, NonContainerWarehouseProductTariffService>();
            services.AddScoped<INonContainerWarehouseProductPriceCalculatorService, NonContainerWarehouseProductPriceCalculatorService>();
            services.AddScoped<INonContainerWarehouseProductDiscountService, NonContainerWarehouseProductDiscountService>();

            services.AddScoped<IContainerFreightProductService, ContainerFreightProductService>();
            services.AddScoped<IContainerFreightProductTariffService, ContainerFreightProductTariffService>();
            services.AddScoped<IContainerFreightProductTariffCalculatorService, ContainerFreightProductTariffCalculatorService>();

            services.AddScoped<IContainerWarehouseProductService, ContainerWarehouseProductService>();
            services.AddScoped<IContainerWarehouseProductTariffService, ContainerWarehouseProductTariffService>();
            services.AddScoped<IContainerWarehouseProductPriceCalculatorService, ContainerWarehouseProductPriceCalculatorService>();

            services.AddScoped<IContainerStripProductService, ContainerStripProductService>();
            services.AddScoped<IContainerStripProductTariffService, ContainerStripProductTariffService>();
            services.AddScoped<IContainerStripProductTariffCalculatorService, ContainerStripProductTariffCalculatorService>();

            services.AddScoped<IKhankaryNonContainerProductService, KhankaryNonContainerProductService>();
            services.AddScoped<IKhankaryNonContainerProductTariffService, KhankaryNonContainerProductTariffService>();
            services.AddScoped<IKhankaryNonContainerProductTariffCalculatorService, KhankaryNonContainerProductTariffCalculatorService>();

            services.AddScoped<IPortDutyService, PortDutyService>();
            services.AddScoped<IPortDutyTariffService, PortDutyTariffService>();
            services.AddScoped<IPortDutyTariffPriceCalculatorService, PortDutyTariffPriceCalculatorService>();

            services.AddScoped<ICopyContractPeriodTable, SpecialGearTariffCopier>();
            services.AddScoped<ICopyContractPeriodTable, PortDutyTariffCopier>();
            services.AddScoped<ICopyContractPeriodTable, NonContainerWarehouseProductTariffCopier>();
            services.AddScoped<ICopyContractPeriodTable, KhankaryNonContainerTariffCopier>();
            services.AddScoped<ICopyContractPeriodTable, GeneralCargoProductTariffCopier>();
            services.AddScoped<ICopyContractPeriodTable, ContainerWarehouseProductTariffCopier>();
            services.AddScoped<ICopyContractPeriodTable, ContainerStripProductTariffCopier>();
            services.AddScoped<ICopyContractPeriodTable, ContainerFreightProductTariffCopier>();

            services.AddScoped<IDashboardAdminService, DashboardAdminService>();

            services.AddScoped<IPortTruckExitService, PortTruckExitService>();
            services.AddScoped<ISeoFactorRemainderService, SeoFactorRemainderService>();
            services.AddScoped<IInvoicePublicService, InvoicePublicService>();
            services.AddScoped<IInvoiceService, InvoiceService>();

            services.AddScoped<IInlandExportManifestService, InlandExportManifestService>();
            services.AddScoped<IInlandExportManifestTruckService, InlandExportManifestTruckService>();

            services.AddScoped<ICascadeDeleteService, CascadeDeleteService>();

            services.AddScoped<IWeightCalculationMethodResolver, WeightCalculationMethodResolver>();

            // Payment services
            services.AddScoped<Services.PublicService.Abstract.IPaymentReceiptService, Services.PublicService.Concrete.PaymentReceiptService>();
            services.AddScoped<
                Services.PublicService.Abstract.ITejaratElectronicParsianService,
                Services.PublicService.Concrete.TejaratElectronicParsianService
            >();
            // seeders
            services.AddScoped<IDbSeeder, DbSeeder>();
            services.AddScoped<PopulateGearStates>();
            services.AddScoped<PopulateGearTypes>();
            services.AddScoped<PopulateSettingEnumDb>();

            services.AddScoped<ISettingService, SettingService>();

            services.AddScoped<IContractSmsService, ContractSmsService>();

            services.AddScoped<Func<string, ISeeder>>(sp =>
                key =>
                {
                    return key switch
                    {
                        nameof(PopulateGearStates) => sp.GetService<PopulateGearStates>(),
                        nameof(PopulateGearTypes) => sp.GetService<PopulateGearTypes>(),
                        nameof(PopulateSettingEnumDb) => sp.GetService<PopulateSettingEnumDb>(),
                        _ => throw new ArgumentException("Invalid key"),
                    };
                }
            );

            #region Api Service Register
            services.AddScoped<Services.ApiServices.Abstract.IBaskolService, Services.ApiServices.ConcreteApi.BaskolService>();
            services.AddScoped<Services.ApiServices.Abstract.IBarshomarService, Services.ApiServices.ConcreteApi.BarshomarService>();
            services.AddScoped<Services.ApiServices.Abstract.ISeoTransportService, Services.ApiServices.ConcreteApi.SeoTransportService>();
            services.AddScoped<Services.ApiServices.Abstract.IOwnerProductService, Services.ApiServices.ConcreteApi.OwnerProductService>();
            services.AddScoped<Services.ApiServices.Abstract.IPortApiService, Services.ApiServices.ConcreteApi.PortApiService>();
            services.AddScoped<Services.ApiServices.Abstract.IShipApiService, Services.ApiServices.ConcreteApi.ShipApiService>();
            services.AddScoped<
                Services.ApiServices.Abstract.IAllocationEqupmentApiService,
                Services.ApiServices.ConcreteApi.AllocationEqupmentApiService
            >();
            services.AddScoped<
                Services.ApiServices.Abstract.IProductCategoryApiService,
                Services.ApiServices.ConcreteApi.ProductCategoryApiService
            >();
            services.AddScoped<Services.ApiServices.Abstract.IBaskolWeightService, Services.ApiServices.ConcreteApi.BaskolWeightService>();
            #endregion


            //var cb = new ContainerBuilder();
            //cb.RegisterType<SeoCompletedService>().As<ISeoCompletedService>()
            //    .InstancePerLifetimeScope()
            //    .PropertiesAutowired(PropertyWiringOptions.AllowCircularDependencies);

            //cb.Build();
            services.AddSingleton<JsonSerializer>();

            services.AddResponseCompression(opt =>
            {
                opt.Providers.Add<GzipCompressionProvider>();
                opt.Providers.Add<BrotliCompressionProvider>();
                opt.EnableForHttps = true;
                opt.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[]
                    {
                        "text/plain",
                        "font/woff2",
                        "text/css",
                        "application/javascript",
                        "text/html",
                        "application/xml",
                        "text/xml",
                        "application/json",
                        "text/json",
                        "image/svg+xml",
                        "image/png",
                        "image/jpg",
                    }
                );
            });
            services.Configure<GzipCompressionProviderOptions>(opt =>
            {
                opt.Level = CompressionLevel.Optimal;
            });
            services.AddResponseCaching(opt =>
            {
                opt.UseCaseSensitivePaths = true;
                opt.MaximumBodySize = 1024;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var cultureInfo = new CultureInfo("en-US");

            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            #region Error Save
            // For WebPanel (MVC views), use the built-in exception/status code handlers instead of the API-style JSON handler.
            // app.UseCustomExceptionHandler();
            //app.UseExceptionHandler(errorApp =>
            //{
            //    errorApp.Run(async context =>
            //    {
            //        context.Response.StatusCode = 500; // یا هر کد وضعیت خطا دلخواهی
            //        context.Response.ContentType = "text/html";

            //        var ex = context.Features.Get<IExceptionHandlerFeature>();
            //        if (ex != null)
            //        {
            //            ErrorLog.SaveError(ex.Error);
            //            // ثبت خطا در لاگ
            //            ILogger logger = context.RequestServices.GetRequiredService<ILogger<Startup>>();
            //            logger.LogError(ex.Error, "An error occurred in the application.");

            //            // نمایش یک صفحه خطا به کاربر (اختیاری)
            //            await context.Response.WriteAsync("<h1>Something went wrong!</h1>").ConfigureAwait(false);
            //        }
            //    });
            //});

            #endregion


            if (env.IsDevelopment())
            {
                // In development, show detailed exception page but still render custom views for status codes (e.g., 404).
                app.UseDeveloperExceptionPage();
                app.UseStatusCodePagesWithReExecute("/Error/StatusCode/{0}");
            }
            else
            {
                // In production, use exception handler and custom status code pages that return views.
                app.UseExceptionHandler("/Error/Index");
                app.UseStatusCodePagesWithReExecute("/Error/StatusCode/{0}");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            //برای ایجاد دیتای اولیه در دیتابیس
            // app.IntializeDatabase();

            app.UseResponseCompression();
            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Ensure AmarPath exists
            if (!Directory.Exists(_siteSetting.AmarPath))
            {
                Directory.CreateDirectory(_siteSetting.AmarPath);
            }

            // Static files for amar folder (PMO reports)
            app.UseStaticFiles(new StaticFileOptions { FileProvider = new PhysicalFileProvider(_siteSetting.AmarPath), RequestPath = "/amar" });

            app.UseRouting();

            app.UseCors();
            app.UseAuthentication();
            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseAuthorization();
            app.UseSession();

            #region Static File
            //AllocationEqu
            app.UseStaticFiles(
                new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(_siteSetting.AllocationEquPath),
                    RequestPath = "/AllocationEqu",

                    OnPrepareResponse = async (sfrc) =>
                    {
                        var startsWithSegmentsImages = sfrc.Context.Request.Path.StartsWithSegments("/AllocationEqu");
                        var isAuthenticated = sfrc.Context.User.Identity.IsAuthenticated;

                        if (startsWithSegmentsImages)
                        {
                            if (!isAuthenticated)
                            {
                                sfrc.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                sfrc.Context.Response.Redirect("/Account/Login");
                                //await sfrc.Context.ForbidAsync();
                            }
                        }
                    },
                }
            );
            #endregion

            //app.UseCookiePolicy();
            //this.AutofacContainer = app.ApplicationServices.GetAutofacRoot();

            using (var scope = app.ApplicationServices.CreateScope())
            {
                var services = scope.ServiceProvider;

                var context = services.GetRequiredService<AppDbContext>();
                if (context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();
                }

                var seeder = services.GetRequiredService<IDbSeeder>();
                var task = Task.Run(async () => await seeder.SeedAsync());
                task.Wait();
            }

            // RotativaConfiguration.Setup(host);
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name: "default", pattern: "{controller=Account}/{action=Login}/{id?}");
            });

            app.UseCookiePolicy();
        }
    }
}
