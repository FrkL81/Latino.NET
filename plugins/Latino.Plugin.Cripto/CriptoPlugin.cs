using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Latino.Core.Runtime;
using Latino.Core.Types;

namespace Latino.Plugin.Cripto;

public static class CriptoPlugin
{
    // Tamaños fijos del protocolo — no cambiar sin versionar el formato
    private const int SaltSize   = 16; // bytes — PBKDF2 salt
    private const int NonceSize  = 12; // bytes — AES-GCM nonce (96 bits, estándar NIST)
    private const int TagSize    = 16; // bytes — AES-GCM authentication tag (128 bits)
    private const int HeaderSize = SaltSize + NonceSize + TagSize; // 44 bytes de overhead fijo
    private const int KeySize    = 32; // bytes — AES-256
    private const int Pbkdf2Iter = 100_000; // iteraciones PBKDF2 (OWASP 2023 mínimo)

    // =========================================================================
    // cripto.encriptar(texto, clave)
    // =========================================================================

    [LatinoExtension("cripto.encriptar",
        Args = "texto, clave",
        Help = "Encripta un texto con AES-256-GCM. Retorna una cadena Base64 auto-contenida " +
               "que incluye el salt, IV y tag de autenticación. La clave puede ser cualquier cadena.")]
    public static LatinoObject Encriptar(List<LatinoObject> args)
    {
        if (args.Count < 2)
            throw new RuntimeError(null!, "cripto.encriptar requiere texto y clave.");

        if (args[0] is LatinoNull || args[1] is LatinoNull)
            throw new RuntimeError(null!, "cripto.encriptar: El texto y la clave no pueden ser nulos.");

        string texto = args[0].ToString();
        string clave = args[1].ToString();

        if (string.IsNullOrEmpty(clave))
            throw new RuntimeError(null!, "cripto.encriptar: La clave no puede estar vacía.");

        try
        {
            // 1. Generar salt e IV únicos para esta encriptación
            var salt  = new byte[SaltSize];
            var nonce = new byte[NonceSize];
            RandomNumberGenerator.Fill(salt);
            RandomNumberGenerator.Fill(nonce);

            // 2. Derivar clave de 256 bits desde la contraseña del usuario
            var keyBytes = DerivarClave(clave, salt);

            // 3. Cifrar con AES-256-GCM
            var plaintext  = Encoding.UTF8.GetBytes(texto);
            var ciphertext = new byte[plaintext.Length];
            var tag        = new byte[TagSize];

            using var aes = new AesGcm(keyBytes, TagSize);
            aes.Encrypt(nonce, plaintext, ciphertext, tag);

            // 4. Empaquetar: [Salt(16)][Nonce(12)][Tag(16)][Ciphertext(N)]
            var payload = new byte[HeaderSize + ciphertext.Length];
            Buffer.BlockCopy(salt,       0, payload, 0,                         SaltSize);
            Buffer.BlockCopy(nonce,      0, payload, SaltSize,                  NonceSize);
            Buffer.BlockCopy(tag,        0, payload, SaltSize + NonceSize,      TagSize);
            Buffer.BlockCopy(ciphertext, 0, payload, HeaderSize,                ciphertext.Length);

            return new LatinoString(Convert.ToBase64String(payload));
        }
        catch (Exception ex) when (ex is not RuntimeError)
        {
            throw new RuntimeError(null!, $"cripto.encriptar: Error interno de encriptación. {ex.Message}");
        }
    }

    // =========================================================================
    // cripto.desencriptar(cifrado, clave)
    // =========================================================================

    [LatinoExtension("cripto.desencriptar",
        Args = "cifrado, clave",
        Help = "Desencripta una cadena producida por cripto.encriptar(). " +
               "Lanza error si la clave es incorrecta o el mensaje fue alterado.")]
    public static LatinoObject Desencriptar(List<LatinoObject> args)
    {
        if (args.Count < 2)
            throw new RuntimeError(null!, "cripto.desencriptar requiere el texto cifrado y la clave.");

        if (args[0] is LatinoNull || args[1] is LatinoNull)
            throw new RuntimeError(null!, "cripto.desencriptar: El texto cifrado y la clave no pueden ser nulos.");

        string cifrado = args[0].ToString();
        string clave   = args[1].ToString();

        if (string.IsNullOrEmpty(clave))
            throw new RuntimeError(null!, "cripto.desencriptar: La clave no puede estar vacía.");

        byte[] payload;
        try
        {
            payload = Convert.FromBase64String(cifrado);
        }
        catch
        {
            throw new RuntimeError(null!,
                "cripto.desencriptar: El texto cifrado no es un Base64 válido. " +
                "¿Fue producido por cripto.encriptar()?");
        }

        if (payload.Length < HeaderSize)
            throw new RuntimeError(null!,
                "cripto.desencriptar: El payload es demasiado corto para ser válido. " +
                "¿Fue producido por cripto.encriptar()?");

        try
        {
            // 1. Extraer componentes del payload
            var salt       = new byte[SaltSize];
            var nonce      = new byte[NonceSize];
            var tag        = new byte[TagSize];
            var ciphertext = new byte[payload.Length - HeaderSize];

            Buffer.BlockCopy(payload, 0,          salt,       0, SaltSize);
            Buffer.BlockCopy(payload, SaltSize,   nonce,      0, NonceSize);
            Buffer.BlockCopy(payload, SaltSize + NonceSize, tag, 0, TagSize);
            Buffer.BlockCopy(payload, HeaderSize, ciphertext, 0, ciphertext.Length);

            // 2. Derivar la misma clave desde la contraseña y el salt almacenado
            var keyBytes  = DerivarClave(clave, salt);
            var plaintext = new byte[ciphertext.Length];

            // 3. Descifrar — lanza AuthenticationTagMismatchException si clave o datos incorrectos
            using var aes = new AesGcm(keyBytes, TagSize);
            aes.Decrypt(nonce, ciphertext, tag, plaintext);

            return new LatinoString(Encoding.UTF8.GetString(plaintext));
        }
        catch (AuthenticationTagMismatchException)
        {
            // Mensaje deliberadamente no técnico — no revelar si falló la clave o el tag
            throw new RuntimeError(null!,
                "cripto.desencriptar: La clave es incorrecta o el mensaje fue alterado.");
        }
        catch (Exception ex) when (ex is not RuntimeError)
        {
            throw new RuntimeError(null!, $"cripto.desencriptar: Error interno. {ex.Message}");
        }
    }

