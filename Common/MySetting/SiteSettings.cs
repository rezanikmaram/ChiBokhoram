using System.Collections.Generic;
using Common.enumeration;
using Newtonsoft.Json;

namespace Common
{
    public class SiteSettings
    {
        public bool TestConfirmSms { get; set; }
        public bool Last4IDConfirmSms { get; set; }

        public string ElmahPath { get; set; }
        public long WeightFreeOver { get; set; } //در بک اند برای تشخیص وزن خالی هر کامیون استفاده می شود
        public int WeightFreeOverPercent { get; set; } //در بک اند برای تشخیص وزن خالی هر کامیون استفاده می شود
        public string MediaPath { get; set; }
        public string AllocationEquPath { get; set; }
        public string AmarPath { get; set; }
        public string AvatarPath { get; set; }
        public string WatermarkPath { get; set; }
        public string Slider { get; set; }
        public string ExcelPath { get; set; }
        public string PwaUrl { get; set; }
        public string PassRfid { get; set; }
        public string PassOrganizationApp { get; set; }
        public string PassBaskol { get; set; }
        public string KeyFileEncrypt { get; set; }
        public string StimulReportFilePath { get; set; }
        public string TaliFilePath { get; set; }
        public string WarehouseReceipt { get; set; }
        public string AppVersionTempPath { get; set; }

        public ImasSetting ImasSettings { get; set; }
        public JwtSettings JwtSettings { get; set; }
        public IdentitySettings IdentitySettings { get; set; }
        public OrganizationInfo OrganizationInfo { get; set; }
        public TruckInfoWebService TruckInfoWebService { get; set; }

        public RahyabSmsInfo RahyabSmsInfo { get; set; }
        public RayganSmsInfo RayganSmsInfo { get; set; }
        public LinePayamakInfo LinePayamakInfo { get; set; }
        public SmsOnlineInfo SmsOnlineInfo { get; set; }

        public TrezRayanInfo TrezRayanInfo { get; set; }
    }

    public class IdentitySettings
    {
        public bool PasswordRequireDigit { get; set; }
        public int PasswordRequiredLength { get; set; }
        public bool PasswordRequireNonAlphanumic { get; set; }
        public bool PasswordRequireUppercase { get; set; }
        public bool PasswordRequireLowercase { get; set; }
        public bool RequireUniqueEmail { get; set; }
    }

    public class JwtSettings
    {
        public string SecretKey { get; set; }
        public string Encryptkey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int NotBeforeMinutes { get; set; }
        public int ExpirationMinutes { get; set; }
    }

    public class OrganizationInfo
    {
        public string OrgNameFa { get; set; }
        public string OrgNameEn { get; set; }
        public EnumOrganizationInfo Type { get; set; }
        public List<AppModule> AppModules { get; set; }
        public string ApiBaseUrl { get; set; }
        public bool IsTestMode { get; set; }
    }

    public class TruckInfoWebService
    {
        public string BaseUrl { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class RahyabSmsInfo
    {
        public string BaseUrlApi { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Company { get; set; }
        public string Number { get; set; }
        public string[] SmsConfirmTemplate { get; set; }
    }

    public class TrezRayanInfo
    {
        public string BasePatternUrlApi { get; set; }
        public string AccessHash { get; set; }
        public string LoginPatternId { get; set; }
        public string TextPatternId { get; set; }

        public string Number { get; set; }
    }

    public class RayganSmsInfo
    {
        public string BaseUrlApi { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Number { get; set; }
        public string[] SmsConfirmTemplate { get; set; }
    }

    public class LinePayamakInfo
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Number { get; set; }
        public string[] SmsConfirmTemplate { get; set; }
    }

    public class SmsOnlineInfo
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Number { get; set; }
        public string[] SmsConfirmTemplate { get; set; }
    }
}
