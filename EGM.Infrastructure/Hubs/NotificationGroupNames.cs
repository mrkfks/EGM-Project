namespace EGM.Infrastructure.Hubs
{
    public static class NotificationGroupNames
    {
        /// <summary>İl Yöneticileri için şehir bazlı grup adı.</summary>
        public static string CityManagers(int cityId) => $"city_{cityId}_managers";

        /// <summary>Başkanlık Yöneticileri için merkezi grup adı.</summary>
        public const string HQManagers = "hq_managers";
    }
}
