namespace EGM.Domain.Attributes
{
    /// <summary>
    /// Bu nitelik ile işaretlenen string alanlar Infrastructure katmanında
    /// AES-256 algoritması kullanılarak şifrelenip depolanır.
    /// Örneğin: TcKimlikNo alanları.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class EncryptedAttribute : Attribute
    {
    }
}
