namespace EGM.Infrastructure.Hubs
{
    public static class NotificationGroupNames
    {
        /// <summary>Bir şehrin tüm personeli (IlPersoneli + IlYoneticisi) için grup adı.</summary>
        public static string City(int cityId) => $"city_{cityId}";

        /// <summary>Tüm başkanlık personeli (BaskanlikPersoneli + BaskanlikYoneticisi) için grup adı.</summary>
        public const string HQ = "hq";

        // Geriye-dönük uyumluluk — eski adlar
        public static string CityManagers(int cityId) => City(cityId);
        public const string HQManagers = HQ;
    }
}
