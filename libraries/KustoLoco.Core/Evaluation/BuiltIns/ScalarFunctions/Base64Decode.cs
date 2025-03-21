using System;

namespace KustoLoco.Core.Evaluation.BuiltIns.Impl
{
    [KustoImplementation(Keyword = "base64_decode_tostring")]
    internal partial class Base64Decode
    {
        private static string Impl(string base64)
        {
            try
            {
                return System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(base64));
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
