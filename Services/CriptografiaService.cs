using System.Security.Cryptography;
using System.Text;

namespace CaixaVersoApi.Services;
public sealed class CriptografiaService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public CriptografiaService(IConfiguration cfg)
    {
        _key = Convert.FromBase64String(cfg["Criptografia:KeyBase64"]!);
        _iv  = Convert.FromBase64String(cfg["Criptografia:IvBase64"]!);
    }

    public string Criptografar(string texto)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        using var encryptor = aes.CreateEncryptor();
        var inputBytes = Encoding.UTF8.GetBytes(texto);
        var encrypted = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

        return Convert.ToBase64String(encrypted);
    }

    public string Descriptografar(string textoBase64)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        using var decryptor = aes.CreateDecryptor();
        var encryptedBytes = Convert.FromBase64String(textoBase64);
        var decrypted = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

        return Encoding.UTF8.GetString(decrypted);
    }
}