using System.Globalization;
using System.Runtime.CompilerServices;
namespace SolidTemplate.Shared;

public class CultureInfoManager
{
    /// <summary>
    ///     To enable/disable multilingual support, navigate to Directory.Build.props and modify the MultilingualEnabled flag.
    /// </summary>
    public static bool MultilingualEnabled => false;
#if MultilingualEnabled
    true;
#else
#endif

    public static CultureInfo DefaultCulture => CreateCultureInfo("en-US");

    public static (string DisplayName, CultureInfo Culture)[] SupportedCultures =>
    [
        ("English US", CreateCultureInfo("en-US")),
        ("VietNamese", CreateCultureInfo("vi-VN"))
    ];

    private static CultureInfo CreateCultureInfo(string name)
    {
        var cultureInfo = OperatingSystem.IsBrowser() ? CultureInfo.CreateSpecificCulture(name) : new CultureInfo(name);

        if (name == "fa-IR")
        {
            CustomizeCultureInfoForFaCulture(cultureInfo);
        }

        return cultureInfo;
    }

    public static void SetCurrentCulture(string cultureName)
    {
        var cultureInfo = SupportedCultures.FirstOrDefault(sc => sc.Culture.Name == cultureName).Culture ?? DefaultCulture;

        CultureInfo.CurrentCulture = CultureInfo.DefaultThreadCurrentCulture = Thread.CurrentThread.CurrentCulture = cultureInfo;
        CultureInfo.CurrentUICulture = CultureInfo.DefaultThreadCurrentUICulture = Thread.CurrentThread.CurrentUICulture = cultureInfo;
    }

    /// <summary>
    ///     This is an example to demonstrate the way you can customize application culture
    /// </summary>
    private static void CustomizeCultureInfoForFaCulture(CultureInfo cultureInfo)
    {
        cultureInfo.DateTimeFormat.AMDesignator = "ق.ظ";
        cultureInfo.DateTimeFormat.PMDesignator = "ب.ظ";
        cultureInfo.DateTimeFormat.ShortDatePattern = "yyyy/MM/dd";
        cultureInfo.DateTimeFormat.AbbreviatedDayNames =
        [
            "ی", "د", "س", "چ", "پ", "ج", "ش"
        ];
        cultureInfo.DateTimeFormat.ShortestDayNames =
        [
            "ی", "د", "س", "چ", "پ", "ج", "ش"
        ];

        Get_CalendarField(cultureInfo) = new PersianCalendar();

    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_calendar")]
    private static extern ref Calendar Get_CalendarField(CultureInfo cultureInfo);
}
