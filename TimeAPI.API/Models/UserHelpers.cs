using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Web;
using Twilio;
using Twilio.Rest.Lookups.V1;
using System.Text.RegularExpressions;
using System.Text.Encodings.Web;
using System.Net.Http;

namespace TimeAPI
{
    public static class UserHelpers
    {
        public static string GetUserId(this IPrincipal principal)
        {
            var claimsIdentity = (ClaimsIdentity)principal.Identity;
            var claim = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return claim.Value;
        }

        public static string Encrypt(string toEncrypt, bool useHashing, string SecurityKey)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            // Get the key from config file
            string key = SecurityKey;

            //System.Windows.Forms.MessageBox.Show(key);
            //If hashing use get hashcode regards to your key
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                //Always release the resources and flush data
                // of the Cryptographic service provide. Best Practice

                hashmd5.Clear();
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes(key);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            //mode of operation. there are other 4 modes.
            //We choose ECB(Electronic code Book)
            tdes.Mode = CipherMode.ECB;
            //padding mode(if any extra byte added)

            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            //transform the specified region of bytes array to resultArray
            byte[] resultArray =
              cTransform.TransformFinalBlock(toEncryptArray, 0,
              toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor
            tdes.Clear();
            //Return the encrypted data into unreadable string format
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public static string Decrypt(string cipherString, bool useHashing, string SecurityKey)
        {
            byte[] keyArray;
            //get the byte code of the string

            byte[] toEncryptArray = Convert.FromBase64String(cipherString);

            //Get your key from config file to open the lock!
            string key = SecurityKey;

            if (useHashing)
            {
                //if hashing was used get the hash code with regards to your key
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
                //release any resource held by the MD5CryptoServiceProvider

                hashmd5.Clear();
            }
            else
            {
                //if hashing was not implemented get the byte code of the key
                keyArray = UTF8Encoding.UTF8.GetBytes(key);
            }

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            //set the secret key for the tripleDES algorithm
            tdes.Key = keyArray;
            //mode of operation. there are other 4 modes. 
            //We choose ECB(Electronic code Book)

            tdes.Mode = CipherMode.ECB;
            //padding mode(if any extra byte added)
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(
                                 toEncryptArray, 0, toEncryptArray.Length);
            //Release resources held by TripleDes Encryptor                
            tdes.Clear();
            //return the Clear decrypted TEXT
            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        public static string HMACSHA1(string key, string dataToSign)
        {
            Byte[] secretBytes = UTF8Encoding.UTF8.GetBytes(key);
            HMACSHA1 hmac = new HMACSHA1(secretBytes);

            Byte[] dataBytes = UTF8Encoding.UTF8.GetBytes(dataToSign);
            Byte[] calcHash = hmac.ComputeHash(dataBytes);
            String calcHashString = Convert.ToBase64String(calcHash);
            return calcHashString;
        }

        public static string Encryptx(string clearText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public static string Decryptx(string cipherText)
        {
            string EncryptionKey = "MAKV2SPBNI99212";
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        public static bool IsLocal(this HttpRequest req)
        {
            var connection = req.HttpContext.Connection;
            if (connection.RemoteIpAddress != null)
            {
                if (connection.LocalIpAddress != null)
                {
                    return connection.RemoteIpAddress.Equals(connection.LocalIpAddress);
                }
                return IPAddress.IsLoopback(connection.RemoteIpAddress);
            }

            // for in memory TestServer or when dealing with default connection info
            if (connection.RemoteIpAddress == null && connection.LocalIpAddress == null)
            {
                return true;
            }

            return false;
        }

        public static string GeneratePassword() => Guid.NewGuid()
            .ToString("N")
            .ToLower(CultureInfo.CurrentCulture)
            .Replace("1", "")
            .Replace("o", "")
            .Replace("0", "")
            .Substring(0, 8) + "@";

        public static async Task<string> ShortenAsync(string longUrl, string _bitlyToken)
        {
            string shortern = string.Empty;
            var url = string.Format("https://api-ssl.bitly.com/v3/shorten?access_token={0}&longUrl={1}", _bitlyToken, longUrl);

            var request = (HttpWebRequest)WebRequest.Create(url);
            try
            {
                var response = request.GetResponse();
                using (var responseStream = response.GetResponseStream())
                {
                    var reader = new StreamReader(responseStream, Encoding.UTF8);
                    var jsonResponse = JObject.Parse(await reader.ReadToEndAsync().ConfigureAwait(true));
                    var statusCode = jsonResponse["status_code"].Value<int>();
                    if (statusCode == (int)HttpStatusCode.OK)
                    {
                        shortern = jsonResponse["data"]["url"].Value<string>();
                        return shortern;
                    }

                    //else some sort of problem
                    var data = ("Bitly request returned error code {0}, status text '{1}' on longUrl = {2}",
                        statusCode, jsonResponse["status_txt"].Value<string>(), longUrl);
                    //What to do if it goes wrong? I return the original long url
                    return longUrl;
                }
            }
            catch (WebException ex)
            {
                var errorResponse = ex.Response;
                using (var responseStream = errorResponse.GetResponseStream())
                {
                    var reader = new StreamReader(responseStream, Encoding.GetEncoding("utf-8"));
                    var errorText = reader.ReadToEnd();
                    // log errorText
                    var data = ("Bitly access threw an exception {0} on url {1}. Content = {2}", ex.Message, url, errorText);
                }
                //What to do if it goes wrong? I return the original long url
                return longUrl;
            }
        }

        public static string IsPhoneValid(string phone)
        {
            string _phone = string.Empty;
            if (phone == null)
            {
                return _phone;
            }
            if (phone == "")
            {
                return _phone;
            }

            if (phone.Contains("+"))
                _phone = phone.Substring(1);
            else
                _phone = phone;

            return _phone;
        }

        public static string ValidatePhoneNumber(string args)
        {
            if (UserHelpers.ValidateEmailOrPhone(args).Equals("EMAIL"))
                return "EMAIL";

            const string accountSid = "ACc574d18f169071a8b477170e3b867b1c";
            const string authToken = "b46ffb4385b38a0502d25f26d6250ee2";

            TwilioClient.Init(accountSid, authToken);

            try
            {
                var phoneNumber = PhoneNumberResource.Fetch(pathPhoneNumber: new Twilio.Types.PhoneNumber(args));
                string Number = phoneNumber.PhoneNumber.ToString();
                return "VALID";
            }
            catch (Exception ex)
            {
                return "INVALID";
            }
        }

        public static string ValidateEmailOrPhone(string _userName)
        {
            string Result = string.Empty;
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Regex r = new Regex(@"^\+?(\d[\d-]+)?(\([\d-]+\))?[\d-]+\d$");
            if (r.IsMatch(_userName))
                Result = "PHONE";
            else if (regex.IsMatch(_userName))
                Result = "EMAIL";

            return Result;
        }
    }

    public static class InternetTime
    {
        public static DateTimeOffset? GetCurrentTimeFromTimeZone()
        {
            DateTime eastern = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Arabian Standard Time");
            return eastern;
        }
    }
}
