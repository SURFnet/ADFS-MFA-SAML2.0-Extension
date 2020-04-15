using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SURFnet.Authentication.Adfs.Plugin.Configuration
{
    public static class TempSigners
    {
        public static readonly string TestSignerOld =
            "MIIC6jCCAdICCQCWUmXBRox3ZDANBgkqhkiG9w0BAQsFADA3MRkwFwYDVQQDDBBHYXRld2F5IFNB" +
            "TUwgSWRQMRowGAYDVQQKDBFTVVJGc2VjdXJlSUQgVEVTVDAeFw0xODA4MDEwODA2MjdaFw0yMzA3M" +
            "zEwODA2MjdaMDcxGTAXBgNVBAMMEEdhdGV3YXkgU0FNTCBJZFAxGjAYBgNVBAoMEVNVUkZzZWN1cm" +
            "VJRCBURVNUMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA3okLxR7J2re6j7/rLjEYLc7" +
            "iWiELcypmFLvL9BmCYqYZ80Pn+SR9LPUOXcplFUt6LYh/NOK5JMT0P6o0OTUP1P4zEMzLEl0wSJ1j" +
            "Bcu88yNppJoUn/TEgXMGNB1DW8jlVvzcgNSsJjxuw2Fj6J/6D+b77+7PhNMagbnfBEFStz0RBv7JO" +
            "BdzuEC71wVxlGXB7C1Y8ZF3AwZgIp0jOVdiMub9i6neaKV9ZBLSv+azkT8BtAauMdKBpBxC+KxUFV" +
            "9ccHKFnF2YOTLQ3CJNNGyQTtCJVI82fggraKnl+by2elV3+Dmzc0iqMcAdECasSS+2E8iOqw+3Qss" +
            "+RgtS7QvCdQIDAQABMA0GCSqGSIb3DQEBCwUAA4IBAQCq/j+uXLvYDHhL7c/Y3+oj25+ur2UtZ/uS" +
            "BqZIIqGlAzlCEL/zdgDI8XmePaRLtc2hYWUH4bD5Iu8HqxrMPrdBkG/5cjbMmlhU5uV3EX7S+m89k" +
            "9vrok9+7B+uynCkMIdA/1Uif2btfEQi9hevvyP/1vvyoHqftym+ivIOyvELJNIgdTUaqvcJy//Qvk" +
            "mpvSpgTvlzHSVgKkSmMoBhTmevu7lQUGYSk/Mt53Zd3WmZhev+emS/MTKwV39JkZg7aykIRqXGVe/" +
            "yTlttW/zaV9WtSIzNZfaKqASraAaClKgv8lsTjWFv88HZrsP/UuEseIWh4NjOo5HHvHYgqN/atX3t";

        public static readonly X509Certificate2 TestOldCert;
        public static readonly X509Certificate2 TestNewCert;

        static TempSigners()
        {
            TestOldCert = Base64ToX509Cert(TestSignerOld);
        }
        public static X509Certificate2 Base64ToX509Cert(string base64cert)
        {
            byte[] raw = Convert.FromBase64String(base64cert);
            return new X509Certificate2(raw);
        }
    }
}
