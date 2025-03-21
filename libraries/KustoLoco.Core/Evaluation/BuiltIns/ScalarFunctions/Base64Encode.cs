using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl
{
    [KustoImplementation(Keyword = "base64_encode_tostring")]
    internal partial class Base64Encode
    {
        private static string Impl(string plainText)
        {
            try
            {
                return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(plainText));
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
