//url reference
//https://stackoverflow.com/questions/46783093/download-all-ssrs-reports
//https://gist.github.com/madcodemonkey/17216111f8ffa8d4515455fb90e1b4e9
//https://stackoverflow.com/questions/55416770/soap-web-service-call-with-ntlm-auth-not-working-c-sharp
//https://www.codeproject.com/Tips/1238108/Publish-RDL-Files-To-SSRS-using-Csharp

using MySSRSReports;
using System.Net;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Xml;


const string REPORTEXECUTION2005ENDPOINTURL = "http://x.x.x.x/ReportServer/ReportService2010.asmx";
const string USERNAME = "xxxxxx";
const string PASSWORD = "xxx";
const string DOMAINNAME = "xxxx";

var rsBinding = new BasicHttpBinding();
rsBinding.Security.Mode = BasicHttpSecurityMode.None;
rsBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
rsBinding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
rsBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
rsBinding.MaxBufferPoolSize = 20000000;
rsBinding.MaxBufferSize = 20000000;
rsBinding.MaxReceivedMessageSize = 20000000;
rsBinding.MessageEncoding = WSMessageEncoding.Text;
rsBinding.TextEncoding = Encoding.UTF8;
rsBinding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;

var rsEndpointAddress = new EndpointAddress(REPORTEXECUTION2005ENDPOINTURL);
var rsClient = new ReportingService2010SoapClient(rsBinding, rsEndpointAddress);

rsClient.ClientCredentials.Windows.ClientCredential = new NetworkCredential(USERNAME, PASSWORD, DOMAINNAME);
rsClient.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Delegation;
var trustedHeader = new TrustedUserHeader();
var ssrsItems = await rsClient.ListChildrenAsync(trustedHeader, "/", true);

foreach (var ssrsItem in ssrsItems.CatalogItems)
{
    if (ssrsItem.TypeName == "Report")
    {
        var ssrsDefinition = await rsClient.GetItemDefinitionAsync(trustedHeader, ssrsItem.Path);
        var memoryStream = new MemoryStream(ssrsDefinition.Definition);
        var ssrsFile = new XmlDocument();
        ssrsFile.Load(memoryStream);
        var fullDataSourceFileName = ssrsItem.Name + ".rdl";
        Console.WriteLine(fullDataSourceFileName);
        //var connStr = StringBetween(ssrsFile.InnerXml, "<ConnectString>", "</ConnectString>");
        //Console.WriteLine(connStr);
        ssrsFile.Save(fullDataSourceFileName);
    }
}


string StringBetween(string STR, string FirstString, string LastString)
{
    if (STR.Contains(FirstString))
    {
        string FinalString;
        int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
        int Pos2 = STR.IndexOf(LastString);
        FinalString = STR.Substring(Pos1, Pos2 - Pos1);
        return FinalString;
    }
    else
        return string.Empty;
}