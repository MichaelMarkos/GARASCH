using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Entities;
using NewGaras.Infrastructure.Helper.TenantService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElectronicInvoice.pages
{
    public class APIEnvironment
    {

        private static GarasTestContext _Context;
        private readonly ITenantService _tenantService;
        public APIEnvironment(ITenantService tenantService)
        {
            _Context = new GarasTestContext(tenantService);
        }

        private EinvoiceSetting EInvoiceSettingObj = _Context.EinvoiceSettings.FirstOrDefault();
        private string apiBaseUrl = "https://api.preprod.invoicing.eta.gov.eg";
        private string idSrvBaseUrl = "https://id.preprod.eta.gov.eg";
        private string clientId = "";
        private string clientSecret1 = "";
        private string clientSecret2 = "";
        #region PI Aroma
        //test

        //private string apiBaseUrl = "https://api.preprod.invoicing.eta.gov.eg";
        //private string idSrvBaseUrl = "https://id.preprod.eta.gov.eg";
        //private string clientId = "357a6a68-b9b1-443d-84f3-2e44d39c749c";
        //private string clientSecret1 = "43c0f0b2-4d66-418b-a400-98f667b61893";
        //private string clientSecret2 = "5c9b24f5-faa5-4ff5-b367-acf47bab8590";
        //---------------------
        // PROD
        //private string apiBaseUrl = "https://api.invoicing.eta.gov.eg";
        //private string idSrvBaseUrl = "https://id.eta.gov.eg";
        //private string clientId = "f4aa8b52-a523-457b-9a10-1edad9ccf611";
        //private string clientSecret1 = "81a54f7a-dac4-4262-81c5-bbf0eb35b70e";
        //private string clientSecret2 = "e3722baf-cbfb-4b01-a349-a0363fdf0ae6";

        public APIEnvironment()
        {
            if (EInvoiceSettingObj != null)
            {
                if (EInvoiceSettingObj.IsProduction)
                {
                    apiBaseUrl = "https://api.invoicing.eta.gov.eg";
                    idSrvBaseUrl = "https://id.eta.gov.eg";
                    clientId = EInvoiceSettingObj.ClientIdProduction;
                    clientSecret1 = EInvoiceSettingObj.Clientsecret1Production;
                    clientSecret2 = EInvoiceSettingObj.Clientsecret2Production;
                }
                else
                {
                    apiBaseUrl = "https://api.preprod.invoicing.eta.gov.eg";
                    idSrvBaseUrl = "https://id.preprod.eta.gov.eg";
                    clientId = EInvoiceSettingObj.ClientIdTest;
                    clientSecret1 = EInvoiceSettingObj.Clientsecret1Test;
                    clientSecret2 = EInvoiceSettingObj.Clientsecret2Test;
                }
            }

        }
        #endregion

        #region El salam
        //test
        //private string apiBaseUrl = "https://api.preprod.invoicing.eta.gov.eg";
        //private string idSrvBaseUrl = "https://id.preprod.eta.gov.eg";
        //private string clientId = "357a6a68-b9b1-443d-84f3-2e44d39c749c";
        //private string clientSecret1 = "43c0f0b2-4d66-418b-a400-98f667b61893";
        //private string clientSecret2 = "5c9b24f5-faa5-4ff5-b367-acf47bab8590";
        //---------------------
        //// PROD
        //private string apiBaseUrl = "https://api.invoicing.eta.gov.eg";
        //private string idSrvBaseUrl = "https://id.eta.gov.eg";
        //private string clientId = "5b1249f5-ec56-48c7-bd06-71cc3e7694d1";
        //private string clientSecret1 = "a38b2493-d98d-444d-bd1e-21dcf0da7bd0";
        //private string clientSecret2 = "9e6bb197-01de-49b2-9525-ca673fae6a51";
        #endregion

        public string ApiBaseUrl
        {
            get
            {
                return apiBaseUrl;
            }
        }
        public string IdSrvBaseUrl
        {
            get
            {
                return idSrvBaseUrl;
            } 
        }
        public string ClientId
        {
            get
            {
                return clientId;
            }      
        }
        public string ClientSecret1
        {
            get
            {
                return clientSecret1;
            }    
        }
        public string ClientSecret2
        {
            get
            {
                return clientSecret2;
            }
        }
      
    }
}