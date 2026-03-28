namespace EGM.Domain.Constants;

public static class Roles
{
    // ── Altı rol (hiyerarşik sıralamayla) ────────────────────────────────
    /// <summary>Varsayılan rol. Sadece okuma/izleme yetkisi vardır.</summary>
    public const string Izleyici            = "Izleyici";

    /// <summary>İl personeli: kendi iline veri girebilir, sadece kendi kayıtlarını düzenler.</summary>
    public const string IlPersoneli         = "IlPersoneli";

    /// <summary>İl yöneticisi: ilindeki tüm veriyi yönetir, kendi ilinin personelini yönetir.</summary>
    public const string IlYoneticisi        = "IlYoneticisi";

    /// <summary>Başkanlık personeli: coğrafi kısıt yok, yalnızca kendi kayıtlarını düzenler.</summary>
    public const string BaskanlikPersoneli  = "BaskanlikPersoneli";

    /// <summary>Başkanlık yöneticisi: 81 il genelinde yönetim yetkisi.</summary>
    public const string BaskanlikYoneticisi = "BaskanlikYoneticisi";

    /// <summary>Yetkili: Tüm sistemde sınırsız erişim ve düzenleme hakkı.</summary>
    public const string Yetkili             = "Yetkili";

    // ── Geriye-dönük uyumluluk takma adları ─────────────────────────────
    public const string IlPersonel         = IlPersoneli;
    public const string IlAdmin            = IlYoneticisi;
    public const string BaskanlikPersonel  = BaskanlikPersoneli;
    public const string BaskanlikAdmin     = BaskanlikYoneticisi;
    public const string Yonetici           = Yetkili;

    // ── Hiyerarşi (düşükten yükseğe) ─────────────────────────────────────
    private static readonly List<string> Hierarchy = new()
    {
        Izleyici, IlPersoneli, IlYoneticisi, BaskanlikPersoneli, BaskanlikYoneticisi, Yetkili
    };

    /// <summary>
    /// İki rol arasında assignerRole rolünün targetRole rolünden üstte olup olmadığını döner.
    /// </summary>
    public static bool IsAbove(string assignerRole, string targetRole)
    {
        var ai = Hierarchy.IndexOf(assignerRole);
        var ti = Hierarchy.IndexOf(targetRole);
        return ai > ti && ai >= 0;
    }

    /// <summary>Verilen rolün geçerli bir sistem rolü olup olmadığını döner.</summary>
    public static bool IsValidRole(string role)
        => Hierarchy.Contains(role);

    /// <summary>Verilen rolün İl düzeyinde (şehir kısıtlı) olup olmadığını döner.</summary>
    public static bool IsCityScoped(string role)
        => role == IlPersoneli || role == IlYoneticisi;
}
