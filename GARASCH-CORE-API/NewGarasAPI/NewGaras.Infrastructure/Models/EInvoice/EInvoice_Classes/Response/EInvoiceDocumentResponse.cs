using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Oracle.Model;

namespace NewGaras.Infrastructure.Models.EInvoice.EInvoice_Classes.Response
{
    [DataContract]
    public class EInvoiceDocumentResponse
    {
        EInvoiceDocumentModel eInvoiceDocumentModel;

        bool result;
        List<BaseError> errors;

        [DataMember]
        public EInvoiceDocumentModel EInvoiceDocumentModel
        {
            get
            {
                return eInvoiceDocumentModel;
            }

            set
            {
                eInvoiceDocumentModel = value;
            }
        }
        [DataMember]
        public bool Result
        {
            get
            {
                return result;
            }

            set
            {
                result = value;
            }
        }
        [DataMember]
        public List<BaseError> Errors
        {
            get
            {
                return errors;
            }

            set
            {
                errors = value;
            }
        }
    }

    [DataContract]
    public class EInvoiceDocumentPrintOutResponse
    {
        string eInvoiceDocumentFilePath;

        bool result;
        List<BaseError> errors;

        [DataMember]
        public string EInvoiceDocumentFilePath
        {
            get
            {
                return eInvoiceDocumentFilePath;
            }

            set
            {
                eInvoiceDocumentFilePath = value;
            }
        }
        [DataMember]
        public bool Result
        {
            get
            {
                return result;
            }

            set
            {
                result = value;
            }
        }
        [DataMember]
        public List<BaseError> Errors
        {
            get
            {
                return errors;
            }

            set
            {
                errors = value;
            }
        }
    }

    [DataContract]
    public class RecentDocumentsResponse
    {
        RecentDocumentsModel recentDocumentsModel;

        bool result;
        List<BaseError> errors;

        [DataMember]
        public RecentDocumentsModel RecentDocumentsModel
        {
            get
            {
                return recentDocumentsModel;
            }

            set
            {
                recentDocumentsModel = value;
            }
        }
        [DataMember]
        public bool Result
        {
            get
            {
                return result;
            }

            set
            {
                result = value;
            }
        }
        [DataMember]
        public List<BaseError> Errors
        {
            get
            {
                return errors;
            }

            set
            {
                errors = value;
            }
        }
    }
}