    // =========================================================================
    // cripto.claveAleatoria()
    // =========================================================================

    [LatinoExtension("cripto.claveAleatoria",
        Help = "Genera una clave aleatoria segura de 256 bits codificada en Base64. " +
               "Úsala como clave en encriptar() y desencriptar(). Guárdala en un lugar seguro.")]
    public static LatinoObject ClaveAleatoria(List<LatinoObject> args)
    {
        var clave = new byte[KeySize];
        RandomNumberGenerator.Fill(clave);
        return new LatinoString(Convert.ToBase64String(clave));
    }

    // =========================================================================
    // cripto.firmar(texto, clave)
    // =========================================================================

    [LatinoExtension("cripto.firmar",
        Args = "texto, clave",
        Help = "Genera una firma HMAC-SHA256 del texto usando la clave. " +
               "Retorna la firma en Base64. Úsala para verificar que un mensaje no fue alterado.")]
    public static LatinoObject Firmar(List<LatinoObject> args)
    {
        if (args.Count < 2)
            throw new RuntimeError(null!, "cripto.firmar requiere texto y clave.");

        if (args[0] is LatinoNull || args[1] is LatinoNull)
            throw new RuntimeError(null!, "cripto.firmar: El texto y la clave no pueden ser nulos.");

        string texto = args[0].ToString();
        string clave = args[1].ToString();

        if (string.IsNullOrEmpty(clave))
            throw new RuntimeError(null!, "cripto.firmar: La clave no puede estar vacía.");

        try
        {
            var keyBytes  = Encoding.UTF8.GetBytes(clave);
            var msgBytes  = Encoding.UTF8.GetBytes(texto);
            var firma     = HMACSHA256.HashData(keyBytes, msgBytes);
            return new LatinoString(Convert.ToBase64String(firma));
        }
        catch (Exception ex) when (ex is not RuntimeError)
        {
            throw new RuntimeError(null!, $"cripto.firmar: {ex.Message}");
        }
    }

    // =========================================================================
    // cripto.verificar(texto, firma, clave)
    // =========================================================================

    [LatinoExtension("cripto.verificar",
        Args = "texto, firma, clave",
        Help = "Verifica que la firma HMAC-SHA256 corresponde al texto y la clave. " +
               "Retorna verdadero si la firma es válida, falso si fue alterada. " +
               "Usa comparación en tiempo constante para evitar ataques de timing.")]
    public static LatinoObject Verificar(List<LatinoObject> args)
    {
        if (args.Count < 3)
            throw new RuntimeError(null!, "cripto.verificar requiere texto, firma y clave.");

        if (args[0] is LatinoNull || args[1] is LatinoNull || args[2] is LatinoNull)
            throw new RuntimeError(null!, "cripto.verificar: El texto, la firma y la clave no pueden ser nulos.");

        string texto = args[0].ToString();
        string firma = args[1].ToString();
        string clave = args[2].ToString();

        if (string.IsNullOrEmpty(clave))
            throw new RuntimeError(null!, "cripto.verificar: La clave no puede estar vacía.");

        try
        {
            var keyBytes      = Encoding.UTF8.GetBytes(clave);
            var msgBytes      = Encoding.UTF8.GetBytes(texto);
            var firmaEsperada = HMACSHA256.HashData(keyBytes, msgBytes);

            byte[] firmaRecibida;
            try
            {
                firmaRecibida = Convert.FromBase64String(firma);
            }
            catch
            {
                // Firma no es Base64 válido — definitivamente inválida
                return new LatinoBoolean(false);
            }

            // Comparación en tiempo constante — previene ataques de timing side-channel
            bool esValida = CryptographicOperations.FixedTimeEquals(firmaEsperada, firmaRecibida);
            return new LatinoBoolean(esValida);
        }
        catch (Exception ex) when (ex is not RuntimeError)
        {
            throw new RuntimeError(null!, $"cripto.verificar: {ex.Message}");
        }
    }

    // =========================================================================
    // Helper privado — Derivación de clave PBKDF2-SHA256
    // =========================================================================

    /// <summary>
    /// Deriva 32 bytes de clave AES desde una contraseña arbitraria usando PBKDF2-SHA256.
    /// El salt debe ser único por encriptación y almacenarse junto al payload.
    /// </summary>
    private static byte[] DerivarClave(string password, byte[] salt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations: Pbkdf2Iter,
            HashAlgorithmName.SHA256,
            outputLength: KeySize);
    }
}
