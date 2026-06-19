using System.ComponentModel.DataAnnotations;

namespace Common.enumeration
{
    public enum ResponseMessageEnum
    {
        [Display(Name ="در این گروه پرسنلی یافت نشد")]
        UserNotFoundInGroup = 1,
        [Display(Name =" با موفقیت انجام شد")]
        Success,
        [Display(Name ="بروز خطا در انجام درخواست شما")]
        Failed,
        [Display(Name ="پرسنل این گروه اپ را نصب نکرده اند")]
        UserNotInstalledApp,
        [Display(Name ="این گروه دارای پیام می باشد")]
        GroupAnyNotif,


        //برای نمایش پیغام های رزرویشن
        [Display(Name = "پزشک برنامه حضور ندارد")]
        PlanNotFound = 6,
        [Display(Name = "برای پزشک، شیف انتخاب شده تعریف نشده است")]
        ShiftNotFound = 7,
        [Display(Name = "این شیفت در تاریخ انتخاب شده،قبلا لغو شده است")]

        DuplicatedRequest = 8,
        [Display(Name = "انجام شد")]
        SuccessFuly = 9,
        [Display(Name = "تاریخ اشتباه")]
        LessDate = 10




    }

    public enum ReservationResponse
    {
        [Display(Name = "پزشک برنامه حضور ندارد")]
        PlanNotFound =1,
        [Display(Name = "برای پزشک، شیف انتخاب شده تعریف نشده است")]
        ShiftNotFound =2,
        [Display(Name = "این شیفت در تاریخ انتخاب شده،قبلا لغو شده است")]

        DuplicatedRequest =3,
        [Display(Name = "انجام شد")]
        SuccessFuly =4,
        [Display(Name = "تاریخ اشتباه")]
        LessDate = 5
    }
}
