using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using EInvoice.App_code.Oracle.Model;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Response;
using NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Inputs;
using NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Oracle.BL;
using NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Oracle.Model;
namespace ElectronicInvoice.pages
{
    public class APICall
    {
        APIEnvironment env;
        string Accesstoken = "";
        ITenantService _tenantService;
        IWebHostEnvironment _host;
        public APICall(ITenantService tenantService,IWebHostEnvironment host)
        {
            env = new APIEnvironment();
            _tenantService = tenantService;
            _host = host;
        }
        public void LoginAsTaxpayerSystem()
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(@"" + env.IdSrvBaseUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                var data = new[]
                    {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("scope", "InvoicingAPI"),
                    new KeyValuePair<string, string>("client_id", env.ClientId),
                    new KeyValuePair<string, string>("client_secret", env.ClientSecret1)
                };
                var response = client.PostAsync("connect/token", new FormUrlEncodedContent(data)).GetAwaiter().GetResult();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var response_content = response.Content.ReadAsStringAsync();
                    AuthenticateResult res = JsonConvert.DeserializeObject<AuthenticateResult>(response_content.Result);
                    DateTime now = DateTime.Now.AddHours(res.expires_in / 3600);
                    Accesstoken = res.access_token;
                    AuthenticateResultByUser resByUser = new AuthenticateResultByUser(res, now);
                }

            }
        }
        public string documentsubmissions(long invoiceID, long clientAddressID, long cusID, long salesOfferID)
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                using (HttpClient client = new HttpClient())
                {
                    String popupMsg = "";
                    LoginAsTaxpayerSystem();
                    client.BaseAddress = new Uri(@"" + env.ApiBaseUrl);
                    // client.Timeout = new TimeSpan(100000000);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Accesstoken);

                    InvoiceBL invHModel = new InvoiceBL(_tenantService);
                    List<InvoiceHeaderModel> invoiceHeaderList = invHModel.oracleGetInvoiceHeader(invoiceID);
                    if (invoiceHeaderList[0].TrxType.CompareTo("Invoice") != 0)
                    {
                        #region Start to Create Body Response Without Signature
                        CreaditOrDebitDocument d = new CreaditOrDebitDocument();
                        d.documents = new List<CreditOrDebitDocumentElement>();
                        CreditOrDebitDocumentElement element = new CreditOrDebitDocumentElement();
                        if (invoiceHeaderList[0].TrxType.CompareTo("Credit Note") == 0)
                            element.documentType = "c";
                        else
                            element.documentType = "d";
                        //#########################################################################
                        //############################ ISSUER #####################################
                        //#########################################################################
                        IssuerBL issuerBL = new IssuerBL(_tenantService);
                        IssuerModel issuerModel = issuerBL.GetProfileInfo(clientAddressID);
                        element.issuer = new Issuer();
                        element.issuer.id = issuerModel.RegNum;// "100710840";
                        element.issuer.name = issuerModel.CompanyName;//"مكتبة الاسكندرية";
                        element.issuer.type = issuerModel.Type;//"B";
                        element.issuer.address = new IssuerAddress();
                        element.issuer.address.country = issuerModel.Country;// "EG";
                        element.issuer.address.governate = issuerModel.Governate;// "Alexandria";
                        element.issuer.address.street = issuerModel.Street;// "0 / 1115   شارع بورسعيد امام المجمع النظرى الشاطبى الاسكندرية";
                        element.issuer.address.regionCity = issuerModel.RegionCity;//"XX";
                        element.issuer.address.branchID = issuerModel.BranchID;// "0";
                        element.issuer.address.buildingNumber = issuerModel.BuildNum;// "1";             
                                                                                     //##OPtinal
                        element.issuer.address.floor = "0";
                        element.issuer.address.landmark = "";
                        element.issuer.address.postalCode = "0";
                        element.issuer.address.room = "0";
                        element.issuer.address.additionalInformation = "";
                        //#########################################################################
                        //########################### RECEIVER ####################################
                        //#########################################################################
                        CustomerBL cusModel = new CustomerBL(_tenantService);
                        List<CustomerModel> customerList = cusModel.oracleGetCustomer(cusID);
                        element.receiver = new Receiver();
                        element.receiver.id = customerList[0].TaxpayerCode;//"100295584" //;
                        element.receiver.name = customerList[0].PartyName;//"شركة لابوار للمحلات السياحية";
                        element.receiver.type = customerList[0].CusType;// "B";
                        element.receiver.address = new ReceiverAddress();
                        element.receiver.address.country = customerList[0].Country;// "EG";
                        element.receiver.address.street = customerList[0].Street; //"3شارع شجرة الدر القاهرة";
                        element.receiver.address.regionCity = customerList[0].RegionCity;// "الزمالك";
                        element.receiver.address.governate = customerList[0].Governate;// "Alexandria";               
                        element.receiver.address.buildingNumber = customerList[0].BuildingNumber;// "1";
                                                                                                 //##OPtinal
                        element.receiver.address.floor = "0";
                        element.receiver.address.landmark = "";
                        element.receiver.address.postalCode = "0";
                        element.receiver.address.room = "0";
                        element.receiver.address.additionalInformation = "";
                        //##########################################################################
                        //########################### INVOICE DATA #################################
                        //##########################################################################  
                        element.documentTypeVersion = "1.0";
                        element.references = new List<string>();

                        String invoiceUUID = invHModel.getUUID(invoiceID);
                        if (invoiceUUID.CompareTo("") == 0)
                        {
                            popupMsg = "THIS CREDIT NOTE IS NOT FOR A SUBMITTED INVOICE";
                            return popupMsg;
                        }
                        element.references.Add(invoiceUUID);
                        if (invoiceHeaderList[0].Trx_date != null)
                        {
                            DateTime date = DateTime.Now;
                            TimeSpan time = new TimeSpan(0, DateTime.Now.Hour, 0, 0);
                            element.dateTimeIssued = ((DateTime)invoiceHeaderList[0].Trx_date).Add(time).AddHours(-4).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
                        }
                        else
                        {
                            element.dateTimeIssued = DateTime.Now.AddHours(-4).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
                        }
                        //DateTime.Now.AddHours(-2).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");

                        //element.invoiceID = invoiceID;
                        element.taxpayerActivityCode = issuerModel.ActivityCode1;// "9101";//--->انشطة المكتبات والارشيف
                        element.internalID = invoiceHeaderList[0].Trx_number;// "3220002";//----->رقم داخلي للفاتورة                   
                                                                             //##OPtinal
                        element.purchaseOrderReference = "";
                        element.purchaseOrderDescription = "";
                        element.salesOrderDescription = "";
                        element.salesOrderReference = "";
                        element.proformaInvoiceNumber = "";
                        //payment
                        element.payment = new Payment();
                        element.payment.bankAccountIBAN = "";
                        element.payment.bankAccountNo = "";
                        element.payment.bankAddress = "";
                        element.payment.bankName = "";
                        element.payment.swiftCode = "";
                        element.payment.terms = "";
                        //delivery
                        element.delivery = new Delivery();
                        element.delivery.grossWeight = 0;
                        element.delivery.approach = "";
                        element.delivery.exportPort = "";
                        element.delivery.terms = "";
                        element.delivery.packaging = "";
                        element.delivery.netWeight = 0;
                        element.delivery.dateValidity = "";
                        element.signatures = new List<Signature>();
                        Signature s = new Signature();
                        s.signatureType = "I";
                        s.value = "MIIGywYJKoZIhvcNAQcCoIIGvDCCBrgCAQMxDTALBglghkgBZQMEAgEwCwYJKoZIhvcNAQcFoIID/zCCA/swggLjoAMCAQICEEFkOqRVlVar0F0n3FZOLiIwDQYJKoZIhvcNAQELBQAwSTELMAkGA1UEBhMCRUcxFDASBgNVBAoTC0VneXB0IFRydXN0MSQwIgYDVQQDExtFZ3lwdCBUcnVzdCBDb3Jwb3JhdGUgQ0EgRzIwHhcNMjAwMzMxMDAwMDAwWhcNMjEwMzMwMjM1OTU5WjBgMRUwEwYDVQQKFAxFZ3lwdCBUcnVzdCAxGDAWBgNVBGEUD1ZBVEVHLTExMzMxNzcxMzELMAkGA1UEBhMCRUcxIDAeBgNVBAMMF1Rlc3QgU2VhbGluZyBEZW1vIHVzZXIyMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEApmVGVJtpImeq\u002BtIJiVWSkIEEOTIcnG1XNYQOYtf5\u002BDg9eF5H5x1wkgR2G7dvWVXrTsdNv2Q\u002Bgvml9SdfWxlYxaljg2AuBrsHFjYVEAQFI37EW2K7tbMT7bfxwT1M5tbjxnkTTK12cgwxPr2LBNhHpfXp8SNyWCxpk6eyJb87DveVwCLbAGGXO9mhDj62glVTrCFit7mHC6bZ6MOMAp013B8No9c8xnrKQiOb4Tm2GxBYHFwEcfYUGZNltGZNdVUtu6ty\u002BNTrSRRC/dILeGHgz6/2pgQPk5OFYRTRHRNVNo\u002BjG\u002BnurUYkSWxA4I9CmsVt2FdeBeuvRFs/U1I\u002BieKg1wIDAQABo4HHMIHEMAkGA1UdEwQCMAAwVAYDVR0fBE0wSzBJoEegRYZDaHR0cDovL21wa2ljcmwuZWd5cHR0cnVzdC5jb20vRWd5cHRUcnVzdENvcnBvcmF0ZUNBRzIvTGF0ZXN0Q1JMLmNybDAdBgNVHQ4EFgQUqzFDImtytsUbghbmtnl2/k4d5jEwEQYJYIZIAYb4QgEBBAQDAgeAMB8GA1UdIwQYMBaAFCInP8ziUIPmu86XJUWXspKN3LsFMA4GA1UdDwEB/wQEAwIGwDANBgkqhkiG9w0BAQsFAAOCAQEAxE3KpyYlPy/e3\u002B6jfz5RqlLhRLppWpRlKYUvH1uIhCNRuWaYYRchw1xe3jn7bLKbNrUmey\u002BMRwp1hZptkxFMYKTIEnNjOKCrLmVIuPFcfLXAQFq5vgLDSbnUhG/r5D\u002B50ndPucyUPhX3gw8gFlA1R\u002BtdNEoeKqYSo9v3p5qNANq12OuZbkhPg6sAD4dojWoNdlkc8J2ML0eq4a5AQvb4yZVb\u002BezqJyqKj83RekRZi0kMxoIm8l82CN8I/Bmp6VVNJRhQKhSeb7ShpdkZcMwcfKdDw6LW02/XcmzVl8NBBbLjKSJ/jxdL1RxPPza7RbGqSx9pfyav5\u002BAxO9sXnXXc5jGCApIwggKOAgEBMF0wSTELMAkGA1UEBhMCRUcxFDASBgNVBAoTC0VneXB0IFRydXN0MSQwIgYDVQQDExtFZ3lwdCBUcnVzdCBDb3Jwb3JhdGUgQ0EgRzICEEFkOqRVlVar0F0n3FZOLiIwCwYJYIZIAWUDBAIBoIIBCjAYBgkqhkiG9w0BCQMxCwYJKoZIhvcNAQcFMBwGCSqGSIb3DQEJBTEPFw0yMTAyMDEyMzUwMjFaMC8GCSqGSIb3DQEJBDEiBCD5bGXJu9uJZIPMGXK98UrHzJM/V2U/WAO6BErxpX5wdTCBngYLKoZIhvcNAQkQAi8xgY4wgYswgYgwgYUEIAJA8uO/ek3l9i3ZOgRtPhGWwwFYljbeJ7yAgEnyYNCWMGEwTaBLMEkxCzAJBgNVBAYTAkVHMRQwEgYDVQQKEwtFZ3lwdCBUcnVzdDEkMCIGA1UEAxMbRWd5cHQgVHJ1c3QgQ29ycG9yYXRlIENBIEcyAhBBZDqkVZVWq9BdJ9xWTi4iMAsGCSqGSIb3DQEBAQSCAQB13E1WX\u002BzbWppfJi3DBK9MMSB1TXuxcNkGXQ19OcRUUAaAe2K\u002BisobYrUCZbi3ygc2AWOMyafboxjjomzrnvXKrFgspT4wAFPYaAGFzKWq\u002BW/nqMhIqJVIpS/NM7Al4HvuBA5iGuZEQFusElB0yIxOIiYDI4v8Ilkff4/duj/V2CNaN5cqXLOpL5RP6Y5i\u002BVsPGb89t/L0dSIldGN0JqaqarqYo5/RwsUFJJq01DFpPGNbOIX3gSCDmycfhJPS9csnne9Zt\u002BabNpja5ZR6KA8JMe4DHes7FDZqHBNHdC\u002BRDXT4crqmnyiJjizULu6MqDc0Fv3vrMMWDLRlwDecgq7i";
                        element.signatures.Add(s);
                        //##########################################################################
                        //########################### INVOICE LINES ################################
                        //##########################################################################                
                        InvoiceBL invLineBL = new InvoiceBL(_tenantService);
                        List<InvoiceLineModel> invLine = invLineBL.oracleGetInvoiceLine(salesOfferID);
                        element.invoiceLines = new List<InvoiceLine>();
                        ItemBL itmModel = new ItemBL(_tenantService);
                        int line = 0;
                        int length = invLine.Count;
                        decimal totSalesAmount = 0;
                        decimal netTotal = 0;
                        decimal totalAmount = 0;
                        decimal totalDiscount = 0;
                        List<TaxTotal> t = new List<TaxTotal>();
                        for (line = 0; line < length; line++)
                        {
                            //LINE

                            List<ItemModel> item = itmModel.oracleGetItem((long)invLine[line].ItemID);
                            InvoiceLine l1 = new InvoiceLine();

                            // Get Item From Offer 
                            
                            l1.description = itmModel.GetItemOfferPricingComment(invLine[line].SalesOfferProductID1);
                            l1.internalCode = item[0].ItemInternalCode;// "1371"; 
                            l1.itemType = item[0].ItemTypeEINVOICE;//"EGS ";//---->
                            l1.itemCode = item[0].ItemCode;// "EG-100710840-1371";
                            l1.unitType = item[0].ItemUnitTypeEINVOICE;// "EA";
                            l1.quantity = (Decimal)invLine[line].Quantity1;// Decimal.Parse(invLine[line].Quantity1.ToString() );//1;
                            UnitValue uv = new UnitValue();
                            uv.amountEGP = (Decimal)invLine[line].ItemPrice1;//2000;
                            uv.currencySold = "EGP";
                            l1.unitValue = uv;
                            l1.salesTotal = (Decimal)invLine[line].SalesTotal;//2000;
                            totSalesAmount += l1.salesTotal;
                            //DISCOUNT
                            Discount dis = new Discount();
                            dis.amount = l1.salesTotal * (Decimal)invLine[line].DiscountPercentage1 / 100;
                            dis.rate = (Decimal)invLine[line].DiscountPercentage1;
                            l1.discount = dis;
                            totalDiscount += dis.amount;
                            //--------------------------
                            l1.netTotal = (Decimal)invLine[line].SalesTotal - (l1.discount.amount);// 2000;                   
                            netTotal += l1.netTotal;
                            l1.valueDifference = 0;
                            l1.itemsDiscount = 0;
                            //----------------------------------------------------------------------------------------------------------------------
                            List<InvoiceLineTaxModel> TaxItem = invHModel.oracleGetInvoiceLineTax(salesOfferID, invLine[line].SalesOfferProductID1);
                            l1.taxableItems = new List<TaxableItem>();
                            //--TAX
                            decimal totalOfTaxableTax = 0;
                            decimal totalTaxPerLine = 0;
                            decimal T4Amount = 0;
                            foreach (InvoiceLineTaxModel taxItem in TaxItem)
                            {
                                TaxableItem ti = new TaxableItem();
                                if (taxItem.TaxName1.CompareTo("T1") == 0)
                                {
                                    ti.amount = Decimal.Parse(String.Format("{0:0.00000}", (l1.netTotal + totalOfTaxableTax) * (Decimal)taxItem.TaxPercentage1 / 100));
                                    ti.subType = taxItem.SubTaxName1;
                                    ti.taxType = taxItem.TaxName1;
                                    ti.rate = Decimal.Parse(String.Format("{0:0.00}", taxItem.TaxPercentage1));
                                    if (line == 0)
                                    {
                                        TaxTotal tt = new TaxTotal();
                                        tt.amount = ti.amount;
                                        tt.taxType = taxItem.TaxName1;
                                        t.Add(tt);
                                    }
                                    else
                                    {
                                        bool found = false;
                                        for (int i = 0; i < t.Count; i++)
                                        {
                                            if (t[i].taxType.CompareTo(taxItem.TaxName1) == 0)
                                            {
                                                t[i].amount += ti.amount;
                                                found = true;
                                                break;
                                            }
                                        }
                                        if (found == false)
                                        {
                                            TaxTotal tt = new TaxTotal();
                                            tt.amount = ti.amount;
                                            tt.taxType = taxItem.TaxName1;
                                            t.Add(tt);
                                        }
                                    }
                                }
                                else if (taxItem.TaxName1.CompareTo("T4") == 0) // Michael Modified
                                {
                                    if (taxItem.IsPercentage1 == true) // Percentage Always
                                        ti.amount = Decimal.Parse(String.Format("{0:0.00000}", l1.netTotal * (Decimal)taxItem.TaxPercentage1 / 100));
                                    else
                                        ti.amount = (Decimal)taxItem.TaxValue1;

                                    ti.subType = taxItem.SubTaxName1;
                                    ti.taxType = taxItem.TaxName1;
                                    if (taxItem.IsPercentage1 == true)
                                        ti.rate = Decimal.Parse(String.Format("{0:0.00}", taxItem.TaxPercentage1));
                                    else
                                        ti.rate = 0;

                                    if (line == 0)
                                    {
                                        TaxTotal tt = new TaxTotal();
                                        tt.amount = ti.amount;
                                        tt.taxType = taxItem.TaxName1;
                                        t.Add(tt);
                                    }
                                    else
                                    {
                                        bool found = false;
                                        for (int i = 0; i < t.Count; i++)
                                        {
                                            if (t[i].taxType.CompareTo(taxItem.TaxName1) == 0)
                                            {
                                                t[i].amount += ti.amount;
                                                found = true;
                                                break;
                                            }
                                        }
                                        if (found == false)
                                        {
                                            TaxTotal tt = new TaxTotal();
                                            tt.amount = ti.amount;
                                            tt.taxType = taxItem.TaxName1;
                                            t.Add(tt);
                                        }
                                    }
                                    T4Amount = ti.amount;
                                    //ti.amount = -1 * ti.amount; // Special case for T4  only subtract from totalTaxPerLine not form totalOfTaxableTax
                                    //T4Amount = ti.amount;
                                    //if (taxItem.TaxType1.CompareTo("Taxable Types") == 0)
                                    //    totalOfTaxableTax -= ti.amount;
                                }
                                else
                                {
                                    if (taxItem.IsPercentage1 == true)
                                        ti.amount = Decimal.Parse(String.Format("{0:0.00000}", l1.netTotal * (Decimal)taxItem.TaxPercentage1 / 100));
                                    else
                                        ti.amount = (Decimal)taxItem.TaxValue1;

                                    ti.subType = taxItem.SubTaxName1;
                                    ti.taxType = taxItem.TaxName1;
                                    if (taxItem.IsPercentage1 == true)
                                        ti.rate = Decimal.Parse(String.Format("{0:0.00}", taxItem.TaxPercentage1));
                                    else
                                        ti.rate = 0;

                                    if (line == 0)
                                    {
                                        TaxTotal tt = new TaxTotal();
                                        tt.amount = ti.amount;
                                        tt.taxType = taxItem.TaxName1;
                                        t.Add(tt);
                                    }
                                    else
                                    {
                                        bool found = false;
                                        for (int i = 0; i < t.Count; i++)
                                        {
                                            if (t[i].taxType.CompareTo(taxItem.TaxName1) == 0)
                                            {
                                                t[i].amount += ti.amount;
                                                found = true;
                                                break;
                                            }
                                        }
                                        if (found == false)
                                        {
                                            TaxTotal tt = new TaxTotal();
                                            tt.amount = ti.amount;
                                            tt.taxType = taxItem.TaxName1;
                                            t.Add(tt);
                                        }
                                    }
                                    if (taxItem.TaxType1.CompareTo("Taxable Types") == 0)
                                        totalOfTaxableTax += ti.amount;
                                }

                                if (taxItem.TaxName1.CompareTo("T4") != 0)
                                {
                                    totalTaxPerLine += ti.amount;
                                }
                                l1.taxableItems.Add(ti);
                            }
                            //-----------------------------------------------------------------------------------------------------------------------
                            l1.totalTaxableFees = totalOfTaxableTax;
                            l1.total = l1.netTotal + totalTaxPerLine - T4Amount;
                            totalAmount += l1.total;
                            //----------------------------------------------------
                            element.invoiceLines.Add(l1);
                        }
                        //##########################################################################
                        element.totalSalesAmount = totSalesAmount;
                        element.totalDiscountAmount = totalDiscount;
                        element.netAmount = netTotal;
                        element.extraDiscountAmount = 0;
                        element.totalItemsDiscountAmount = 0;
                        element.totalAmount = totalAmount;
                        //##########################################################################
                        element.taxTotals = new List<TaxTotal>();
                        for (int i = 0; i < t.Count; i++)
                        {
                            element.taxTotals.Add(t[i]);
                        }
                        //##########################################################################
                        d.documents.Add(element);
                        #endregion

                        #region Attach Signature
                        //String uridirectory = Path.GetPathRoot(Environment.SystemDirectory) + "\\Signature";
                        //string directory = new Uri(uridirectory).LocalPath;
                        //File.WriteAllText(@"" + directory + "\\SourceDocumentJson.json", JsonConvert.SerializeObject(d.documents[0], Formatting.Indented));
                        ////RUN BATCH
                        //runBatchFile();
                        ////  READ OUTPUT FILE
                        //string json = "";
                        //using (StreamReader r = new StreamReader(@"" + directory + "\\FullSignedDocument.json"))
                        //{
                        //    json = r.ReadToEnd();
                        //}
                        #endregion

                        # region Start To Send Body
                        var submissionText = new StringContent(JsonConvert.SerializeObject(d, Formatting.Indented), Encoding.UTF8, "application/json");

                        var response = client.PostAsync("api/v1.0/documentsubmissions", submissionText).GetAwaiter().GetResult();
                        var response_content = response.Content.ReadAsStringAsync();
                        documentsubmissionsResult SubmissionResult = JsonConvert.DeserializeObject<documentsubmissionsResult>(response_content.Result);
                        popupMsg = response_content.Result;
                        if (SubmissionResult.acceptedDocuments != null && SubmissionResult.acceptedDocuments.Count > 0)
                        {
                            string internalId = SubmissionResult.acceptedDocuments[0].internalId;
                            string longId = SubmissionResult.acceptedDocuments[0].longId;
                            string uuid = SubmissionResult.acceptedDocuments[0].uuid;

                            popupMsg = popupMsg + "acceptedDocuments count : " + SubmissionResult.acceptedDocuments.Count + Environment.NewLine + " internalId : " + internalId + Environment.NewLine +
                                " longId : " + longId + Environment.NewLine + " uuid : " + uuid;

                            invLineBL.saveInvoiceUUID(invoiceID, uuid, "Accepted", DateTime.Now);

                        }
                        if (SubmissionResult.rejectedDocuments != null && SubmissionResult.rejectedDocuments.Count > 0)
                        {
                            string code = SubmissionResult.rejectedDocuments[0].error.code;
                            string target = SubmissionResult.rejectedDocuments[0].error.target;
                            string message = SubmissionResult.rejectedDocuments[0].error.message;
                            string details = "";
                            for (int e = 0; e < SubmissionResult.rejectedDocuments[0].error.details.Length; e++)
                            {
                                details = details + "error " + (e + 1) + Environment.NewLine + " Code:" + SubmissionResult.rejectedDocuments[0].error.details[e].code + Environment.NewLine + " Target:"
                                    + SubmissionResult.rejectedDocuments[0].error.details[e].target + Environment.NewLine
                                    + "Message" + SubmissionResult.rejectedDocuments[0].error.details[e].message + Environment.NewLine;
                            }
                            popupMsg = popupMsg + "rejectedDocuments count : " + SubmissionResult.rejectedDocuments.Count + Environment.NewLine + " code : " + code + Environment.NewLine
                                + "  target : " + target + Environment.NewLine + "  message : "
                                + message
                             + Environment.NewLine + "  details : " + details + Environment.NewLine;

                            invLineBL.saveInvoiceUUID(invoiceID, "", "Rejected " + details, DateTime.Now);
                        }
                        #endregion
                    }
                    else
                    {

                        Document d = new Document();
                        d.documents = new List<DocumentElement>();
                        DocumentElement element = new DocumentElement();
                        element.documentType = "I";
                        //#########################################################################
                        //############################ ISSUER #####################################
                        //#########################################################################
                        IssuerBL issuerBL = new IssuerBL(_tenantService);
                        IssuerModel issuerModel = issuerBL.GetProfileInfo(clientAddressID);
                        element.issuer = new Issuer();
                        element.issuer.id = issuerModel.RegNum;// "100710840";
                        element.issuer.name = issuerModel.CompanyName;//"مكتبة الاسكندرية";
                        element.issuer.type = issuerModel.Type;//"B";
                        element.issuer.address = new IssuerAddress();
                        element.issuer.address.country = issuerModel.Country;// "EG";
                        element.issuer.address.governate = issuerModel.Governate;// "Alexandria";
                        element.issuer.address.street = issuerModel.Street;// "0 / 1115   شارع بورسعيد امام المجمع النظرى الشاطبى الاسكندرية";
                        element.issuer.address.regionCity = issuerModel.RegionCity;//"XX";
                        element.issuer.address.branchID = issuerModel.BranchID;// "0";
                        element.issuer.address.buildingNumber = issuerModel.BuildNum;// "1";             
                                                                                     //##OPtinal
                        element.issuer.address.floor = "0";
                        element.issuer.address.landmark = "";
                        element.issuer.address.postalCode = "0";
                        element.issuer.address.room = "0";
                        element.issuer.address.additionalInformation = "";
                        //#########################################################################
                        //########################### RECEIVER ####################################
                        //#########################################################################
                        CustomerBL cusModel = new CustomerBL(_tenantService);
                        List<CustomerModel> customerList = cusModel.oracleGetCustomer(cusID);
                        element.receiver = new Receiver();
                        element.receiver.id = customerList[0].TaxpayerCode;//"100295584";
                        element.receiver.name = customerList[0].PartyName;//"شركة لابوار للمحلات السياحية";
                        element.receiver.type = customerList[0].CusType;// "B";
                        element.receiver.address = new ReceiverAddress();
                        element.receiver.address.country = customerList[0].Country;// "EG";
                        element.receiver.address.street = customerList[0].Street; //"3شارع شجرة الدر القاهرة";
                        element.receiver.address.regionCity = customerList[0].RegionCity;// "الزمالك";
                        element.receiver.address.governate = customerList[0].Governate;// "Alexandria";               
                        element.receiver.address.buildingNumber = customerList[0].BuildingNumber;// "1";
                                                                                                 //##OPtinal
                        element.receiver.address.floor = "0";
                        element.receiver.address.landmark = "";
                        element.receiver.address.postalCode = "0";
                        element.receiver.address.room = "0";
                        element.receiver.address.additionalInformation = "";
                        //##########################################################################
                        //########################### INVOICE DATA #################################
                        //##########################################################################  
                        if (invoiceHeaderList[0].Trx_date != null)
                        {
                            DateTime date = DateTime.Now;
                            TimeSpan time = new TimeSpan(0, DateTime.Now.Hour, 0, 0);
                            element.dateTimeIssued = ((DateTime)invoiceHeaderList[0].Trx_date).Add(time).AddHours(-4).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
                        }
                        else
                        {
                            element.dateTimeIssued = DateTime.Now.AddHours(-4).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
                        }
                        //element.dateTimeIssued =// invoiceHeaderList[0].Trx_date.AddHours(-2).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
                        //    DateTime.Now.AddHours(-2).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");                   
                        element.documentTypeVersion = "0.9";
                        //element.invoiceID = invoiceID;
                        element.taxpayerActivityCode = issuerModel.ActivityCode1;// "9101";//--->انشطة المكتبات والارشيف
                        element.internalID = invoiceHeaderList[0].Trx_number;// "3220002";//----->رقم داخلي للفاتورة 
                                                                             //##OPtinal
                        element.purchaseOrderReference = "";
                        element.purchaseOrderDescription = "";
                        element.salesOrderDescription = "";
                        element.salesOrderReference = "";
                        element.proformaInvoiceNumber = "";
                        //payment
                        element.payment = new Payment();
                        element.payment.bankAccountIBAN = "";
                        element.payment.bankAccountNo = "";
                        element.payment.bankAddress = "";
                        element.payment.bankName = "";
                        element.payment.swiftCode = "";
                        element.payment.terms = "";
                        //delivery
                        element.delivery = new Delivery();
                        element.delivery.grossWeight = 0;
                        element.delivery.approach = "";
                        element.delivery.exportPort = "";
                        element.delivery.terms = "";
                        element.delivery.packaging = "";
                        element.delivery.netWeight = 0;
                        element.delivery.dateValidity = "";
                        element.signatures = new List<Signature>();
                        Signature s = new Signature();
                        s.signatureType = "I";
                        s.value = "MIIGywYJKoZIhvcNAQcCoIIGvDCCBrgCAQMxDTALBglghkgBZQMEAgEwCwYJKoZIhvcNAQcFoIID/zCCA/swggLjoAMCAQICEEFkOqRVlVar0F0n3FZOLiIwDQYJKoZIhvcNAQELBQAwSTELMAkGA1UEBhMCRUcxFDASBgNVBAoTC0VneXB0IFRydXN0MSQwIgYDVQQDExtFZ3lwdCBUcnVzdCBDb3Jwb3JhdGUgQ0EgRzIwHhcNMjAwMzMxMDAwMDAwWhcNMjEwMzMwMjM1OTU5WjBgMRUwEwYDVQQKFAxFZ3lwdCBUcnVzdCAxGDAWBgNVBGEUD1ZBVEVHLTExMzMxNzcxMzELMAkGA1UEBhMCRUcxIDAeBgNVBAMMF1Rlc3QgU2VhbGluZyBEZW1vIHVzZXIyMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEApmVGVJtpImeq\u002BtIJiVWSkIEEOTIcnG1XNYQOYtf5\u002BDg9eF5H5x1wkgR2G7dvWVXrTsdNv2Q\u002Bgvml9SdfWxlYxaljg2AuBrsHFjYVEAQFI37EW2K7tbMT7bfxwT1M5tbjxnkTTK12cgwxPr2LBNhHpfXp8SNyWCxpk6eyJb87DveVwCLbAGGXO9mhDj62glVTrCFit7mHC6bZ6MOMAp013B8No9c8xnrKQiOb4Tm2GxBYHFwEcfYUGZNltGZNdVUtu6ty\u002BNTrSRRC/dILeGHgz6/2pgQPk5OFYRTRHRNVNo\u002BjG\u002BnurUYkSWxA4I9CmsVt2FdeBeuvRFs/U1I\u002BieKg1wIDAQABo4HHMIHEMAkGA1UdEwQCMAAwVAYDVR0fBE0wSzBJoEegRYZDaHR0cDovL21wa2ljcmwuZWd5cHR0cnVzdC5jb20vRWd5cHRUcnVzdENvcnBvcmF0ZUNBRzIvTGF0ZXN0Q1JMLmNybDAdBgNVHQ4EFgQUqzFDImtytsUbghbmtnl2/k4d5jEwEQYJYIZIAYb4QgEBBAQDAgeAMB8GA1UdIwQYMBaAFCInP8ziUIPmu86XJUWXspKN3LsFMA4GA1UdDwEB/wQEAwIGwDANBgkqhkiG9w0BAQsFAAOCAQEAxE3KpyYlPy/e3\u002B6jfz5RqlLhRLppWpRlKYUvH1uIhCNRuWaYYRchw1xe3jn7bLKbNrUmey\u002BMRwp1hZptkxFMYKTIEnNjOKCrLmVIuPFcfLXAQFq5vgLDSbnUhG/r5D\u002B50ndPucyUPhX3gw8gFlA1R\u002BtdNEoeKqYSo9v3p5qNANq12OuZbkhPg6sAD4dojWoNdlkc8J2ML0eq4a5AQvb4yZVb\u002BezqJyqKj83RekRZi0kMxoIm8l82CN8I/Bmp6VVNJRhQKhSeb7ShpdkZcMwcfKdDw6LW02/XcmzVl8NBBbLjKSJ/jxdL1RxPPza7RbGqSx9pfyav5\u002BAxO9sXnXXc5jGCApIwggKOAgEBMF0wSTELMAkGA1UEBhMCRUcxFDASBgNVBAoTC0VneXB0IFRydXN0MSQwIgYDVQQDExtFZ3lwdCBUcnVzdCBDb3Jwb3JhdGUgQ0EgRzICEEFkOqRVlVar0F0n3FZOLiIwCwYJYIZIAWUDBAIBoIIBCjAYBgkqhkiG9w0BCQMxCwYJKoZIhvcNAQcFMBwGCSqGSIb3DQEJBTEPFw0yMTAyMDEyMzUwMjFaMC8GCSqGSIb3DQEJBDEiBCD5bGXJu9uJZIPMGXK98UrHzJM/V2U/WAO6BErxpX5wdTCBngYLKoZIhvcNAQkQAi8xgY4wgYswgYgwgYUEIAJA8uO/ek3l9i3ZOgRtPhGWwwFYljbeJ7yAgEnyYNCWMGEwTaBLMEkxCzAJBgNVBAYTAkVHMRQwEgYDVQQKEwtFZ3lwdCBUcnVzdDEkMCIGA1UEAxMbRWd5cHQgVHJ1c3QgQ29ycG9yYXRlIENBIEcyAhBBZDqkVZVWq9BdJ9xWTi4iMAsGCSqGSIb3DQEBAQSCAQB13E1WX\u002BzbWppfJi3DBK9MMSB1TXuxcNkGXQ19OcRUUAaAe2K\u002BisobYrUCZbi3ygc2AWOMyafboxjjomzrnvXKrFgspT4wAFPYaAGFzKWq\u002BW/nqMhIqJVIpS/NM7Al4HvuBA5iGuZEQFusElB0yIxOIiYDI4v8Ilkff4/duj/V2CNaN5cqXLOpL5RP6Y5i\u002BVsPGb89t/L0dSIldGN0JqaqarqYo5/RwsUFJJq01DFpPGNbOIX3gSCDmycfhJPS9csnne9Zt\u002BabNpja5ZR6KA8JMe4DHes7FDZqHBNHdC\u002BRDXT4crqmnyiJjizULu6MqDc0Fv3vrMMWDLRlwDecgq7i";
                        element.signatures.Add(s);
                        //##########################################################################
                        //########################### INVOICE LINES ################################
                        //##########################################################################                
                        InvoiceBL invLineBL = new InvoiceBL(_tenantService);
                        List<InvoiceLineModel> invLine = invLineBL.oracleGetInvoiceLine(salesOfferID);
                        element.invoiceLines = new List<InvoiceLine>();
                        ItemBL itmModel = new ItemBL(_tenantService);
                        int line = 0;
                        int length = invLine.Count;
                        decimal totSalesAmount = 0;
                        decimal netTotal = 0;
                        decimal totalAmount = 0;
                        decimal totalDiscount = 0;
                        List<TaxTotal> t = new List<TaxTotal>();
                        for (line = 0; line < length; line++)
                        {
                            //LINE

                            List<ItemModel> item = itmModel.oracleGetItem((long)invLine[line].ItemID);
                            InvoiceLine l1 = new InvoiceLine();
                            //l1.description = item[0].ItemDescription;
                            l1.description = itmModel.GetItemOfferPricingComment(invLine[line].SalesOfferProductID1);
                            l1.internalCode = item[0].ItemInternalCode;// "1371"; 
                            l1.itemType = item[0].ItemTypeEINVOICE;//"EGS ";//---->
                            l1.itemCode = item[0].ItemCode;// "EG-100710840-1371";
                            l1.unitType = item[0].ItemUnitTypeEINVOICE;// "EA";
                            l1.quantity = (Decimal)invLine[line].Quantity1;// Decimal.Parse(invLine[line].Quantity1.ToString() );//1;
                            UnitValue uv = new UnitValue();
                            uv.amountEGP = (Decimal)invLine[line].ItemPrice1;//2000;
                            uv.currencySold = "EGP";
                            l1.unitValue = uv;
                            l1.salesTotal = (Decimal)invLine[line].SalesTotal;//2000;
                            totSalesAmount += l1.salesTotal;
                            //DISCOUNT
                            Discount dis = new Discount();
                            dis.amount = l1.salesTotal * (Decimal)invLine[line].DiscountPercentage1 / 100;
                            dis.rate = (Decimal)invLine[line].DiscountPercentage1;
                            l1.discount = dis;
                            totalDiscount += dis.amount;
                            //--------------------------
                            l1.netTotal = (Decimal)invLine[line].SalesTotal - (l1.discount.amount);// 2000;                   
                            netTotal += l1.netTotal;
                            l1.valueDifference = 0;
                            l1.itemsDiscount = 0;
                            //----------------------------------------------------------------------------------------------------------------------
                            List<InvoiceLineTaxModel> TaxItem = invHModel.oracleGetInvoiceLineTax(salesOfferID, invLine[line].SalesOfferProductID1);
                            l1.taxableItems = new List<TaxableItem>();
                            //--TAX
                            decimal totalOfTaxableTax = 0;
                            decimal totalTaxPerLine = 0;
                            // T4 is Found
                            decimal T4Amount = 0;
                            foreach (InvoiceLineTaxModel taxItem in TaxItem)
                            {
                                TaxableItem ti = new TaxableItem();
                                if (taxItem.TaxName1.CompareTo("T1") == 0)
                                {
                                    ti.amount = Decimal.Parse(String.Format("{0:0.00000}", (l1.netTotal + (totalOfTaxableTax)) * (Decimal)taxItem.TaxPercentage1 / 100));
                                    ti.subType = taxItem.SubTaxName1;
                                    ti.taxType = taxItem.TaxName1;
                                    ti.rate = Decimal.Parse(String.Format("{0:0.00}", taxItem.TaxPercentage1));
                                    if (line == 0)
                                    {
                                        TaxTotal tt = new TaxTotal();
                                        tt.amount = ti.amount;
                                        tt.taxType = taxItem.TaxName1;
                                        t.Add(tt);
                                    }
                                    else
                                    {
                                        bool found = false;
                                        for (int i = 0; i < t.Count; i++)
                                        {
                                            if (t[i].taxType.CompareTo(taxItem.TaxName1) == 0)
                                            {
                                                t[i].amount += ti.amount;
                                                found = true;
                                                break;
                                            }
                                        }
                                        if (found == false)
                                        {
                                            TaxTotal tt = new TaxTotal();
                                            tt.amount = ti.amount;
                                            tt.taxType = taxItem.TaxName1;
                                            t.Add(tt);
                                        }
                                    }
                                }
                                else if (taxItem.TaxName1.CompareTo("T4") == 0) // Michael Modified
                                {
                                    if (taxItem.IsPercentage1 == true) // Percentage Always
                                        ti.amount = Decimal.Parse(String.Format("{0:0.00000}", l1.netTotal * (Decimal)taxItem.TaxPercentage1 / 100));
                                    else
                                        ti.amount = (Decimal)taxItem.TaxValue1;

                                    ti.subType = taxItem.SubTaxName1;
                                    ti.taxType = taxItem.TaxName1;
                                    if (taxItem.IsPercentage1 == true)
                                        ti.rate = Decimal.Parse(String.Format("{0:0.00}", taxItem.TaxPercentage1));
                                    else
                                        ti.rate = 0;

                                    if (line == 0)
                                    {
                                        TaxTotal tt = new TaxTotal();
                                        tt.amount = ti.amount;
                                        tt.taxType = taxItem.TaxName1;
                                        t.Add(tt);
                                    }
                                    else
                                    {
                                        bool found = false;
                                        for (int i = 0; i < t.Count; i++)
                                        {
                                            if (t[i].taxType.CompareTo(taxItem.TaxName1) == 0)
                                            {
                                                t[i].amount += ti.amount;
                                                found = true;
                                                break;
                                            }
                                        }
                                        if (found == false)
                                        {
                                            TaxTotal tt = new TaxTotal();
                                            tt.amount = ti.amount;
                                            tt.taxType = taxItem.TaxName1;
                                            t.Add(tt);
                                        }
                                    }
                                    T4Amount = ti.amount;
                                    //ti.amount = -1 * ti.amount; // Special case for T4  only subtract from totalTaxPerLine not form totalOfTaxableTax
                                    //T4Amount = ti.amount;
                                    //if (taxItem.TaxType1.CompareTo("Taxable Types") == 0)
                                    //    totalOfTaxableTax -= ti.amount;
                                }
                                else
                                {
                                    if (taxItem.IsPercentage1 == true)
                                        ti.amount = Decimal.Parse(String.Format("{0:0.00000}", l1.netTotal * (Decimal)taxItem.TaxPercentage1 / 100));
                                    else
                                        ti.amount = (Decimal)taxItem.TaxValue1;

                                    ti.subType = taxItem.SubTaxName1;
                                    ti.taxType = taxItem.TaxName1;
                                    if (taxItem.IsPercentage1 == true)
                                        ti.rate = Decimal.Parse(String.Format("{0:0.00}", taxItem.TaxPercentage1));
                                    else
                                        ti.rate = 0;

                                    if (line == 0)
                                    {
                                        TaxTotal tt = new TaxTotal();
                                        tt.amount = ti.amount;
                                        tt.taxType = taxItem.TaxName1;
                                        t.Add(tt);
                                    }
                                    else
                                    {
                                        bool found = false;
                                        for (int i = 0; i < t.Count; i++)
                                        {
                                            if (t[i].taxType.CompareTo(taxItem.TaxName1) == 0)
                                            {
                                                t[i].amount += ti.amount;
                                                found = true;
                                                break;
                                            }
                                        }
                                        if (found == false)
                                        {
                                            TaxTotal tt = new TaxTotal();
                                            tt.amount = ti.amount;
                                            tt.taxType = taxItem.TaxName1;
                                            t.Add(tt);
                                        }
                                    }
                                    if (taxItem.TaxType1.CompareTo("Taxable Types") == 0)
                                        totalOfTaxableTax += ti.amount;
                                }
                                if (taxItem.TaxName1.CompareTo("T4") != 0)
                                {
                                    totalTaxPerLine += ti.amount;
                                }
                                l1.taxableItems.Add(ti);
                            }
                            //-----------------------------------------------------------------------------------------------------------------------
                            l1.totalTaxableFees = totalOfTaxableTax;
                            l1.total = l1.netTotal + totalTaxPerLine - T4Amount;
                            totalAmount += l1.total;
                            //----------------------------------------------------
                            element.invoiceLines.Add(l1);
                        }
                        //##########################################################################
                        element.totalSalesAmount = totSalesAmount;
                        element.totalDiscountAmount = totalDiscount;
                        element.netAmount = netTotal;
                        element.extraDiscountAmount = 0;
                        element.totalItemsDiscountAmount = 0;
                        element.totalAmount = totalAmount;
                        //##########################################################################
                        element.taxTotals = new List<TaxTotal>();
                        for (int i = 0; i < t.Count; i++)
                        {
                            element.taxTotals.Add(t[i]);
                        }
                        //##########################################################################
                        d.documents.Add(element);

                        var submissionText = new StringContent(JsonConvert.SerializeObject(d, Formatting.Indented), Encoding.UTF8, "application/json");
                        var response = client.PostAsync("api/v1.0/documentsubmissions", submissionText).GetAwaiter().GetResult();
                        var response_content = response.Content.ReadAsStringAsync();
                        documentsubmissionsResult SubmissionResult = JsonConvert.DeserializeObject<documentsubmissionsResult>(response_content.Result);
                        popupMsg = response_content.Result;
                        if (SubmissionResult.acceptedDocuments != null && SubmissionResult.acceptedDocuments.Count > 0)
                        {
                            string internalId = SubmissionResult.acceptedDocuments[0].internalId;
                            string longId = SubmissionResult.acceptedDocuments[0].longId;
                            string uuid = SubmissionResult.acceptedDocuments[0].uuid;

                            popupMsg = popupMsg + "acceptedDocuments count : " + SubmissionResult.acceptedDocuments.Count + Environment.NewLine + " internalId : " + internalId + Environment.NewLine +
                                " longId : " + longId + Environment.NewLine + " uuid : " + uuid;
                            invLineBL.saveInvoiceUUID(invoiceID, uuid, "Accepted", DateTime.Now);
                        }
                        if (SubmissionResult.rejectedDocuments != null && SubmissionResult.rejectedDocuments.Count > 0)
                        {
                            string code = SubmissionResult.rejectedDocuments[0].error.code;
                            string target = SubmissionResult.rejectedDocuments[0].error.target;
                            string message = SubmissionResult.rejectedDocuments[0].error.message;
                            string details = "";
                            for (int e = 0; e < SubmissionResult.rejectedDocuments[0].error.details.Length; e++)
                            {
                                details = details + "error " + (e + 1) + Environment.NewLine + " Code:" + SubmissionResult.rejectedDocuments[0].error.details[e].code + Environment.NewLine + " Target:"
                                    + SubmissionResult.rejectedDocuments[0].error.details[e].target + Environment.NewLine
                                    + "Message" + SubmissionResult.rejectedDocuments[0].error.details[e].message + Environment.NewLine;
                            }
                            popupMsg = popupMsg + "rejectedDocuments count : " + SubmissionResult.rejectedDocuments.Count + Environment.NewLine + " code : " + code + Environment.NewLine
                                + "  target : " + target + Environment.NewLine + "  message : "
                                + message
                             + Environment.NewLine + "  details : " + details + Environment.NewLine;


                            invLineBL.saveInvoiceUUID(invoiceID, "", "Rejected " + details, DateTime.Now);
                        }
                    }
                    return popupMsg;
                }

            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        public string SignedDocumentsubmissions(long invoiceID, long clientAddressID, long cusID, long salesOfferID)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            String popupMsg = "";
            try
            {
                LoginAsTaxpayerSystem();
                using (HttpClient client = new HttpClient())
                {

                    client.BaseAddress = new Uri(@"" + env.ApiBaseUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Accesstoken);

                    InvoiceBL invHModel = new InvoiceBL(_tenantService);
                    List<InvoiceHeaderModel> invoiceHeaderList = invHModel.oracleGetInvoiceHeader(invoiceID);
                    if (invoiceHeaderList[0].TrxType.CompareTo("Invoice") != 0)
                    {
                        SignedCreaditOrDebitDocument d = new SignedCreaditOrDebitDocument();
                        d.documents = new List<SignedCreditOrDebitDocumentElement>();
                        SignedCreditOrDebitDocumentElement element = new SignedCreditOrDebitDocumentElement();
                        if (invoiceHeaderList[0].TrxType.CompareTo("Credit Note") == 0)
                            element.documentType = "c";
                        else
                            element.documentType = "d";
                        //#########################################################################
                        //############################ ISSUER #####################################
                        //#########################################################################
                        IssuerBL issuerBL = new IssuerBL(_tenantService);
                        IssuerModel issuerModel = issuerBL.GetProfileInfo(clientAddressID);
                        element.issuer = new Issuer();
                        element.issuer.id = issuerModel.RegNum;// "100710840";
                        element.issuer.name = issuerModel.CompanyName;//"مكتبة الاسكندرية";
                        element.issuer.type = issuerModel.Type;//"B";
                        element.issuer.address = new IssuerAddress();
                        element.issuer.address.country = issuerModel.Country;// "EG";
                        element.issuer.address.governate = issuerModel.Governate;// "Alexandria";
                        element.issuer.address.street = issuerModel.Street;// "0 / 1115   شارع بورسعيد امام المجمع النظرى الشاطبى الاسكندرية";
                        element.issuer.address.regionCity = issuerModel.RegionCity;//"XX";
                        element.issuer.address.branchID = issuerModel.BranchID;// "0";
                        element.issuer.address.buildingNumber = issuerModel.BuildNum;// "1";             
                                                                                     //##OPtinal
                        element.issuer.address.floor = "0";
                        element.issuer.address.landmark = "";
                        element.issuer.address.postalCode = "0";
                        element.issuer.address.room = "0";
                        element.issuer.address.additionalInformation = "";
                        //#########################################################################
                        //########################### RECEIVER ####################################
                        //#########################################################################
                        CustomerBL cusModel = new CustomerBL(_tenantService);
                        List<CustomerModel> customerList = cusModel.oracleGetCustomer(cusID);
                        element.receiver = new Receiver();
                        element.receiver.id = customerList[0].TaxpayerCode;//"100295584";
                        element.receiver.name = customerList[0].PartyName;//"شركة لابوار للمحلات السياحية";
                        element.receiver.type = customerList[0].CusType;// "B";
                        element.receiver.address = new ReceiverAddress();
                        element.receiver.address.country = customerList[0].Country;// "EG";
                        element.receiver.address.street = customerList[0].Street; //"3شارع شجرة الدر القاهرة";
                        element.receiver.address.regionCity = customerList[0].RegionCity;// "الزمالك";
                        element.receiver.address.governate = customerList[0].Governate;// "Alexandria";               
                        element.receiver.address.buildingNumber = customerList[0].BuildingNumber;// "1";
                                                                                                 //##OPtinal
                        element.receiver.address.floor = "0";
                        element.receiver.address.landmark = "";
                        element.receiver.address.postalCode = "0";
                        element.receiver.address.room = "0";
                        element.receiver.address.additionalInformation = "";
                        //##########################################################################
                        //########################### INVOICE DATA #################################
                        //##########################################################################  
                        element.documentTypeVersion = "1.0";
                        element.references = new List<string>();

                        String invoiceUUID = invHModel.getUUID(invoiceID);
                        if (invoiceUUID.CompareTo("") == 0)
                        {
                            popupMsg = "THIS CREDIT NOTE IS NOT FOR A SUBMITTED INVOICE";
                            return popupMsg;
                        }
                        element.references.Add(invoiceUUID);
                        if (invoiceHeaderList[0].Trx_date != null)
                        {
                            DateTime date = DateTime.Now;
                            TimeSpan time = new TimeSpan(0, DateTime.Now.Hour, 0, 0);
                            element.dateTimeIssued = ((DateTime)invoiceHeaderList[0].Trx_date).Add(time).AddHours(-4).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
                        }
                        else
                        {
                            element.dateTimeIssued = DateTime.Now.AddHours(-4).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
                        }
                        //element.dateTimeIssued = // invoiceHeaderList[0].Trx_date.AddHours(-2).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
                        //    DateTime.Now.AddHours(-2).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
                        //element.invoiceID = invoiceID;
                        element.taxpayerActivityCode = issuerModel.ActivityCode1;// "9101";//--->انشطة المكتبات والارشيف
                        element.internalID = invoiceHeaderList[0].Trx_number;// "3220002";//----->رقم داخلي للفاتورة                   
                                                                             //##OPtinal
                        element.purchaseOrderReference = "";
                        element.purchaseOrderDescription = "";
                        element.salesOrderDescription = "";
                        element.salesOrderReference = "";
                        element.proformaInvoiceNumber = "";
                        //payment
                        element.payment = new Payment();
                        element.payment.bankAccountIBAN = "";
                        element.payment.bankAccountNo = "";
                        element.payment.bankAddress = "";
                        element.payment.bankName = "";
                        element.payment.swiftCode = "";
                        element.payment.terms = "";
                        //delivery
                        element.delivery = new Delivery();
                        element.delivery.grossWeight = 0;
                        element.delivery.approach = "";
                        element.delivery.exportPort = "";
                        element.delivery.terms = "";
                        element.delivery.packaging = "";
                        element.delivery.netWeight = 0;
                        element.delivery.dateValidity = "";

                        //##########################################################################
                        //########################### INVOICE LINES ################################
                        //##########################################################################                
                        InvoiceBL invLineBL = new InvoiceBL(_tenantService);
                        List<InvoiceLineModel> invLine = invLineBL.oracleGetInvoiceLine(salesOfferID);
                        element.invoiceLines = new List<InvoiceLine>();
                        ItemBL itmModel = new ItemBL(_tenantService);
                        int line = 0;
                        int length = invLine.Count;
                        decimal totSalesAmount = 0;
                        decimal netTotal = 0;
                        decimal totalAmount = 0;
                        decimal totalDiscount = 0;
                        List<TaxTotal> t = new List<TaxTotal>();
                        for (line = 0; line < length; line++)
                        {
                            //LINE

                            List<ItemModel> item = itmModel.oracleGetItem((long)invLine[line].ItemID);
                            InvoiceLine l1 = new InvoiceLine();
                            //l1.description = item[0].ItemDescription;
                            l1.description = itmModel.GetItemOfferPricingComment(invLine[line].SalesOfferProductID1);
                            l1.internalCode = item[0].ItemInternalCode;// "1371"; 
                            l1.itemType = item[0].ItemTypeEINVOICE;//"EGS ";//---->
                            l1.itemCode = item[0].ItemCode;// "EG-100710840-1371";
                            l1.unitType = item[0].ItemUnitTypeEINVOICE;// "EA";
                            l1.quantity = (Decimal)invLine[line].Quantity1;// Decimal.Parse(invLine[line].Quantity1.ToString() );//1;
                            UnitValue uv = new UnitValue();
                            uv.amountEGP = (Decimal)invLine[line].ItemPrice1;//2000;
                            uv.currencySold = "EGP";
                            l1.unitValue = uv;
                            l1.salesTotal = (Decimal)invLine[line].SalesTotal;//2000;
                            totSalesAmount += l1.salesTotal;
                            //DISCOUNT
                            Discount dis = new Discount();
                            dis.amount = l1.salesTotal * (Decimal)invLine[line].DiscountPercentage1 / 100;
                            dis.rate = (Decimal)invLine[line].DiscountPercentage1;
                            l1.discount = dis;
                            totalDiscount += dis.amount;
                            //--------------------------
                            l1.netTotal = (Decimal)invLine[line].SalesTotal - (l1.discount.amount);// 2000;                   
                            netTotal += l1.netTotal;
                            l1.valueDifference = 0;
                            l1.itemsDiscount = 0;
                            //----------------------------------------------------------------------------------------------------------------------
                            List<InvoiceLineTaxModel> TaxItem = invHModel.oracleGetInvoiceLineTax(salesOfferID, invLine[line].SalesOfferProductID1);
                            l1.taxableItems = new List<TaxableItem>();
                            //--TAX
                            decimal totalOfTaxableTax = 0;
                            decimal totalTaxPerLine = 0;
                            decimal T4Amount = 0;
                            foreach (InvoiceLineTaxModel taxItem in TaxItem)
                            {
                                TaxableItem ti = new TaxableItem();
                                if (taxItem.TaxName1.CompareTo("T1") == 0)
                                {
                                    ti.amount = Decimal.Parse(String.Format("{0:0.00000}", (l1.netTotal + totalOfTaxableTax) * (Decimal)taxItem.TaxPercentage1 / 100));
                                    ti.subType = taxItem.SubTaxName1;
                                    ti.taxType = taxItem.TaxName1;
                                    ti.rate = Decimal.Parse(String.Format("{0:0.00}", taxItem.TaxPercentage1));
                                    if (line == 0)
                                    {
                                        TaxTotal tt = new TaxTotal();
                                        tt.amount = ti.amount;
                                        tt.taxType = taxItem.TaxName1;
                                        t.Add(tt);
                                    }
                                    else
                                    {
                                        bool found = false;
                                        for (int i = 0; i < t.Count; i++)
                                        {
                                            if (t[i].taxType.CompareTo(taxItem.TaxName1) == 0)
                                            {
                                                t[i].amount += ti.amount;
                                                found = true;
                                                break;
                                            }
                                        }
                                        if (found == false)
                                        {
                                            TaxTotal tt = new TaxTotal();
                                            tt.amount = ti.amount;
                                            tt.taxType = taxItem.TaxName1;
                                            t.Add(tt);
                                        }
                                    }
                                }
                                else if (taxItem.TaxName1.CompareTo("T4") == 0) // Michael Modified
                                {
                                    if (taxItem.IsPercentage1 == true) // Percentage Always
                                        ti.amount = Decimal.Parse(String.Format("{0:0.00000}", l1.netTotal * (Decimal)taxItem.TaxPercentage1 / 100));
                                    else
                                        ti.amount = (Decimal)taxItem.TaxValue1;

                                    ti.subType = taxItem.SubTaxName1;
                                    ti.taxType = taxItem.TaxName1;
                                    if (taxItem.IsPercentage1 == true)
                                        ti.rate = Decimal.Parse(String.Format("{0:0.00}", taxItem.TaxPercentage1));
                                    else
                                        ti.rate = 0;

                                    if (line == 0)
                                    {
                                        TaxTotal tt = new TaxTotal();
                                        tt.amount = ti.amount;
                                        tt.taxType = taxItem.TaxName1;
                                        t.Add(tt);
                                    }
                                    else
                                    {
                                        bool found = false;
                                        for (int i = 0; i < t.Count; i++)
                                        {
                                            if (t[i].taxType.CompareTo(taxItem.TaxName1) == 0)
                                            {
                                                t[i].amount += ti.amount;
                                                found = true;
                                                break;
                                            }
                                        }
                                        if (found == false)
                                        {
                                            TaxTotal tt = new TaxTotal();
                                            tt.amount = ti.amount;
                                            tt.taxType = taxItem.TaxName1;
                                            t.Add(tt);
                                        }
                                    }
                                    T4Amount = ti.amount;
                                    //ti.amount = -1 * ti.amount; // Special case for T4  only subtract from totalTaxPerLine not form totalOfTaxableTax
                                    //T4Amount = ti.amount;
                                    //if (taxItem.TaxType1.CompareTo("Taxable Types") == 0)
                                    //    totalOfTaxableTax -= ti.amount;
                                }
                                else
                                {
                                    if (taxItem.IsPercentage1 == true)
                                        ti.amount = Decimal.Parse(String.Format("{0:0.00000}", l1.netTotal * (Decimal)taxItem.TaxPercentage1 / 100));
                                    else
                                        ti.amount = (Decimal)taxItem.TaxValue1;

                                    ti.subType = taxItem.SubTaxName1;
                                    ti.taxType = taxItem.TaxName1;
                                    if (taxItem.IsPercentage1 == true)
                                        ti.rate = Decimal.Parse(String.Format("{0:0.00}", taxItem.TaxPercentage1));
                                    else
                                        ti.rate = 0;

                                    if (line == 0)
                                    {
                                        TaxTotal tt = new TaxTotal();
                                        tt.amount = ti.amount;
                                        tt.taxType = taxItem.TaxName1;
                                        t.Add(tt);
                                    }
                                    else
                                    {
                                        bool found = false;
                                        for (int i = 0; i < t.Count; i++)
                                        {
                                            if (t[i].taxType.CompareTo(taxItem.TaxName1) == 0)
                                            {
                                                t[i].amount += ti.amount;
                                                found = true;
                                                break;
                                            }
                                        }
                                        if (found == false)
                                        {
                                            TaxTotal tt = new TaxTotal();
                                            tt.amount = ti.amount;
                                            tt.taxType = taxItem.TaxName1;
                                            t.Add(tt);
                                        }
                                    }
                                    if (taxItem.TaxType1.CompareTo("Taxable Types") == 0)
                                        totalOfTaxableTax += ti.amount;
                                }
                                if (taxItem.TaxName1.CompareTo("T4") != 0)
                                {
                                    totalTaxPerLine += ti.amount;
                                }
                                l1.taxableItems.Add(ti);
                            }
                            //-----------------------------------------------------------------------------------------------------------------------
                            l1.totalTaxableFees = totalOfTaxableTax;
                            l1.total = l1.netTotal + totalTaxPerLine - T4Amount;
                            totalAmount += l1.total;
                            //----------------------------------------------------
                            element.invoiceLines.Add(l1);
                        }
                        //##########################################################################
                        element.totalSalesAmount = totSalesAmount;
                        element.totalDiscountAmount = totalDiscount;
                        element.netAmount = netTotal;
                        element.extraDiscountAmount = 0;
                        element.totalItemsDiscountAmount = 0;
                        element.totalAmount = totalAmount;
                        //##########################################################################
                        element.taxTotals = new List<TaxTotal>();
                        for (int i = 0; i < t.Count; i++)
                        {
                            element.taxTotals.Add(t[i]);
                        }
                        //##########################################################################
                        d.documents.Add(element);

                        String uridirectory = Path.GetPathRoot(Environment.SystemDirectory) + "\\Signature";
                        string directory = new Uri(uridirectory).LocalPath;
                        File.WriteAllText(@"" + directory + "\\SourceDocumentJson.json", JsonConvert.SerializeObject(d.documents[0], Formatting.Indented));
                        //RUN BATCH
                        runBatchFile();
                        //  READ OUTPUT FILE
                        string json = "";
                        using (StreamReader r = new StreamReader(@"" + directory + "\\FullSignedDocument.json"))
                        {
                            json = r.ReadToEnd();
                        }
                        var submissionText = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = client.PostAsync("api/v1.0/documentsubmissions", submissionText).GetAwaiter().GetResult();
                        var response_content = response.Content.ReadAsStringAsync();
                        documentsubmissionsResult SubmissionResult = JsonConvert.DeserializeObject<documentsubmissionsResult>(response_content.Result);
                        popupMsg = response_content.Result;
                        if (SubmissionResult.acceptedDocuments != null && SubmissionResult.acceptedDocuments.Count > 0)
                        {
                            string internalId = SubmissionResult.acceptedDocuments[0].internalId;
                            string longId = SubmissionResult.acceptedDocuments[0].longId;
                            string uuid = SubmissionResult.acceptedDocuments[0].uuid;

                            popupMsg = popupMsg + "acceptedDocuments count : " + SubmissionResult.acceptedDocuments.Count + Environment.NewLine + " internalId : " + internalId + Environment.NewLine +
                                " longId : " + longId + Environment.NewLine + " uuid : " + uuid;

                            invLineBL.saveInvoiceUUID(invoiceID, uuid, "Accepted", DateTime.Now);

                        }
                        if (SubmissionResult.rejectedDocuments != null && SubmissionResult.rejectedDocuments.Count > 0)
                        {
                            string code = SubmissionResult.rejectedDocuments[0].error.code;
                            string target = SubmissionResult.rejectedDocuments[0].error.target;
                            string message = SubmissionResult.rejectedDocuments[0].error.message;
                            string details = "";
                            for (int e = 0; e < SubmissionResult.rejectedDocuments[0].error.details.Length; e++)
                            {
                                details = details + "error " + (e + 1) + Environment.NewLine + " Code:" + SubmissionResult.rejectedDocuments[0].error.details[e].code + Environment.NewLine + " Target:"
                                    + SubmissionResult.rejectedDocuments[0].error.details[e].target + Environment.NewLine
                                    + "Message" + SubmissionResult.rejectedDocuments[0].error.details[e].message + Environment.NewLine;
                            }
                            popupMsg = popupMsg + "rejectedDocuments count : " + SubmissionResult.rejectedDocuments.Count + Environment.NewLine + " code : " + code + Environment.NewLine
                                + "  target : " + target + Environment.NewLine + "  message : "
                                + message
                             + Environment.NewLine + "  details : " + details + Environment.NewLine;

                            invLineBL.saveInvoiceUUID(invoiceID, "", "Rejected " + details, DateTime.Now);
                        }

                    }
                    else
                    {

                        SignedDocument d = new SignedDocument();
                        d.documents = new List<SignedDocumentElement>();
                        SignedDocumentElement element = new SignedDocumentElement();
                        element.documentType = "I";
                        //#########################################################################
                        //############################ ISSUER #####################################
                        //#########################################################################
                        IssuerBL issuerBL = new IssuerBL(_tenantService);
                        IssuerModel issuerModel = issuerBL.GetProfileInfo(clientAddressID);
                        //if (issuerModel == null)
                        //{
                        //    return "Invalid Issur Data , yo are missing some data";
                        //}
                        element.issuer = new Issuer();
                        element.issuer.id = issuerModel.RegNum;// "100710840";
                        element.issuer.name = issuerModel.CompanyName;//"مكتبة الاسكندرية";
                        element.issuer.type = issuerModel.Type;//"B";
                        element.issuer.address = new IssuerAddress();
                        element.issuer.address.country = issuerModel.Country;// "EG";
                        element.issuer.address.governate = issuerModel.Governate;// "Alexandria";
                        element.issuer.address.street = issuerModel.Street;// "0 / 1115   شارع بورسعيد امام المجمع النظرى الشاطبى الاسكندرية";
                        element.issuer.address.regionCity = issuerModel.RegionCity;//"XX";
                        element.issuer.address.branchID = issuerModel.BranchID;// "0";
                        element.issuer.address.buildingNumber = issuerModel.BuildNum;// "1";             
                                                                                     //##OPtinal
                        element.issuer.address.floor = "0";
                        element.issuer.address.landmark = "";
                        element.issuer.address.postalCode = "0";
                        element.issuer.address.room = "0";
                        element.issuer.address.additionalInformation = "";
                        //#########################################################################
                        //########################### RECEIVER ####################################
                        //#########################################################################
                        CustomerBL cusModel = new CustomerBL(_tenantService);
                        List<CustomerModel> customerList = cusModel.oracleGetCustomer(cusID);
                        element.receiver = new Receiver();
                        element.receiver.id = customerList[0].TaxpayerCode;//"100295584";
                        element.receiver.name = customerList[0].PartyName;//"شركة لابوار للمحلات السياحية";
                        element.receiver.type = customerList[0].CusType;// "B";
                        element.receiver.address = new ReceiverAddress();
                        element.receiver.address.country = customerList[0].Country;// "EG";
                        element.receiver.address.street = customerList[0].Street; //"3شارع شجرة الدر القاهرة";
                        element.receiver.address.regionCity = customerList[0].RegionCity;// "الزمالك";
                        element.receiver.address.governate = customerList[0].Governate;// "Alexandria";               
                        element.receiver.address.buildingNumber = customerList[0].BuildingNumber;// "1";
                                                                                                 //##OPtinal
                        element.receiver.address.floor = "0";
                        element.receiver.address.landmark = "";
                        element.receiver.address.postalCode = "0";
                        element.receiver.address.room = "0";
                        element.receiver.address.additionalInformation = "";
                        //##########################################################################
                        //########################### INVOICE DATA #################################
                        //##########################################################################  
                        //element.dateTimeIssued =// invoiceHeaderList[0].Trx_date.AddHours(-2).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
                        //    DateTime.Now.AddHours(-2).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
                        if (invoiceHeaderList[0].Trx_date != null)
                        {
                            DateTime date = DateTime.Now;
                            TimeSpan time = new TimeSpan(0, DateTime.Now.Hour, 0, 0);
                            element.dateTimeIssued = ((DateTime)invoiceHeaderList[0].Trx_date).Add(time).AddHours(-4).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
                        }
                        else
                        {
                            element.dateTimeIssued = DateTime.Now.AddHours(-4).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
                        }


                        element.documentTypeVersion = "1.0";
                        //element.invoiceID = invoiceID;
                        element.taxpayerActivityCode = issuerModel.ActivityCode1;// "9101";//--->انشطة المكتبات والارشيف
                        element.internalID = invoiceHeaderList[0].Trx_number;// "3220002";//----->رقم داخلي للفاتورة 
                                                                             //##OPtinal
                        element.purchaseOrderReference = "";
                        element.purchaseOrderDescription = "";
                        element.salesOrderDescription = "";
                        element.salesOrderReference = "";
                        element.proformaInvoiceNumber = "";
                        //payment
                        element.payment = new Payment();
                        element.payment.bankAccountIBAN = "";
                        element.payment.bankAccountNo = "";
                        element.payment.bankAddress = "";
                        element.payment.bankName = "";
                        element.payment.swiftCode = "";
                        element.payment.terms = "";
                        //delivery
                        element.delivery = new Delivery();
                        element.delivery.grossWeight = 0;
                        element.delivery.approach = "";
                        element.delivery.exportPort = "";
                        element.delivery.terms = "";
                        element.delivery.packaging = "";
                        element.delivery.netWeight = 0;
                        element.delivery.dateValidity = "";



                        //##########################################################################
                        //########################### INVOICE LINES ################################
                        //##########################################################################                
                        InvoiceBL invLineBL = new InvoiceBL(_tenantService);
                        List<InvoiceLineModel> invLine = invLineBL.oracleGetInvoiceLine(salesOfferID);
                        element.invoiceLines = new List<InvoiceLine>();
                        ItemBL itmModel = new ItemBL(_tenantService);
                        int line = 0;
                        int length = invLine.Count;
                        decimal totSalesAmount = 0;
                        decimal netTotal = 0;
                        decimal totalAmount = 0;
                        decimal totalDiscount = 0;
                        List<TaxTotal> t = new List<TaxTotal>();
                        for (line = 0; line < length; line++)
                        {
                            //LINE

                            List<ItemModel> item = itmModel.oracleGetItem((long)invLine[line].ItemID);
                            InvoiceLine l1 = new InvoiceLine();
                            //l1.description = item[0].ItemDescription;
                            l1.description = itmModel.GetItemOfferPricingComment(invLine[line].SalesOfferProductID1);
                            l1.internalCode = item[0].ItemInternalCode;// "1371"; 
                            l1.itemType = item[0].ItemTypeEINVOICE;//"EGS ";//---->
                            l1.itemCode = item[0].ItemCode;// "EG-100710840-1371";
                            l1.unitType = item[0].ItemUnitTypeEINVOICE;// "EA";
                            l1.quantity = (Decimal)invLine[line].Quantity1;// Decimal.Parse(invLine[line].Quantity1.ToString() );//1;
                            UnitValue uv = new UnitValue();
                            uv.amountEGP = (Decimal)invLine[line].ItemPrice1;//2000;
                            uv.currencySold = "EGP";
                            l1.unitValue = uv;
                            l1.salesTotal = (Decimal)invLine[line].SalesTotal;//2000;
                            totSalesAmount += l1.salesTotal;
                            //DISCOUNT
                            Discount dis = new Discount();
                            dis.amount = l1.salesTotal * (Decimal)invLine[line].DiscountPercentage1 / 100;
                            dis.rate = (Decimal)invLine[line].DiscountPercentage1;
                            l1.discount = dis;
                            totalDiscount += dis.amount;
                            //--------------------------
                            l1.netTotal = (Decimal)invLine[line].SalesTotal - (l1.discount.amount);// 2000;                   
                            netTotal += l1.netTotal;
                            l1.valueDifference = 0;
                            l1.itemsDiscount = 0;
                            //----------------------------------------------------------------------------------------------------------------------
                            List<InvoiceLineTaxModel> TaxItem = invHModel.oracleGetInvoiceLineTax(salesOfferID, invLine[line].SalesOfferProductID1);
                            l1.taxableItems = new List<TaxableItem>();
                            //--TAX
                            decimal totalOfTaxableTax = 0;
                            decimal totalTaxPerLine = 0;
                            decimal T4Amount = 0;
                            foreach (InvoiceLineTaxModel taxItem in TaxItem)
                            {
                                TaxableItem ti = new TaxableItem();
                                if (taxItem.TaxName1.CompareTo("T1") == 0)
                                {
                                    ti.amount = Decimal.Parse(String.Format("{0:0.00000}", (l1.netTotal + totalOfTaxableTax) * (Decimal)taxItem.TaxPercentage1 / 100));
                                    ti.subType = taxItem.SubTaxName1;
                                    ti.taxType = taxItem.TaxName1;
                                    ti.rate = Decimal.Parse(String.Format("{0:0.00}", taxItem.TaxPercentage1));
                                    if (line == 0)
                                    {
                                        TaxTotal tt = new TaxTotal();
                                        tt.amount = ti.amount;
                                        tt.taxType = taxItem.TaxName1;
                                        t.Add(tt);
                                    }
                                    else
                                    {
                                        bool found = false;
                                        for (int i = 0; i < t.Count; i++)
                                        {
                                            if (t[i].taxType.CompareTo(taxItem.TaxName1) == 0)
                                            {
                                                t[i].amount += ti.amount;
                                                found = true;
                                                break;
                                            }
                                        }
                                        if (found == false)
                                        {
                                            TaxTotal tt = new TaxTotal();
                                            tt.amount = ti.amount;
                                            tt.taxType = taxItem.TaxName1;
                                            t.Add(tt);
                                        }
                                    }
                                }
                                else if (taxItem.TaxName1.CompareTo("T4") == 0) // Michael Modified
                                {
                                    if (taxItem.IsPercentage1 == true) // Percentage Always
                                        ti.amount = Decimal.Parse(String.Format("{0:0.00000}", l1.netTotal * (Decimal)taxItem.TaxPercentage1 / 100));
                                    else
                                        ti.amount = (Decimal)taxItem.TaxValue1;

                                    ti.subType = taxItem.SubTaxName1;
                                    ti.taxType = taxItem.TaxName1;
                                    if (taxItem.IsPercentage1 == true)
                                        ti.rate = Decimal.Parse(String.Format("{0:0.00}", taxItem.TaxPercentage1));
                                    else
                                        ti.rate = 0;

                                    if (line == 0)
                                    {
                                        TaxTotal tt = new TaxTotal();
                                        tt.amount = ti.amount;
                                        tt.taxType = taxItem.TaxName1;
                                        t.Add(tt);
                                    }
                                    else
                                    {
                                        bool found = false;
                                        for (int i = 0; i < t.Count; i++)
                                        {
                                            if (t[i].taxType.CompareTo(taxItem.TaxName1) == 0)
                                            {
                                                t[i].amount += ti.amount;
                                                found = true;
                                                break;
                                            }
                                        }
                                        if (found == false)
                                        {
                                            TaxTotal tt = new TaxTotal();
                                            tt.amount = ti.amount;
                                            tt.taxType = taxItem.TaxName1;
                                            t.Add(tt);
                                        }
                                    }
                                    T4Amount = ti.amount;
                                    //ti.amount = -1 * ti.amount; // Special case for T4  only subtract from totalTaxPerLine not form totalOfTaxableTax
                                    //T4Amount = ti.amount;
                                    //if (taxItem.TaxType1.CompareTo("Taxable Types") == 0)
                                    //    totalOfTaxableTax -= ti.amount;
                                }
                                else
                                {
                                    if (taxItem.IsPercentage1 == true)
                                        ti.amount = Decimal.Parse(String.Format("{0:0.00000}", l1.netTotal * (Decimal)taxItem.TaxPercentage1 / 100));
                                    else
                                        ti.amount = (Decimal)taxItem.TaxValue1;

                                    ti.subType = taxItem.SubTaxName1;
                                    ti.taxType = taxItem.TaxName1;
                                    if (taxItem.IsPercentage1 == true)
                                        ti.rate = Decimal.Parse(String.Format("{0:0.00}", taxItem.TaxPercentage1));
                                    else
                                        ti.rate = 0;

                                    if (line == 0)
                                    {
                                        TaxTotal tt = new TaxTotal();
                                        tt.amount = ti.amount;
                                        tt.taxType = taxItem.TaxName1;
                                        t.Add(tt);
                                    }
                                    else
                                    {
                                        bool found = false;
                                        for (int i = 0; i < t.Count; i++)
                                        {
                                            if (t[i].taxType.CompareTo(taxItem.TaxName1) == 0)
                                            {
                                                t[i].amount += ti.amount;
                                                found = true;
                                                break;
                                            }
                                        }
                                        if (found == false)
                                        {
                                            TaxTotal tt = new TaxTotal();
                                            tt.amount = ti.amount;
                                            tt.taxType = taxItem.TaxName1;
                                            t.Add(tt);
                                        }
                                    }
                                    if (taxItem.TaxType1.CompareTo("Taxable Types") == 0)
                                        totalOfTaxableTax += ti.amount;
                                }

                                if (taxItem.TaxName1.CompareTo("T4") != 0)
                                {
                                    totalTaxPerLine += ti.amount;
                                }
                                l1.taxableItems.Add(ti);
                            }
                            //-----------------------------------------------------------------------------------------------------------------------
                            l1.totalTaxableFees = totalOfTaxableTax;
                            l1.total = l1.netTotal + totalTaxPerLine - T4Amount; ;
                            totalAmount += l1.total;
                            //----------------------------------------------------
                            element.invoiceLines.Add(l1);
                        }
                        //##########################################################################
                        element.totalSalesAmount = totSalesAmount;
                        element.totalDiscountAmount = totalDiscount;
                        element.netAmount = netTotal;
                        element.extraDiscountAmount = 0;
                        element.totalItemsDiscountAmount = 0;
                        element.totalAmount = totalAmount;
                        //##########################################################################
                        element.taxTotals = new List<TaxTotal>();
                        for (int i = 0; i < t.Count; i++)
                        {
                            element.taxTotals.Add(t[i]);
                        }
                        //##########################################################################
                        d.documents.Add(element);
                        //CREATE FILE   

                        String uridirectory = Path.GetPathRoot(Environment.SystemDirectory) + "\\Signature";
                        string directory = new Uri(uridirectory).LocalPath;
                        File.WriteAllText(@"" + directory + "\\SourceDocumentJson.json", JsonConvert.SerializeObject(d.documents[0], Formatting.Indented));
                        //RUN BATCH
                        runBatchFile();
                        //  READ OUTPUT FILE
                        string json = "";
                        using (StreamReader r = new StreamReader(@"" + directory + "\\FullSignedDocument.json"))
                        {
                            json = r.ReadToEnd();
                        }
                        var submissionText = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = client.PostAsync("api/v1.0/documentsubmissions", submissionText).GetAwaiter().GetResult();
                        var response_content = response.Content.ReadAsStringAsync();
                        documentsubmissionsResult SubmissionResult = JsonConvert.DeserializeObject<documentsubmissionsResult>(response_content.Result);
                        popupMsg = response_content.Result;
                        if (SubmissionResult.acceptedDocuments != null && SubmissionResult.acceptedDocuments.Count > 0)
                        {
                            string internalId = SubmissionResult.acceptedDocuments[0].internalId;
                            string longId = SubmissionResult.acceptedDocuments[0].longId;
                            string uuid = SubmissionResult.acceptedDocuments[0].uuid;

                            popupMsg = popupMsg + "acceptedDocuments count : " + SubmissionResult.acceptedDocuments.Count + Environment.NewLine + " internalId : " + internalId + Environment.NewLine +
                                " longId : " + longId + Environment.NewLine + " uuid : " + uuid;
                            invLineBL.saveInvoiceUUID(invoiceID, uuid, "Accepted", DateTime.Now);
                        }
                        if (SubmissionResult.rejectedDocuments != null && SubmissionResult.rejectedDocuments.Count > 0)
                        {
                            string code = SubmissionResult.rejectedDocuments[0].error.code;
                            string target = SubmissionResult.rejectedDocuments[0].error.target;
                            string message = SubmissionResult.rejectedDocuments[0].error.message;
                            string details = "";
                            for (int e = 0; e < SubmissionResult.rejectedDocuments[0].error.details.Length; e++)
                            {
                                details = details + "error " + (e + 1) + Environment.NewLine + " Code:" + SubmissionResult.rejectedDocuments[0].error.details[e].code + Environment.NewLine + " Target:"
                                    + SubmissionResult.rejectedDocuments[0].error.details[e].target + Environment.NewLine
                                    + "Message" + SubmissionResult.rejectedDocuments[0].error.details[e].message + Environment.NewLine;
                            }
                            popupMsg = popupMsg + "rejectedDocuments count : " + SubmissionResult.rejectedDocuments.Count + Environment.NewLine + " code : " + code + Environment.NewLine
                                + "  target : " + target + Environment.NewLine + "  message : "
                                + message
                             + Environment.NewLine + "  details : " + details + Environment.NewLine;


                            invLineBL.saveInvoiceUUID(invoiceID, "", "Rejected " + details, DateTime.Now);
                        }
                    }
                    return popupMsg;
                }
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
        }
        private void runBatchFile()
        {
            String uridirectory = Path.GetPathRoot(Environment.SystemDirectory) + "\\Signature";
            string directory = new Uri(uridirectory).LocalPath;
            if (File.Exists(@"" + directory + "\\SourceDocumentJson.json"))
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.EnableRaisingEvents = false;
                proc.StartInfo.FileName = @"" + directory + "\\SubmitInvoices.bat";
                proc.Start();
                //  MessageBox.Show("Cleaned up files, your welcome.");

            }
            else
            {
                // label4.Text = "Error: No file found";
            }
        }




        public BaseMessageResponse EInvoiceBody(long invoiceID, long clientAddressID, long cusID, long salesOfferID
            ,long? invoiceIDReturned) // modified by michael in sep 2023 for Return invoice
        {
            BaseMessageResponse Response = new BaseMessageResponse();
            Response.Result = true;
            Response.Errors = new List<BaseError>();
            try
            {
                // Som Validation Before Return
                if (invoiceID == 0)
                {
                    Response.Result = false;
                    var error = new BaseError();
                    error.ErrorCode = "Err-eI1";
                    error.ErrorMSG = "invoiceID is required";
                    Response.Errors.Add(error);
                }
                if (clientAddressID == 0)
                {
                    Response.Result = false;
                    var error = new BaseError();
                    error.ErrorCode = "Err-eI2";
                    error.ErrorMSG = "clientAddressID is required";
                    Response.Errors.Add(error);
                }
                if (cusID == 0)
                {
                    Response.Result = false;
                    var error = new BaseError();
                    error.ErrorCode = "Err-eI3";
                    error.ErrorMSG = "ClientID is required";
                    Response.Errors.Add(error);
                }

                if (salesOfferID == 0)
                {
                    Response.Result = false;
                    var error = new BaseError();
                    error.ErrorCode = "Err-eI4";
                    error.ErrorMSG = "salesOfferID is required";
                    Response.Errors.Add(error);
                }

                if (Response.Result)
                {

                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    using (HttpClient client = new HttpClient())
                    {
                        String popupMsg = "";
                        //LoginAsTaxpayerSystem();
                        //client.BaseAddress = new Uri(@"" + env.ApiBaseUrl);
                        //// client.Timeout = new TimeSpan(100000000);
                        //client.DefaultRequestHeaders.Accept.Clear();
                        //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Accesstoken);

                        InvoiceBL invHModel = new InvoiceBL(_tenantService);
                        List<InvoiceHeaderModel> invoiceHeaderList = invHModel.oracleGetInvoiceHeader(invoiceID);
                        if (invoiceHeaderList[0].TrxType.CompareTo("Invoice") != 0)
                        {
                            #region Start to Create Body Response Without Signature
                            SignedCreaditOrDebitDocument d = new SignedCreaditOrDebitDocument();
                            d.documents = new List<SignedCreditOrDebitDocumentElement>();
                            SignedCreditOrDebitDocumentElement element = new SignedCreditOrDebitDocumentElement();
                            if (invoiceHeaderList[0].TrxType.CompareTo("Credit Note") == 0)
                                element.documentType = "c";
                            else
                                element.documentType = "d";
                            //#########################################################################
                            //############################ ISSUER #####################################
                            //#########################################################################
                            IssuerBL issuerBL = new IssuerBL(_tenantService);
                            IssuerModel issuerModel = issuerBL.GetProfileInfo(clientAddressID);
                            element.issuer = new Issuer();
                            element.issuer.id = issuerModel.RegNum;// "100710840";
                            element.issuer.name = issuerModel.CompanyName;//"مكتبة الاسكندرية";
                            element.issuer.type = issuerModel.Type;//"B";
                            element.issuer.address = new IssuerAddress();
                            element.issuer.address.country = issuerModel.Country;// "EG";
                            element.issuer.address.governate = issuerModel.Governate;// "Alexandria";
                            element.issuer.address.street = issuerModel.Street;// "0 / 1115   شارع بورسعيد امام المجمع النظرى الشاطبى الاسكندرية";
                            element.issuer.address.regionCity = issuerModel.RegionCity;//"XX";
                            element.issuer.address.branchID = issuerModel.BranchID;// "0";
                            element.issuer.address.buildingNumber = issuerModel.BuildNum;// "1";             
                                                                                         //##OPtinal
                            element.issuer.address.floor = "0";
                            element.issuer.address.landmark = "";
                            element.issuer.address.postalCode = "0";
                            element.issuer.address.room = "0";
                            element.issuer.address.additionalInformation = "";
                            //#########################################################################
                            //########################### RECEIVER ####################################
                            //#########################################################################
                            CustomerBL cusModel = new CustomerBL(_tenantService);
                            List<CustomerModel> customerList = cusModel.oracleGetCustomer(cusID);
                            element.receiver = new Receiver();
                            element.receiver.id = customerList[0].TaxpayerCode;//"100295584" //;
                            element.receiver.name = customerList[0].PartyName;//"شركة لابوار للمحلات السياحية";
                            element.receiver.type = customerList[0].CusType;// "B";
                            element.receiver.address = new ReceiverAddress();
                            element.receiver.address.country = customerList[0].Country;// "EG";
                            element.receiver.address.street = customerList[0].Street; //"3شارع شجرة الدر القاهرة";
                            element.receiver.address.regionCity = customerList[0].RegionCity;// "الزمالك";
                            element.receiver.address.governate = customerList[0].Governate;// "Alexandria";               
                            element.receiver.address.buildingNumber = customerList[0].BuildingNumber;// "1";
                                                                                                     //##OPtinal
                            element.receiver.address.floor = "0";
                            element.receiver.address.landmark = "";
                            element.receiver.address.postalCode = "0";
                            element.receiver.address.room = "0";
                            element.receiver.address.additionalInformation = "";
                            //##########################################################################
                            //########################### INVOICE DATA #################################
                            //##########################################################################  
                            element.documentTypeVersion = "1.0";
                            element.references = new List<string>();

                            String invoiceUUID = invHModel.getUUID(invoiceIDReturned ?? 0);
                            if (invoiceUUID.CompareTo("") == 0)
                            {
                                popupMsg = "THIS CREDIT NOTE IS NOT FOR A SUBMITTED INVOICE";
                                //return popupMsg;

                                Response.Result = false;
                                var error = new BaseError();
                                error.ErrorCode = "Err-eI5";
                                error.ErrorMSG = popupMsg;
                                Response.Errors.Add(error);
                            }
                            element.references.Add(invoiceUUID);
                            if (invoiceHeaderList[0].Trx_date != null)
                            {
                                DateTime date = DateTime.Now;
                                TimeSpan time = new TimeSpan(0, DateTime.Now.Hour, 0, 0);
                                element.dateTimeIssued = ((DateTime)invoiceHeaderList[0].Trx_date).Add(time).AddHours(-4).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
                            }
                            else
                            {
                                element.dateTimeIssued = DateTime.Now.AddHours(-4).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
                            }
                            //DateTime.Now.AddHours(-2).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");

                            //element.invoiceID = invoiceID;
                            element.taxpayerActivityCode = issuerModel.ActivityCode1;// "9101";//--->انشطة المكتبات والارشيف
                            element.internalID = invoiceHeaderList[0].Trx_number;// "3220002";//----->رقم داخلي للفاتورة                   
                                                                                 //##OPtinal
                            element.purchaseOrderReference = "";
                            element.purchaseOrderDescription = "";
                            element.salesOrderDescription = "";
                            element.salesOrderReference = "";
                            element.proformaInvoiceNumber = "";
                            //payment
                            element.payment = new Payment();
                            element.payment.bankAccountIBAN = "";
                            element.payment.bankAccountNo = "";
                            element.payment.bankAddress = "";
                            element.payment.bankName = "";
                            element.payment.swiftCode = "";
                            element.payment.terms = "";
                            //delivery
                            element.delivery = new Delivery();
                            element.delivery.grossWeight = 0;
                            element.delivery.approach = "";
                            element.delivery.exportPort = "";
                            element.delivery.terms = "";
                            element.delivery.packaging = "";
                            element.delivery.netWeight = 0;
                            element.delivery.dateValidity = "";                           
                            //##########################################################################
                            //########################### INVOICE LINES ################################
                            //##########################################################################                
                            InvoiceBL invLineBL = new InvoiceBL(_tenantService);
                            List<InvoiceLineModel> invLine = invLineBL.oracleGetInvoiceLine(salesOfferID);
                            element.invoiceLines = new List<InvoiceLine>();
                            ItemBL itmModel = new ItemBL(_tenantService);
                            int line = 0;
                            int length = invLine.Count;
                            decimal totSalesAmount = 0;
                            decimal netTotal = 0;
                            decimal totalAmount = 0;
                            decimal totalDiscount = 0;
                            List<TaxTotal> t = new List<TaxTotal>();
                            for (line = 0; line < length; line++)
                            {
                                //LINE

                                List<ItemModel> item = itmModel.oracleGetItem((long)invLine[line].ItemID);
                                InvoiceLine l1 = new InvoiceLine();
                                //l1.description = item[0].ItemDescription;
                                l1.description = itmModel.GetItemOfferPricingComment(invLine[line].SalesOfferProductID1);
                                l1.internalCode = item[0].ItemInternalCode;// "1371"; 
                                l1.itemType = item[0].ItemTypeEINVOICE;//"EGS ";//---->
                                l1.itemCode = item[0].ItemCode;// "EG-100710840-1371";
                                l1.unitType = item[0].ItemUnitTypeEINVOICE;// "EA";
                                l1.quantity = (Decimal)invLine[line].Quantity1;// Decimal.Parse(invLine[line].Quantity1.ToString() );//1;
                                UnitValue uv = new UnitValue();
                                uv.amountEGP = (Decimal)invLine[line].ItemPrice1;//2000;
                                uv.currencySold = "EGP";
                                l1.unitValue = uv;
                                l1.salesTotal = (Decimal)invLine[line].SalesTotal;//2000;
                                totSalesAmount += l1.salesTotal;
                                //DISCOUNT
                                Discount dis = new Discount();
                                dis.amount = l1.salesTotal * (Decimal)invLine[line].DiscountPercentage1 / 100;
                                dis.rate = (Decimal)invLine[line].DiscountPercentage1;
                                l1.discount = dis;
                                totalDiscount += dis.amount;
                                //--------------------------
                                l1.netTotal = (Decimal)invLine[line].SalesTotal - (l1.discount.amount);// 2000;                   
                                netTotal += l1.netTotal;
                                l1.valueDifference = 0;
                                l1.itemsDiscount = 0;
                                //----------------------------------------------------------------------------------------------------------------------
                                List<InvoiceLineTaxModel> TaxItem = invHModel.oracleGetInvoiceLineTax(salesOfferID, invLine[line].SalesOfferProductID1);
                                l1.taxableItems = new List<TaxableItem>();
                                //--TAX
                                decimal totalOfTaxableTax = 0;
                                decimal totalTaxPerLine = 0;
                                decimal T4Amount = 0;
                                foreach (InvoiceLineTaxModel taxItem in TaxItem)
                                {
                                    TaxableItem ti = new TaxableItem();
                                    if (taxItem.TaxName1.CompareTo("T1") == 0)
                                    {
                                        ti.amount = Decimal.Parse(String.Format("{0:0.00000}", (l1.netTotal + totalOfTaxableTax) * (Decimal)taxItem.TaxPercentage1 / 100));
                                        ti.subType = taxItem.SubTaxName1;
                                        ti.taxType = taxItem.TaxName1;
                                        ti.rate = Decimal.Parse(String.Format("{0:0.00}", taxItem.TaxPercentage1));
                                        if (line == 0)
                                        {
                                            TaxTotal tt = new TaxTotal();
                                            tt.amount = ti.amount;
                                            tt.taxType = taxItem.TaxName1;
                                            t.Add(tt);
                                        }
                                        else
                                        {
                                            bool found = false;
                                            for (int i = 0; i < t.Count; i++)
                                            {
                                                if (t[i].taxType.CompareTo(taxItem.TaxName1) == 0)
                                                {
                                                    t[i].amount += ti.amount;
                                                    found = true;
                                                    break;
                                                }
                                            }
                                            if (found == false)
                                            {
                                                TaxTotal tt = new TaxTotal();
                                                tt.amount = ti.amount;
                                                tt.taxType = taxItem.TaxName1;
                                                t.Add(tt);
                                            }
                                        }
                                    }
                                    else if (taxItem.TaxName1.CompareTo("T4") == 0) // Michael Modified
                                    {
                                        if (taxItem.IsPercentage1 == true) // Percentage Always
                                            ti.amount = Decimal.Parse(String.Format("{0:0.00000}", l1.netTotal * (Decimal)taxItem.TaxPercentage1 / 100));
                                        else
                                            ti.amount = (Decimal)taxItem.TaxValue1;

                                        ti.subType = taxItem.SubTaxName1;
                                        ti.taxType = taxItem.TaxName1;
                                        if (taxItem.IsPercentage1 == true)
                                            ti.rate = Decimal.Parse(String.Format("{0:0.00}", taxItem.TaxPercentage1));
                                        else
                                            ti.rate = 0;

                                        if (line == 0)
                                        {
                                            TaxTotal tt = new TaxTotal();
                                            tt.amount = ti.amount;
                                            tt.taxType = taxItem.TaxName1;
                                            t.Add(tt);
                                        }
                                        else
                                        {
                                            bool found = false;
                                            for (int i = 0; i < t.Count; i++)
                                            {
                                                if (t[i].taxType.CompareTo(taxItem.TaxName1) == 0)
                                                {
                                                    t[i].amount += ti.amount;
                                                    found = true;
                                                    break;
                                                }
                                            }
                                            if (found == false)
                                            {
                                                TaxTotal tt = new TaxTotal();
                                                tt.amount = ti.amount;
                                                tt.taxType = taxItem.TaxName1;
                                                t.Add(tt);
                                            }
                                        }
                                        T4Amount = ti.amount;
                                        //ti.amount = -1 * ti.amount; // Special case for T4  only subtract from totalTaxPerLine not form totalOfTaxableTax
                                        //T4Amount = ti.amount;
                                        //if (taxItem.TaxType1.CompareTo("Taxable Types") == 0)
                                        //    totalOfTaxableTax -= ti.amount;
                                    }
                                    else
                                    {
                                        if (taxItem.IsPercentage1 == true)
                                            ti.amount = Decimal.Parse(String.Format("{0:0.00000}", l1.netTotal * (Decimal)taxItem.TaxPercentage1 / 100));
                                        else
                                            ti.amount = (Decimal)taxItem.TaxValue1;

                                        ti.subType = taxItem.SubTaxName1;
                                        ti.taxType = taxItem.TaxName1;
                                        if (taxItem.IsPercentage1 == true)
                                            ti.rate = Decimal.Parse(String.Format("{0:0.00}", taxItem.TaxPercentage1));
                                        else
                                            ti.rate = 0;

                                        if (line == 0)
                                        {
                                            TaxTotal tt = new TaxTotal();
                                            tt.amount = ti.amount;
                                            tt.taxType = taxItem.TaxName1;
                                            t.Add(tt);
                                        }
                                        else
                                        {
                                            bool found = false;
                                            for (int i = 0; i < t.Count; i++)
                                            {
                                                if (t[i].taxType.CompareTo(taxItem.TaxName1) == 0)
                                                {
                                                    t[i].amount += ti.amount;
                                                    found = true;
                                                    break;
                                                }
                                            }
                                            if (found == false)
                                            {
                                                TaxTotal tt = new TaxTotal();
                                                tt.amount = ti.amount;
                                                tt.taxType = taxItem.TaxName1;
                                                t.Add(tt);
                                            }
                                        }
                                        if (taxItem.TaxType1.CompareTo("Taxable Types") == 0)
                                            totalOfTaxableTax += ti.amount;
                                    }

                                    if (taxItem.TaxName1.CompareTo("T4") != 0)
                                    {
                                        totalTaxPerLine += ti.amount;
                                    }
                                    l1.taxableItems.Add(ti);
                                }
                                //-----------------------------------------------------------------------------------------------------------------------
                                l1.totalTaxableFees = totalOfTaxableTax;
                                l1.total = l1.netTotal + totalTaxPerLine - T4Amount;
                                totalAmount += l1.total;
                                //----------------------------------------------------
                                element.invoiceLines.Add(l1);
                            }
                            //##########################################################################
                            element.totalSalesAmount = totSalesAmount;
                            element.totalDiscountAmount = totalDiscount;
                            element.netAmount = netTotal;
                            element.extraDiscountAmount = 0;
                            element.totalItemsDiscountAmount = 0;
                            element.totalAmount = totalAmount;
                            //##########################################################################
                            element.taxTotals = new List<TaxTotal>();
                            for (int i = 0; i < t.Count; i++)
                            {
                                element.taxTotals.Add(t[i]);
                            }
                            //##########################################################################
                            d.documents.Add(element);
                            #endregion

                            #region Attach Signature
                            //String uridirectory = Path.GetPathRoot(Environment.SystemDirectory) + "\\Signature";
                            //string directory = new Uri(uridirectory).LocalPath;
                            //File.WriteAllText(@"" + directory + "\\SourceDocumentJson.json", JsonConvert.SerializeObject(d.documents[0], Formatting.Indented));
                            ////RUN BATCH
                            //runBatchFile();
                            ////  READ OUTPUT FILE
                            //string json = "";
                            //using (StreamReader r = new StreamReader(@"" + directory + "\\FullSignedDocument.json"))
                            //{
                            //    json = r.ReadToEnd();
                            //}
                            #endregion

                            //var submissionText = new StringContent(JsonConvert.SerializeObject(d, Formatting.Indented), Encoding.UTF8, "application/json");
                            //Response.Message = JsonConvert.SerializeObject(d, Formatting.Indented);
                            Response.Message = JsonConvert.SerializeObject(d.documents[0], Formatting.Indented);

                            //# region Start To Send Body

                            //var response = client.PostAsync("api/v1.0/documentsubmissions", submissionText).GetAwaiter().GetResult();
                            //var response_content = response.Content.ReadAsStringAsync();
                            //documentsubmissionsResult SubmissionResult = JsonConvert.DeserializeObject<documentsubmissionsResult>(response_content.Result);
                            //if (SubmissionResult.acceptedDocuments != null && SubmissionResult.acceptedDocuments.Count > 0)
                            //{
                            //    string internalId = SubmissionResult.acceptedDocuments[0].internalId;
                            //    string longId = SubmissionResult.acceptedDocuments[0].longId;
                            //    string uuid = SubmissionResult.acceptedDocuments[0].uuid;

                            //    popupMsg = popupMsg + "acceptedDocuments count : " + SubmissionResult.acceptedDocuments.Count + Environment.NewLine + " internalId : " + internalId + Environment.NewLine +
                            //        " longId : " + longId + Environment.NewLine + " uuid : " + uuid;

                            //    invLineBL.saveInvoiceUUID(element.invoiceID, uuid, "Accepted", DateTime.Now);

                            //}
                            //if (SubmissionResult.rejectedDocuments != null && SubmissionResult.rejectedDocuments.Count > 0)
                            //{
                            //    string code = SubmissionResult.rejectedDocuments[0].error.code;
                            //    string target = SubmissionResult.rejectedDocuments[0].error.target;
                            //    string message = SubmissionResult.rejectedDocuments[0].error.message;
                            //    string details = "";
                            //    for (int e = 0; e < SubmissionResult.rejectedDocuments[0].error.details.Length; e++)
                            //    {
                            //        details = details + "error " + (e + 1) + Environment.NewLine + " Code:" + SubmissionResult.rejectedDocuments[0].error.details[e].code + Environment.NewLine + " Target:"
                            //            + SubmissionResult.rejectedDocuments[0].error.details[e].target + Environment.NewLine
                            //            + "Message" + SubmissionResult.rejectedDocuments[0].error.details[e].message + Environment.NewLine;
                            //    }
                            //    popupMsg = popupMsg + "rejectedDocuments count : " + SubmissionResult.rejectedDocuments.Count + Environment.NewLine + " code : " + code + Environment.NewLine
                            //        + "  target : " + target + Environment.NewLine + "  message : "
                            //        + message
                            //     + Environment.NewLine + "  details : " + details + Environment.NewLine;

                            //    invLineBL.saveInvoiceUUID(element.invoiceID, "", "Rejected " + details, DateTime.Now);
                            //}
                            //#endregion
                        }
                        else
                        {

                            SignedDocument d = new SignedDocument();
                            d.documents = new List<SignedDocumentElement>();
                            SignedDocumentElement element = new SignedDocumentElement();
                            element.documentType = "I";
                            //#########################################################################
                            //############################ ISSUER #####################################
                            //#########################################################################
                            IssuerBL issuerBL = new IssuerBL(_tenantService);
                            IssuerModel issuerModel = issuerBL.GetProfileInfo(clientAddressID);
                            element.issuer = new Issuer();
                            element.issuer.id = issuerModel.RegNum;// "100710840";
                            element.issuer.name = issuerModel.CompanyName;//"مكتبة الاسكندرية";
                            element.issuer.type = issuerModel.Type;//"B";
                            element.issuer.address = new IssuerAddress();
                            element.issuer.address.country = issuerModel.Country;// "EG";
                            element.issuer.address.governate = issuerModel.Governate;// "Alexandria";
                            element.issuer.address.street = issuerModel.Street;// "0 / 1115   شارع بورسعيد امام المجمع النظرى الشاطبى الاسكندرية";
                            element.issuer.address.regionCity = issuerModel.RegionCity;//"XX";
                            element.issuer.address.branchID = issuerModel.BranchID;// "0";
                            element.issuer.address.buildingNumber = issuerModel.BuildNum;// "1";             
                                                                                         //##OPtinal
                            element.issuer.address.floor = "0";
                            element.issuer.address.landmark = "";
                            element.issuer.address.postalCode = "0";
                            element.issuer.address.room = "0";
                            element.issuer.address.additionalInformation = "";
                            //#########################################################################
                            //########################### RECEIVER ####################################
                            //#########################################################################
                            CustomerBL cusModel = new CustomerBL(_tenantService);
                            List<CustomerModel> customerList = cusModel.oracleGetCustomer(cusID);
                            element.receiver = new Receiver();
                            element.receiver.id = customerList[0].TaxpayerCode;//"100295584";
                            element.receiver.name = customerList[0].PartyName;//"شركة لابوار للمحلات السياحية";
                            element.receiver.type = customerList[0].CusType;// "B";
                            element.receiver.address = new ReceiverAddress();
                            element.receiver.address.country = customerList[0].Country;// "EG";
                            element.receiver.address.street = customerList[0].Street; //"3شارع شجرة الدر القاهرة";
                            element.receiver.address.regionCity = customerList[0].RegionCity;// "الزمالك";
                            element.receiver.address.governate = customerList[0].Governate;// "Alexandria";               
                            element.receiver.address.buildingNumber = customerList[0].BuildingNumber;// "1";
                                                                                                     //##OPtinal
                            element.receiver.address.floor = "0";
                            element.receiver.address.landmark = "";
                            element.receiver.address.postalCode = "0";
                            element.receiver.address.room = "0";
                            element.receiver.address.additionalInformation = "";
                            //##########################################################################
                            //########################### INVOICE DATA #################################
                            //##########################################################################  
                            if (invoiceHeaderList[0].Trx_date != null)
                            {
                                DateTime date = DateTime.Now;
                                TimeSpan time = new TimeSpan(0, DateTime.Now.Hour, 0, 0);
                                element.dateTimeIssued = ((DateTime)invoiceHeaderList[0].Trx_date).Add(time).AddHours(-4).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
                            }
                            else
                            {
                                element.dateTimeIssued = DateTime.Now.AddHours(-4).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
                            }
                            //element.dateTimeIssued =// invoiceHeaderList[0].Trx_date.AddHours(-2).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
                            //    DateTime.Now.AddHours(-2).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");                   
                            element.documentTypeVersion = "1.0";
                          //  element.invoiceID = invoiceID;
                            element.taxpayerActivityCode = issuerModel.ActivityCode1;// "9101";//--->انشطة المكتبات والارشيف
                            element.internalID = invoiceHeaderList[0].Trx_number;// "3220002";//----->رقم داخلي للفاتورة 
                                                                                 //##OPtinal
                            element.purchaseOrderReference = "";
                            element.purchaseOrderDescription = "";
                            element.salesOrderDescription = "";
                            element.salesOrderReference = "";
                            element.proformaInvoiceNumber = "";
                            //payment
                            element.payment = new Payment();
                            element.payment.bankAccountIBAN = "";
                            element.payment.bankAccountNo = "";
                            element.payment.bankAddress = "";
                            element.payment.bankName = "";
                            element.payment.swiftCode = "";
                            element.payment.terms = "";
                            //delivery
                            element.delivery = new Delivery();
                            element.delivery.grossWeight = 0;
                            element.delivery.approach = "";
                            element.delivery.exportPort = "";
                            element.delivery.terms = "";
                            element.delivery.packaging = "";
                            element.delivery.netWeight = 0;
                            element.delivery.dateValidity = "";
                            //##########################################################################
                            //########################### INVOICE LINES ################################
                            //##########################################################################                
                            InvoiceBL invLineBL = new InvoiceBL(_tenantService);
                            List<InvoiceLineModel> invLine = invLineBL.oracleGetInvoiceLine(salesOfferID);
                            element.invoiceLines = new List<InvoiceLine>();
                            ItemBL itmModel = new ItemBL(_tenantService);
                            int line = 0;
                            int length = invLine.Count;
                            decimal totSalesAmount = 0;
                            decimal netTotal = 0;
                            decimal totalAmount = 0;
                            decimal totalDiscount = 0;
                            List<TaxTotal> t = new List<TaxTotal>();
                            for (line = 0; line < length; line++)
                            {
                                //LINE

                                List<ItemModel> item = itmModel.oracleGetItem((long)invLine[line].ItemID);
                                InvoiceLine l1 = new InvoiceLine();
                                //l1.description = item[0].ItemDescription;
                                l1.description = itmModel.GetItemOfferPricingComment(invLine[line].SalesOfferProductID1);
                                l1.internalCode = item[0].ItemInternalCode;// "1371"; 
                                l1.itemType = item[0].ItemTypeEINVOICE;//"EGS ";//---->
                                l1.itemCode = item[0].ItemCode;// "EG-100710840-1371";
                                l1.unitType = item[0].ItemUnitTypeEINVOICE;// "EA";
                                l1.quantity = (Decimal)invLine[line].Quantity1;// Decimal.Parse(invLine[line].Quantity1.ToString() );//1;
                                UnitValue uv = new UnitValue();
                                uv.amountEGP = (Decimal)invLine[line].ItemPrice1;//2000;
                                uv.currencySold = "EGP";
                                l1.unitValue = uv;
                                l1.salesTotal = (Decimal)invLine[line].SalesTotal;//2000;
                                totSalesAmount += l1.salesTotal;
                                //DISCOUNT
                                Discount dis = new Discount();
                                dis.amount = l1.salesTotal * (Decimal)invLine[line].DiscountPercentage1 / 100;
                                dis.rate = (Decimal)invLine[line].DiscountPercentage1;
                                l1.discount = dis;
                                totalDiscount += dis.amount;
                                //--------------------------
                                l1.netTotal = (Decimal)invLine[line].SalesTotal - (l1.discount.amount);// 2000;                   
                                netTotal += l1.netTotal;
                                l1.valueDifference = 0;
                                l1.itemsDiscount = 0;
                                //----------------------------------------------------------------------------------------------------------------------
                                List<InvoiceLineTaxModel> TaxItem = invHModel.oracleGetInvoiceLineTax(salesOfferID, invLine[line].SalesOfferProductID1);
                                l1.taxableItems = new List<TaxableItem>();
                                //--TAX
                                decimal totalOfTaxableTax = 0;
                                decimal totalTaxPerLine = 0;
                                // T4 is Found
                                decimal T4Amount = 0;
                                foreach (InvoiceLineTaxModel taxItem in TaxItem)
                                {
                                    TaxableItem ti = new TaxableItem();
                                    if (taxItem.TaxName1.CompareTo("T1") == 0)
                                    {
                                        ti.amount = Decimal.Parse(String.Format("{0:0.00000}", (l1.netTotal + (totalOfTaxableTax)) * (Decimal)taxItem.TaxPercentage1 / 100));
                                        ti.subType = taxItem.SubTaxName1;
                                        ti.taxType = taxItem.TaxName1;
                                        ti.rate = Decimal.Parse(String.Format("{0:0.00}", taxItem.TaxPercentage1));
                                        if (line == 0)
                                        {
                                            TaxTotal tt = new TaxTotal();
                                            tt.amount = ti.amount;
                                            tt.taxType = taxItem.TaxName1;
                                            t.Add(tt);
                                        }
                                        else
                                        {
                                            bool found = false;
                                            for (int i = 0; i < t.Count; i++)
                                            {
                                                if (t[i].taxType.CompareTo(taxItem.TaxName1) == 0)
                                                {
                                                    t[i].amount += ti.amount;
                                                    found = true;
                                                    break;
                                                }
                                            }
                                            if (found == false)
                                            {
                                                TaxTotal tt = new TaxTotal();
                                                tt.amount = ti.amount;
                                                tt.taxType = taxItem.TaxName1;
                                                t.Add(tt);
                                            }
                                        }
                                    }
                                    else if (taxItem.TaxName1.CompareTo("T4") == 0) // Michael Modified
                                    {
                                        if (taxItem.IsPercentage1 == true) // Percentage Always
                                            ti.amount = Decimal.Parse(String.Format("{0:0.00000}", l1.netTotal * (Decimal)taxItem.TaxPercentage1 / 100));
                                        else
                                            ti.amount = (Decimal)taxItem.TaxValue1;

                                        ti.subType = taxItem.SubTaxName1;
                                        ti.taxType = taxItem.TaxName1;
                                        if (taxItem.IsPercentage1 == true)
                                            ti.rate = Decimal.Parse(String.Format("{0:0.00}", taxItem.TaxPercentage1));
                                        else
                                            ti.rate = 0;

                                        if (line == 0)
                                        {
                                            TaxTotal tt = new TaxTotal();
                                            tt.amount = ti.amount;
                                            tt.taxType = taxItem.TaxName1;
                                            t.Add(tt);
                                        }
                                        else
                                        {
                                            bool found = false;
                                            for (int i = 0; i < t.Count; i++)
                                            {
                                                if (t[i].taxType.CompareTo(taxItem.TaxName1) == 0)
                                                {
                                                    t[i].amount += ti.amount;
                                                    found = true;
                                                    break;
                                                }
                                            }
                                            if (found == false)
                                            {
                                                TaxTotal tt = new TaxTotal();
                                                tt.amount = ti.amount;
                                                tt.taxType = taxItem.TaxName1;
                                                t.Add(tt);
                                            }
                                        }
                                        T4Amount = ti.amount;
                                        //ti.amount = -1 * ti.amount; // Special case for T4  only subtract from totalTaxPerLine not form totalOfTaxableTax
                                        //T4Amount = ti.amount;
                                        //if (taxItem.TaxType1.CompareTo("Taxable Types") == 0)
                                        //    totalOfTaxableTax -= ti.amount;
                                    }
                                    else
                                    {
                                        if (taxItem.IsPercentage1 == true)
                                            ti.amount = Decimal.Parse(String.Format("{0:0.00000}", l1.netTotal * (Decimal)taxItem.TaxPercentage1 / 100));
                                        else
                                            ti.amount = (Decimal)taxItem.TaxValue1;

                                        ti.subType = taxItem.SubTaxName1;
                                        ti.taxType = taxItem.TaxName1;
                                        if (taxItem.IsPercentage1 == true)
                                            ti.rate = Decimal.Parse(String.Format("{0:0.00}", taxItem.TaxPercentage1));
                                        else
                                            ti.rate = 0;

                                        if (line == 0)
                                        {
                                            TaxTotal tt = new TaxTotal();
                                            tt.amount = ti.amount;
                                            tt.taxType = taxItem.TaxName1;
                                            t.Add(tt);
                                        }
                                        else
                                        {
                                            bool found = false;
                                            for (int i = 0; i < t.Count; i++)
                                            {
                                                if (t[i].taxType.CompareTo(taxItem.TaxName1) == 0)
                                                {
                                                    t[i].amount += ti.amount;
                                                    found = true;
                                                    break;
                                                }
                                            }
                                            if (found == false)
                                            {
                                                TaxTotal tt = new TaxTotal();
                                                tt.amount = ti.amount;
                                                tt.taxType = taxItem.TaxName1;
                                                t.Add(tt);
                                            }
                                        }
                                        if (taxItem.TaxType1.CompareTo("Taxable Types") == 0)
                                            totalOfTaxableTax += ti.amount;
                                    }
                                    if (taxItem.TaxName1.CompareTo("T4") != 0)
                                    {
                                        totalTaxPerLine += ti.amount;
                                    }
                                    l1.taxableItems.Add(ti);
                                }
                                //-----------------------------------------------------------------------------------------------------------------------
                                l1.totalTaxableFees = totalOfTaxableTax;
                                l1.total = l1.netTotal + totalTaxPerLine - T4Amount;
                                totalAmount += l1.total;
                                //----------------------------------------------------
                                element.invoiceLines.Add(l1);
                            }
                            //##########################################################################
                            element.totalSalesAmount = totSalesAmount;
                            element.totalDiscountAmount = totalDiscount;
                            element.netAmount = netTotal;
                            element.extraDiscountAmount = 0;
                            element.totalItemsDiscountAmount = 0;
                            element.totalAmount = totalAmount;
                            //##########################################################################
                            element.taxTotals = new List<TaxTotal>();
                            for (int i = 0; i < t.Count; i++)
                            {
                                element.taxTotals.Add(t[i]);
                            }
                            //##########################################################################
                            d.documents.Add(element);

                            //var submissionText = new StringContent(JsonConvert.SerializeObject(d, Formatting.Indented), Encoding.UTF8, "application/json");
                            Response.Message = JsonConvert.SerializeObject(d.documents[0], Formatting.Indented);


                            //var response = client.PostAsync("api/v1.0/documentsubmissions", submissionText).GetAwaiter().GetResult();
                            //var response_content = response.Content.ReadAsStringAsync();
                            //documentsubmissionsResult SubmissionResult = JsonConvert.DeserializeObject<documentsubmissionsResult>(response_content.Result);
                            //if (SubmissionResult.acceptedDocuments != null && SubmissionResult.acceptedDocuments.Count > 0)
                            //{
                            //    string internalId = SubmissionResult.acceptedDocuments[0].internalId;
                            //    string longId = SubmissionResult.acceptedDocuments[0].longId;
                            //    string uuid = SubmissionResult.acceptedDocuments[0].uuid;

                            //    popupMsg = popupMsg + "acceptedDocuments count : " + SubmissionResult.acceptedDocuments.Count + Environment.NewLine + " internalId : " + internalId + Environment.NewLine +
                            //        " longId : " + longId + Environment.NewLine + " uuid : " + uuid;
                            //    invLineBL.saveInvoiceUUID(element.invoiceID, uuid, "Accepted", DateTime.Now);
                            //}
                            //if (SubmissionResult.rejectedDocuments != null && SubmissionResult.rejectedDocuments.Count > 0)
                            //{
                            //    string code = SubmissionResult.rejectedDocuments[0].error.code;
                            //    string target = SubmissionResult.rejectedDocuments[0].error.target;
                            //    string message = SubmissionResult.rejectedDocuments[0].error.message;
                            //    string details = "";
                            //    for (int e = 0; e < SubmissionResult.rejectedDocuments[0].error.details.Length; e++)
                            //    {
                            //        details = details + "error " + (e + 1) + Environment.NewLine + " Code:" + SubmissionResult.rejectedDocuments[0].error.details[e].code + Environment.NewLine + " Target:"
                            //            + SubmissionResult.rejectedDocuments[0].error.details[e].target + Environment.NewLine
                            //            + "Message" + SubmissionResult.rejectedDocuments[0].error.details[e].message + Environment.NewLine;
                            //    }
                            //    popupMsg = popupMsg + "rejectedDocuments count : " + SubmissionResult.rejectedDocuments.Count + Environment.NewLine + " code : " + code + Environment.NewLine
                            //        + "  target : " + target + Environment.NewLine + "  message : "
                            //        + message
                            //     + Environment.NewLine + "  details : " + details + Environment.NewLine;


                            //    invLineBL.saveInvoiceUUID(element.invoiceID, "", "Rejected " + details, DateTime.Now);
                            //}
                        }
                        //return popupMsg;
                    }
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                var error = new BaseError();
                error.ErrorCode = "Err-eI110";
                error.ErrorMSG = "ex : " + ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public EInvoiceDocumentResponse GetEInvoiceDocument(string UUID)
        {
            EInvoiceDocumentResponse Response = new EInvoiceDocumentResponse();
            Response.Result = true;
            Response.Errors = new List<BaseError>();
            try
            {
                if (string.IsNullOrEmpty(UUID))
                {
                    Response.Result = false;
                    var error = new BaseError();
                    error.ErrorCode = "Err-eI1";
                    error.ErrorMSG = "UUID is required";
                    Response.Errors.Add(error);
                }

                if (Response.Result)
                {
                    LoginAsTaxpayerSystem();
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(@"" + env.ApiBaseUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Accesstoken);

                        var request = new HttpRequestMessage
                        {
                            Method = HttpMethod.Get,
                            RequestUri = new Uri(client.BaseAddress + "api/v1.0/documents/" + UUID + "/raw"),
                        };
                        var response = client.SendAsync(request).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            var ResponseJsonString = response.Content.ReadAsStringAsync().Result;
                            var ResponseJsonObject = JsonConvert.DeserializeObject<EInvoiceDocumentModel>(ResponseJsonString);
                            Response.EInvoiceDocumentModel = ResponseJsonObject;
                        }
                    }
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                var error = new BaseError();
                error.ErrorCode = "Err-eI110";
                error.ErrorMSG = "ex : " + ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }
        
        public EInvoiceDocumentPrintOutResponse GetEInvoiceDocumentPrintOut(string UUID)
        {
            EInvoiceDocumentPrintOutResponse Response = new EInvoiceDocumentPrintOutResponse();
            Response.Result = true;
            Response.Errors = new List<BaseError>();
            try
            {
                if (string.IsNullOrEmpty(UUID))
                {
                    Response.Result = false;
                    var error = new BaseError();
                    error.ErrorCode = "Err-eI1";
                    error.ErrorMSG = "UUID is required";
                    Response.Errors.Add(error);
                }

                if (Response.Result)
                {
                    LoginAsTaxpayerSystem();
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(@"" + env.ApiBaseUrl);
                        client.Timeout = TimeSpan.FromSeconds(30); 
                        client.DefaultRequestHeaders.Clear();
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.ConnectionClose = true;
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Accesstoken);

                        var request = new HttpRequestMessage
                        {
                            Method = HttpMethod.Get,
                            RequestUri = new Uri(client.BaseAddress + "api/v1.0/documents/" + UUID + "/pdf"),
                        };
                        var response = client.SendAsync(request).Result;
                        byte[] buffer = null;

                        if (response.IsSuccessStatusCode)
                        {
                            Stream streamToReadFrom = response.Content.ReadAsStreamAsync().Result;
                            using (MemoryStream ms = new MemoryStream())
                            {
                                streamToReadFrom.CopyToAsync(ms);
                                buffer = ms.ToArray();
                            }

                            DateTime dt = DateTime.Now;
                            string PathDay = dt.ToString("yyyy-MM-dd");
                            string root = "~/Attachments/SDK_PDF";
                            string subdir = root + PathDay;

                            var FilePath = Common.SaveFile(root, subdir, buffer, UUID, "pdf",_host);

                            Response.EInvoiceDocumentFilePath = FilePath;
                        }
                    }
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                var error = new BaseError();
                error.ErrorCode = "Err-eI110";
                error.ErrorMSG = "ex : " + ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public RecentDocumentsResponse GetRecentEInvoiceDocuments(int? CurrentPage, int? ItemsNumberPerPage)
        {
            RecentDocumentsResponse Response = new RecentDocumentsResponse();
            Response.Result = true;
            Response.Errors = new List<BaseError>();
            try
            {
                var currentPage = 1;
                if (CurrentPage != 0 || CurrentPage != null)
                {
                    currentPage = (int)CurrentPage;
                }
                var itemsNumberPerPage = 10;
                if (ItemsNumberPerPage != 0 || ItemsNumberPerPage != null)
                {
                    itemsNumberPerPage = (int)ItemsNumberPerPage;
                }

                if (Response.Result)
                {
                    LoginAsTaxpayerSystem();
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    using (HttpClient client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(@"" + env.ApiBaseUrl);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Accesstoken);

                        var request = new HttpRequestMessage
                        {
                            Method = HttpMethod.Get,
                            RequestUri = new Uri(client.BaseAddress + "api/v1.0/documents/recent"),
                            Headers =
                            {
                                { "PageNo", currentPage.ToString()},
                                { "PageSize", itemsNumberPerPage.ToString()}
                            }
                        };
                        var response = client.SendAsync(request).Result;
                        if (response.IsSuccessStatusCode)
                        {
                            var ResponseJsonString = response.Content.ReadAsStringAsync().Result;
                            var ResponseJsonObject = JsonConvert.DeserializeObject<RecentDocumentsModel>(ResponseJsonString);
                            Response.RecentDocumentsModel = ResponseJsonObject;
                        }
                    }
                }
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                var error = new BaseError();
                error.ErrorCode = "Err-eI110";
                error.ErrorMSG = "ex : " + ex.Message;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public string LoginToken()
        {
            LoginAsTaxpayerSystem();
            return Accesstoken;
        }
    }
}

