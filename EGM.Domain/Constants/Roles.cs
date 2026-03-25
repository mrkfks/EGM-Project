namespace EGM.Domain.Constants;

public static class Roles
{
    // ── Beş rol (hiyerarşik sıralamayla) ─────────────────────────────────
    /// <summary>Varsayılan rol. Sadece okuma/izleme yetkisi vardır.</summary>
    public const string Izleyici            = "Izleyici";

    /// <summary>İl personeli: kendi iline veri girebilir, sadece kendi kayıtlarını düzenler.</summary>
    public const string IlPersoneli         = "IlPersoneli";

    /// <summary>İl yöneticisi: ilindeki tüm veriyi yönetir, İzleyici → İl Personeli atayabilir.</summary>
    public const string IlYoneticisi        = "IlYoneticisi";

    /// <summary>Başkanlık personeli: coğrafi kısıt yok, yalnızca kendi kayıtlarını düzenler.</summary>
    public const string BaskanlikPersoneli  = "BaskanlikPersoneli";

    /// <summary>Başkanlık yöneticisi / süper admin: tam yetki, tüm atamalar.</summary>
    public const string BaskanlikYoneticisi = "BaskanlikYoneticisi";

    // ── Geriye-dönük uyumluluk takma adları (eski kod varsa)  ────────────
    public const string IlPersonel         = IlPersoneli;
    public const string IlAdmin            = IlYoneticisi;
    public const string BaskanlikPersonel  = BaskanlikPersoneli;
    public const string BaskanlikAdmin     = BaskanlikYoneticisi;
    public const string Yonetici           = BaskanlikYoneticisi;

    // ── Hiyerarşi (düşükten yükseğe) ─────────────────────────────────────
    private static readonly List<string> Hierarchy = new()
    {
        Izleyici, IlPersoneli, IlYoneticisi, BaskanlikPersoneli, BaskanlikYoneticisi
    };

    /// <summary>
    /// İki rol arasında <paramref name="assignerRole"/> rolünün
    /// <paramref name="targetRole"/> rolünden üstte olup olmadığını döner.
    /// </summary>
    public static bool IsAbove(string assignerRole, string targetRole)
    {
        var ai = Hierarchy.IndexOf(assignerRole);
        var ti = Hierarchy.IndexOf(targetRole);
        return ai > ti && ai >= 0;
    }

    /// <summary>Verilen rolün İl düzeyinde (şehir kısıtlı) olup olmadığını döner.</summary>
    public static bool IsCityScoped(string role)
        => role == IlPersoneli || role == IlYoneticisi;
}