using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Response
{
    public class AuthenticateResult
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string scope { get; set; }

    }
    public class AuthenticateResultByUser
    {
        public AuthenticateResult authenticateResult { get; set; }
        public DateTime validTo { get; set; }
        public AuthenticateResultByUser(AuthenticateResult authenticateResult, DateTime validTo)
        {
            this.authenticateResult = authenticateResult;
            this.validTo = validTo;
        }
    }
}