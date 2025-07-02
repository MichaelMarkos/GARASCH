﻿//using DocumentFormat.OpenXml.InkML;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
//using NewGarasAPI.Controllers;
using NewGaras.Infrastructure.Entities;
using NewGarasAPI.Models.Account;
using NewGarasAPI.Models.AccountAndFinance;
using NewGarasAPI.Models.Admin;
using NewGarasAPI.Models.HR;
using NewGarasAPI.Models.Supplier;
using NewGarasAPI.Models.User;
using System.Drawing;
using System.Linq.Expressions;
using NewGaras.Infrastructure.DBContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using NewGaras.Infrastructure.Models.Admin;
using System.Net;
using NewGarasAPI.Models.Project.UsedInResponses;
using static iTextSharp.text.pdf.PdfStructTreeController;
namespace NewGaras.Infrastructure
{
    public static class Common
    {
        public static DateTime StartYear;
        public static DateTime CurrentEndYear;
        //public static GarasTestContext _context;

        static Common()
        {
            StartYear = new DateTime(DateTime.Now.Year, 1, 1);
            CurrentEndYear = new DateTime(DateTime.Now.Year + 1, 1, 1);
            //_context = new GarasTestContext();
        }

        public static string GetUserPhoto(long UserID, GarasTestContext _Context)
        {
            string UserPhoto = null;
            if (UserID != 0)
            {
                // var UserDB = _Context.proc_UserLoadByPrimaryKey(UserID).FirstOrDefault();                          

                var UserDB = _Context.Users.Find(UserID);

                if (UserDB != null)
                {
                    UserPhoto = UserDB.PhotoUrl;
                }
            }

            return UserPhoto;
        }
        public static string GetDailyJournalEntrySerial(long DailyJournalEntryID, GarasTestContext _Context)
        {
            string Serial = "";
            if (DailyJournalEntryID != 0)
            {
                var DailyJournalEntryDB = _Context.VDailyJournalEntries.Where(x => x.Id == DailyJournalEntryID).FirstOrDefault();
                if (DailyJournalEntryDB != null)
                {
                    Serial = DailyJournalEntryDB.Serial;
                }
            }
            return Serial;
        }



        public static bool CheckUserIsGroupUser(int UserID, List<long> GroupDB, GarasTestContext _Context)
        {
            bool IsFound = false;
            if (UserID != 0)
            {

                var GroupUserDB = _Context.GroupUsers.Where(x => x.UserId == UserID && GroupDB.Contains(x.GroupId)).FirstOrDefault();
                if (GroupUserDB != null)
                {
                    IsFound = true;
                }
            }

            return IsFound;
        }
        /*public static string GetTaskTypeName(int ID, GarasTestContext _Context)
        {
            string Name = "";
            if (ID != 0)
            {
                var TaskTypeNameDB = _Context.TaskTypes.Find(ID);
                if (TaskTypeNameDB != null)
                {
                    Name = TaskTypeNameDB.Name;
                }
            }
            return Name;
        }*/
        public static bool CheckUserRole(long UserId, int RoleID, GarasTestContext _Context)
        {
            bool Res = false;
            var LoadObjDB = _Context.UserRoles.Where(x => x.UserId == UserId && x.RoleId == RoleID).FirstOrDefault();
            if (LoadObjDB != null)
            {
                Res = true;
            }
            return Res;
        }

        /*public static long CreateNotification(long ToUserID, string _Title, string _Description, string _URL, bool _New, long? _FromUserID, int? _NotificationProcessID, GarasTestContext _Context)
        {
            *//*
##########################################################################################
########################### NOTE ########################################################
Using ToUserID column as FromUserID 
Using FromUserID column as ToUserID

      because must be there sender (From UserID ) but reciver not mandatory if null (meaning send this notification to all)

      ##########################################################################################
###############################################################################################
*//*
             long NotificationID = 0;
            SqlParameter IDNotification = new SqlParameter
            {
                SqlDbType = System.Data.SqlDbType.BigInt,
                ParameterName = "ID",
                Direction = System.Data.ParameterDirection.Output
            };
            SqlParameter UserID = new SqlParameter
            {
                SqlDbType = System.Data.SqlDbType.BigInt,
                ParameterName = "UserID",
                Value = ToUserID
            };
            SqlParameter Title = new SqlParameter
            {
                SqlDbType = System.Data.SqlDbType.NVarChar,
                ParameterName = "Title",
                Value = _Title
            };
            SqlParameter Description = new SqlParameter
            {
                SqlDbType = System.Data.SqlDbType.NText,
                ParameterName = "Description",
                Value = _Description
            };
            SqlParameter Date = new SqlParameter
            {
                SqlDbType = System.Data.SqlDbType.DateTime,
                ParameterName = "Date",
                Value = DateTime.Now
            };
            SqlParameter URL = new SqlParameter
            {
                SqlDbType = System.Data.SqlDbType.NVarChar,
                ParameterName = "URL",
                Value = _URL
            };
            SqlParameter New = new SqlParameter
            {
                SqlDbType = System.Data.SqlDbType.Bit,
                ParameterName = "New",
                Value = _New
            };
            SqlParameter FromUserId = new SqlParameter
            {
                SqlDbType = System.Data.SqlDbType.BigInt,
                ParameterName = "FromUserId",
                Value = _FromUserID == 0 ? DBNull.Value : _FromUserID
            };
            SqlParameter NotificationProcessId = new SqlParameter
            {
                SqlDbType = System.Data.SqlDbType.Int,
                ParameterName = "NotificationProcessId",
                Value = _NotificationProcessID == 0 ? DBNull.Value : _NotificationProcessID
            };
            object[] param = new object[] {IDNotification,UserID,Title,Description,Date,URL,
                            New,FromUserId,NotificationProcessId};
            var NotificationInsert = _Context.Database.SqlQueryRaw<long>("Exec proc_NotificationInsert @ID output,@UserID ,@Title ,@Description ,@Date ,@URL ,@New ,@FromUserId ,@NotificationProcessId ", param).AsEnumerable().FirstOrDefault();
            //var NotificationInsert = _Context.proc_NotificationInsert(IDNotification,
            //                                                                ToUserID, // From USer ID
            //                                                                Title,
            //                                                                Description,
            //                                                                DateTime.Now,
            //                                                                URL,
            //                                                                New,
            //                                                                FromUserID, // To USerID
            //                                                                NotificationProcessID);


            if (IDNotification.Value != null)
            {
                NotificationID = (long)IDNotification.Value;
            }

            return NotificationID;
        }*/
        public static string GetRoleName(int id, GarasTestContext _Context)
        {
            string name = "";
            if (id != 0)
            {
                var ID = new SqlParameter("ID", System.Data.SqlDbType.BigInt);
                ID.Value = id;


                object[] param = new object[] { ID };

                var LoadObjDB = _Context.Database.SqlQueryRaw<proc_RoleLoadByPrimaryKey_Result>("Exec proc_RoleLoadByPrimaryKey @ID", param).AsEnumerable().FirstOrDefault();

                if (LoadObjDB != null)
                {
                    name = LoadObjDB.Name;
                }

            }
            return name;
        }
        public static string GetModuleName(long id, GarasTestContext _Context)
        {
            string name = "";
            var LoadObjDB = _Context.Modules.Find(id);
            if (LoadObjDB != null)
            {
                name = LoadObjDB.Name;
            }
            return name;
        }
        public static List<TreeViewDto2> BuildTreeViews2(string pid, IList<TreeViewDto2> candicates)
        {
            var subs = candicates.Where(c => c.parentId == pid).ToList();
            if (subs.Count() == 0)
            {
                return new List<TreeViewDto2>();
            }
            foreach (var i in subs)
            {
                i.subs = BuildTreeViews2(i.id, candicates);
            }
            return subs;
        }
        public static string GetUserName(long UserID, GarasTestContext _Context)
        {
            string UserName = "";
            if (UserID != 0)
            {
                //var UserObjDB = _Context.proc_UserLoadByPrimaryKey(UserID).FirstOrDefault();
                /*var ID = new SqlParameter("ID", System.Data.SqlDbType.BigInt);
                ID.Value = UserID;


                object[] param = new object[] { ID };


                var UserObjDB = _Context.Database.SqlQueryRaw<proc_UserLoadByPrimaryKey_Result>("Exec proc_UserLoadByPrimaryKey @ID", param).AsEnumerable().FirstOrDefault();

                if (UserObjDB != null)
                {
                    UserName = UserObjDB.FirstName + " " + UserObjDB.LastName;
                }*/
                var User = _Context.Users.Find(UserID);
                if (User != null)
                {
                    UserName = User.FirstName + " " + User.LastName;
                }
            }
            return UserName;
        }

        public static string GetCurrencyName(int id, GarasTestContext _Context)
        {
            string name = "";

            var ID = new SqlParameter("ID", System.Data.SqlDbType.Int);
            ID.Value = id;


            object[] param = new object[] { ID };


            var CurrencyLoadObjDB = _Context.Database.SqlQueryRaw<proc_CurrencyLoadByPrimaryKey_Result>("Exec proc_CurrencyLoadByPrimaryKey @ID", param).AsEnumerable().FirstOrDefault();


            if (CurrencyLoadObjDB != null)
            {
                name = CurrencyLoadObjDB.Name;
            }
            return name;
        }

        public static string GetParentCategory(long id, GarasTestContext _Context)
        {
            string name = "";
            var ID = new SqlParameter("ID", System.Data.SqlDbType.BigInt);
            ID.Value = id;


            object[] param = new object[] { ID };


            var AccountLoadObjDB = _Context.Database.SqlQueryRaw<proc_AccountsLoadByPrimaryKey_Result>("Exec proc_AccountsLoadByPrimaryKey @ID", param).AsEnumerable().FirstOrDefault();


            if (AccountLoadObjDB != null)
            {
                if (AccountLoadObjDB.ParentCategory != null && AccountLoadObjDB.ParentCategory != 0)
                {
                    name = GetParentCategory((long)AccountLoadObjDB.ParentCategory, _Context);
                }
                else
                {
                    name = AccountLoadObjDB.AccountName;
                }
            }
            return name;
        }

        public static string GetAccountName(long id, GarasTestContext _Context)
        {
            string name = "";

            var ID = new SqlParameter("ID", System.Data.SqlDbType.BigInt);
            ID.Value = id;


            object[] param = new object[] { ID };


            var AccountLoadObjDB = _Context.Database.SqlQueryRaw<proc_AccountsLoadByPrimaryKey_Result>("Exec proc_AccountsLoadByPrimaryKey @ID", param).AsEnumerable().FirstOrDefault();


            if (AccountLoadObjDB != null)
            {
                name = AccountLoadObjDB.AccountName;
            }
            return name;
        }

        public static string GetAccountBalanceType(long id, GarasTestContext _Context)
        {
            string name = "";
            var ID = new SqlParameter("ID", System.Data.SqlDbType.BigInt);
            ID.Value = id;


            object[] param = new object[] { ID };


            var LoadObjDB = _Context.Database.SqlQueryRaw<proc_AccountsLoadByPrimaryKey_Result>("Exec proc_AccountsLoadByPrimaryKey @ID", param).AsEnumerable().FirstOrDefault();


            if (LoadObjDB != null)
            {

                if (LoadObjDB.AccountTypeName == "Debit")
                {
                    name = "D";
                }
                else
                {
                    name = "C";
                }
            }
            return name;
        }

        public static string GetMonthName(int month)
        {
            DateTime date = new DateTime(2020, month, 1);
            return date.ToString("MMMM");
        }

        public static string GetClientName(long ClientID, GarasTestContext _Context)
        {
            string ClientName = "";
            if (ClientID != 0)
            {
                var ID = new SqlParameter("ID", System.Data.SqlDbType.BigInt);
                ID.Value = ClientID;


                object[] param = new object[] { ID };


                var ClientObjDB = _Context.Database.SqlQueryRaw<proc_ClientLoadByPrimaryKey_Result>("Exec proc_ClientLoadByPrimaryKey @ID", param).AsEnumerable().FirstOrDefault();


                if (ClientObjDB != null)
                {
                    ClientName = ClientObjDB.Name;
                }
            }
            return ClientName;
        }

        public static decimal GetTotalSalesForceClientReportAmount(GarasTestContext _Context)
        {
            decimal totalValue = 0;
            // DateTime StartYear = new DateTime(DateTime.Now.Year, 1, 1);
            var ProjectSalesOfferListDB = _Context.VProjectSalesOfferClients.Where(a => a.OfferType != "New Internal Order" && a.SalesOfferStatus == "Closed" && a.ProjectCreationDate >= StartYear && a.ProjectCreationDate < DateTime.Now && a.ProjectActive == true).ToList();
            //var ProjectSalesOfferListDB = _Context.V_Project_SalesOffer.Where(x => x.Active == true && x.CreationDate >= StartYear && x.CreationDate < DateTime.Now).ToList();
            decimal FinalOfferPrice = ProjectSalesOfferListDB.Where(x => x.FinalOfferPrice != null).Select(x => (decimal)x.FinalOfferPrice).Sum();
            decimal ExtraCost = ProjectSalesOfferListDB.Where(x => x.ProjectExtraCost != null).Select(x => (decimal)x.ProjectExtraCost).Sum();
            totalValue = FinalOfferPrice + ExtraCost;
            return totalValue;
        }
        public static decimal GetTotalSalesOfferProjectFinalPriceAmount(GarasTestContext _Context)
        {
            decimal totalValue = 0;

            var ProjectSalesOfferListDB = _Context.VProjectSalesOfferClients.Where(a => a.OfferType != "New Internal Order" && a.SalesOfferStatus == "Closed" && a.ProjectCreationDate >= StartYear && a.ProjectCreationDate < DateTime.Now && a.ProjectActive == true).ToList();
            //var ProjectSalesOfferListDB = _Context.V_Project_SalesOffer.Where(x => x.Active == true && x.CreationDate >= StartYear && x.CreationDate < DateTime.Now).ToList();
            decimal FinalOfferPrice = ProjectSalesOfferListDB.Where(x => x.FinalOfferPrice != null).Select(x => (decimal)x.FinalOfferPrice).Sum();
            //decimal ExtraCost = ProjectSalesOfferListDB.Where(x => x.ProjectExtraCost != null).Select(x => (decimal)x.ProjectExtraCost).Sum();
            totalValue = FinalOfferPrice;
            return totalValue;
        }

        public static decimal GetTotalSalesOfferProjectExtraCostsAmount(GarasTestContext _Context)
        {
            decimal totalValue = 0;
            //DateTime StartYear = new DateTime(DateTime.Now.Year, 1, 1);
            var ProjectSalesOfferListDB = _Context.VProjectSalesOfferClients.Where(a => a.OfferType != "New Internal Order" && a.SalesOfferStatus == "Closed" && a.ProjectCreationDate >= StartYear && a.ProjectCreationDate < DateTime.Now && a.ProjectActive == true).ToList();
            //var ProjectSalesOfferListDB = _Context.V_Project_SalesOffer.Where(x => x.Active == true && x.CreationDate >= StartYear && x.CreationDate < DateTime.Now).ToList();
            //decimal FinalOfferPrice = ProjectSalesOfferListDB.Where(x => x.FinalOfferPrice != null).Select(x => (decimal)x.FinalOfferPrice).Sum();
            decimal ExtraCost = ProjectSalesOfferListDB.Where(x => x.ProjectExtraCost != null).Select(x => (decimal)x.ProjectExtraCost).Sum();
            totalValue = ExtraCost;
            return totalValue;
        }

        public static decimal GetTotalSalesOfferProjectAmountForOffterTypeInternal(GarasTestContext _Context)
        {
            decimal totalValue = 0;
            // DateTime StartYear = new DateTime(DateTime.Now.Year, 1, 1);
            var ProjectSalesOfferListDB = _Context.VProjectSalesOfferClients.Where(a => a.OfferType == "New Internal Order" && a.SalesOfferStatus == "Closed" && a.ProjectCreationDate >= StartYear && a.ProjectCreationDate < DateTime.Now && a.ProjectActive == true).ToList();
            //var ProjectSalesOfferListDB = _Context.V_Project_SalesOffer.Where(x => x.Active == true && x.CreationDate >= StartYear && x.CreationDate < DateTime.Now).ToList();
            decimal FinalOfferPrice = ProjectSalesOfferListDB.Where(x => x.FinalOfferPrice != null).Select(x => (decimal)x.FinalOfferPrice).Sum();
            //decimal ExtraCost = ProjectSalesOfferListDB.Where(x => x.ProjectExtraCost != null).Select(x => (decimal)x.ProjectExtraCost).Sum();
            totalValue = FinalOfferPrice;
            return totalValue;
        }

        public static decimal GetTotalProjectsCollected2021WithOutInternal(GarasTestContext _Context)
        {
            decimal collected = 0;
            //DateTime StartYear = new DateTime(DateTime.Now.Year, 1, 1);
            var ProjectSalesOfferListDB = _Context.VProjectSalesOfferClients.Where(a => a.OfferType != "New Internal Order" && a.SalesOfferStatus == "Closed" && a.ProjectCreationDate >= StartYear && a.ProjectCreationDate < DateTime.Now && a.ProjectActive == true).ToList();
            //var ProjectSalesOfferListDB = _Context.V_Project_SalesOffer_Client.Where(x => x.ProjectActive == true && x.ProjectCreationDate >= StartYear && x.ProjectCreationDate < DateTime.Now).ToList();

            if (ProjectSalesOfferListDB.Count > 0)
            {
                foreach (var ProjectDetailsDataOBJ in ProjectSalesOfferListDB)
                {
                    var clientAccounts = _Context.ClientAccounts.Where(x => x.Active == true && x.ProjectId == ProjectDetailsDataOBJ.ProjectId && x.ClientId == ProjectDetailsDataOBJ.ClientId
                                                                          && x.CreationDate >= StartYear && x.CreationDate <= DateTime.Now).ToList();
                    if (clientAccounts.Count() > 0)
                    {
                        foreach (var ClientAccOBJ in clientAccounts)
                        {
                            if (ClientAccOBJ.AmountSign == "plus")
                            {
                                collected = collected + ClientAccOBJ.Amount;
                            }
                            else if (ClientAccOBJ.AmountSign == "minus")
                            {
                                collected = collected - ClientAccOBJ.Amount;
                            }
                        }
                    }
                }


            }


            return collected;
        }

        public static string GetSupplierName(long ID, GarasTestContext _Context)
        {
            string Name = "";

            if (ID != 0)
            {
                var SupID = new SqlParameter("ID", System.Data.SqlDbType.BigInt);
                SupID.Value = ID;


                object[] param = new object[] { SupID };


                var supplierDB = _Context.Database.SqlQueryRaw<proc_SupplierLoadByPrimaryKey_Result>("Exec proc_SupplierLoadByPrimaryKey @ID", param).AsEnumerable().FirstOrDefault();


                if (supplierDB != null)
                {
                    Name = supplierDB.Name;
                }
            }
            return Name;
        }

        public static decimal GetTotalPurchasingAndSupplierReportAmount(GarasTestContext _Context)
        {
            decimal totalValue = 0;
            //DateTime StartYear = new DateTime(DateTime.Now.Year, 1, 1);
            var SupplierPurchasePOListDB = _Context.VPurchasePos.Where(x => x.Active == true && x.CreationDate >= StartYear && x.CreationDate < DateTime.Now).ToList();
            totalValue = SupplierPurchasePOListDB.Where(x => x.TotalInvoiceCost != null).Select(x => (decimal)x.TotalInvoiceCost).Sum();
            return totalValue;
        }
        public static string string_compare_prepare_function(string str_sent)
        {
            try
            {
                if (str_sent.Length > 0)
                {
                    str_sent = str_sent.Trim();
                    str_sent = str_sent.Replace("ة", "ه");
                    str_sent = str_sent.Replace("أ", "ا");
                    str_sent = str_sent.Replace("إ", "ا");
                    str_sent = str_sent.Replace("ي", "ى");
                    str_sent = str_sent.Replace("ئ", "ء");
                    str_sent = " " + str_sent + " ";
                    str_sent = str_sent.Replace(" ال", " ");
                    str_sent = str_sent.ToUpper();
                    str_sent = str_sent.Trim();
                }
                return str_sent;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string GetGroupName(long id, GarasTestContext _Context)
        {
            string name = "";
            var ID = new SqlParameter("ID", System.Data.SqlDbType.BigInt);
            ID.Value = id;


            object[] param = new object[] { ID };


            var LoadObjDB = _Context.Database.SqlQueryRaw<proc_GroupLoadByPrimaryKey_Result>("Exec proc_GroupLoadByPrimaryKey @ID", param).AsEnumerable().FirstOrDefault();



            if (LoadObjDB != null)
            {
                name = LoadObjDB.Name;
            }
            return name;
        }

        public static bool CheckInventoryItemHaveOpeningBalance(long InventoryItemID, GarasTestContext _Context)
        {
            bool HaveOpeningBalance = false;
            var CheckAddingOrderOpeningBalance = _Context.VInventoryAddingOrderItems.Where(x => x.InventoryItemId == InventoryItemID && x.OperationType == "Opening Balance").FirstOrDefault();
            if (CheckAddingOrderOpeningBalance != null)
            {
                HaveOpeningBalance = true;
            }
            return HaveOpeningBalance;
        }


        //public static Tuple<int?, string?> GetHeadParentCategory(List<InventoryItemCategory> InventoryItemCategoryList, int? CategoryId)
        //{

        //    int? HeadParentCategoryId = null;
        //    string HeadParentCategoryName = null;
        //    var ParentCategoryObj = InventoryItemCategoryList.Where(x => x.Id == CategoryId).FirstOrDefault();
        //    if (ParentCategoryObj != null)
        //    {

        //        if (ParentCategoryObj.ParentCategoryId == null)
        //        {
        //            HeadParentCategoryId = CategoryId;
        //            HeadParentCategoryName = ParentCategoryObj.Name;
        //        }
        //        else
        //        {
        //             GetHeadParentCategory(InventoryItemCategoryList, ParentCategoryObj.ParentCategoryId);
        //        }
        //    }
        //    return Tuple.Create(HeadParentCategoryId, HeadParentCategoryName);
        //}


        public static Tuple<int?, string?> GetHeadParentCategory(List<InventoryItemCategory> inventoryItemCategoryList, int? categoryId)
        {
            if (categoryId == null)
                return Tuple.Create<int?, string?>(null, null);

            var category = inventoryItemCategoryList.FirstOrDefault(x => x.Id == categoryId);

            if (category == null)
                return Tuple.Create<int?, string?>(null, null);

            // If there's no parent, this is the head category
            if (category.ParentCategoryId == null)
                return Tuple.Create(category?.Id, category?.Name);

            // Recursively find the head parent category
            return GetHeadParentCategory(inventoryItemCategoryList, category.ParentCategoryId);
        }

        public static List<int> GetCategoryIdsIncludingChildren(int parentCategoryId, GarasTestContext _Context)
        {
            var categoryIds = new List<int> { parentCategoryId };

            // Find all child categories
            var children = _Context.InventoryItemCategories
                .Where(c => c.ParentCategoryId == parentCategoryId)
                .ToList();

            foreach (var child in children)
            {
                categoryIds.AddRange(GetCategoryIdsIncludingChildren(child.Id,_Context));
            }

            return categoryIds;
        }

        public static string GetBranchName(int id, GarasTestContext _Context)
        {
            var ID = new SqlParameter("ID", System.Data.SqlDbType.BigInt);
            ID.Value = id;


            object[] param = new object[] { ID };
            string name = "";
            var LoadObjDB = _Context.Database.SqlQueryRaw<proc_BranchLoadByPrimaryKey_Result>("Exec proc_BranchLoadByPrimaryKey @ID", param).AsEnumerable().FirstOrDefault();
            if (LoadObjDB != null)
            {
                name = LoadObjDB.Name;
            }
            return name;
        }

        public static string GetDepartmentName(int id, GarasTestContext _Context)
        {
            var ID = new SqlParameter("ID", System.Data.SqlDbType.BigInt);
            ID.Value = id;


            object[] param = new object[] { ID };
            string name = "";
            var LoadObjDB = _Context.Database.SqlQueryRaw<proc_DepartmentLoadByPrimaryKey_Result>("Exec proc_DepartmentLoadByPrimaryKey @ID", param).AsEnumerable().FirstOrDefault();
            if (LoadObjDB != null)
            {
                name = LoadObjDB.Name;
            }
            return name;
        }


        public static string GetJobTitleName(int id, GarasTestContext _Context)
        {
            var ID = new SqlParameter("ID", System.Data.SqlDbType.BigInt);
            ID.Value = id;


            object[] param = new object[] { ID };
            string name = "";
            var LoadObjDB = _Context.Database.SqlQueryRaw<proc_JobTitleLoadByPrimaryKey_Result>("Exec proc_JobTitleLoadByPrimaryKey @ID", param).AsEnumerable().FirstOrDefault();
            if (LoadObjDB != null)
            {
                name = LoadObjDB.Name;
            }
            return name;
        }
        public static string GetUserJobTitleName(long UserId, GarasTestContext _Context)
        {
            string JobtitleName = "";

            if (UserId != null)
            {
                var user = _Context.Users.Find(UserId);
                if (user != null)
                {
                    var jobtitle = _Context.JobTitles.Find(user.JobTitleId);
                    if (jobtitle != null)
                    {
                        JobtitleName = jobtitle.Name;
                    }
                }
            }
            return JobtitleName;
        }
        public static bool CheckInventoryItemHavePRorPO(long InventoryItemID, GarasTestContext _Context)
        {
            bool HavePRorPO = false;
            var V_PR = _Context.VPurchaseRequestItems.Where(x => x.InventoryItemId == InventoryItemID).FirstOrDefault();
            var V_PO = _Context.VPurchasePoItems.Where(x => x.InventoryItemId == InventoryItemID).FirstOrDefault();
            if (V_PR != null && V_PO != null)
            {
                HavePRorPO = true;
            }
            return HavePRorPO;
        }

        public static string SaveFileIFF(string PathAddress, IFormFile FileContent, string FileName, string FileExtension, IWebHostEnvironment host)
        {
            string path = Path.Combine(host.WebRootPath, PathAddress);

            //Check if directory exist
            string directory = Path.GetDirectoryName(PathAddress);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path); //Create directory if it doesn't exist
            }

            string FullFileName = DateTime.Now.ToFileTime() + "_" + FileName.Trim().Replace(" ", "") + "." + FileExtension;
            string FilePath = Path.Combine(path, FullFileName);

            using FileStream fileStream = new(FilePath, FileMode.Create);

            FileContent.CopyTo(fileStream);

            //File.WriteAllBytes(FilePath, FileBytes);
            var DbSavingPath = PathAddress + FullFileName;
            return DbSavingPath;

        }
        public static (int Width, int Height) GetImageResolution(string imageUrl)
        {
            try
            {
                // Download the image from the URL
                using (WebClient webClient = new WebClient())
                {
                    byte[] imageData = webClient.DownloadData(imageUrl);
                    using (var stream = new System.IO.MemoryStream(imageData))
                    {
                        using (Image image = Image.FromStream(stream))
                        {
                            return (image.Width, image.Height);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., invalid URL, network issues, etc.)
                Console.WriteLine("Error: " + ex.Message);
                return (0, 0); // Return 0,0 if there's an error
            }
        }

        public static string SaveFile(string Root, string SubDir, byte[] FileContent, string FileName, string FileExtension, IWebHostEnvironment host)
        {
            var PathAddress = Root + SubDir;
            string path = Path.Combine(host.WebRootPath, PathAddress); //Path

            //Check if directory exist
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path); //Create directory if it doesn't exist
            }

            string FullFileName = DateTime.Now.ToFileTime() + "_" + FileName + "." + FileExtension;

            //set the image path
            string FilePath = Path.Combine(path, FullFileName);

            byte[] FileBytes = FileContent;

            File.WriteAllBytes(FilePath, FileBytes);
            System.Diagnostics.Process.Start(FilePath);

            var DbSavingPath = PathAddress + "/" + FullFileName;
            return DbSavingPath;
        }

        public static string SaveFile(string PathAddress, string FileContent, string FileName, string FileExtension, IWebHostEnvironment host)
        {

            string path = Path.Combine(host.WebRootPath, PathAddress); //Path

            //Check if directory exist
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory); //Create directory if it doesn't exist
            }

            string FullFileName = DateTime.Now.ToFileTime() + "_" + FileName.Trim().Replace(" ", "") + "." + FileExtension;

            //set the image path
            string FilePath = Path.Combine(path, FullFileName);

            byte[] FileBytes = Convert.FromBase64String(FileContent);

            File.WriteAllBytes(FilePath, FileBytes);
            var DbSavingPath = "/" + PathAddress + "/" + FullFileName;
            return DbSavingPath;
        }
        public static async Task<string> SaveFileAsync(string PathAddress, string FileContent, string FileName, string FileExtension, IWebHostEnvironment host)
        {

            string path = Path.Combine(host.WebRootPath, PathAddress); //Path

            //Check if directory exist
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory); //Create directory if it doesn't exist
            }

            string FullFileName = DateTime.Now.ToFileTime() + "_" + FileName.Trim().Replace(" ", "") + "." + FileExtension;

            //set the image path
            string FilePath = Path.Combine(path, FullFileName);

            byte[] FileBytes = Convert.FromBase64String(FileContent);

            File.WriteAllBytes(FilePath, FileBytes);
            var DbSavingPath = "/" + PathAddress + "/" + FullFileName;
            return DbSavingPath;
        }
        public static List<TreeViewCountr> BuildTreeViews(string pid, IList<TreeViewCountr> candicates)
        {
            var subs = candicates.Where(c => c.parentId == pid).ToList();
            if (subs.Count() == 0)
            {
                return new List<TreeViewCountr>();
            }
            foreach (var i in subs)
            {
                i.subs = BuildTreeViews(i.id, candicates);
            }
            return subs;
        }

        public static List<TreeViewDto> BuildTreeViews(string pid, IList<TreeViewDto> candicates)
        {
            var subs = candicates.Where(c => c.parentId == pid).ToList();
            if (subs.Count() == 0)
            {
                return new List<TreeViewDto>();
            }
            foreach (var i in subs)
            {
                i.subs = BuildTreeViews(i.id, candicates);
            }
            return subs;
        }

        public static List<TreeViewAccount> BuildTreeViews(string pid, IList<TreeViewAccount> candicates)
        {
            var subs = candicates.Where(c => c.parentId == pid).ToList();
            if (subs.Count() == 0)
            {
                return new List<TreeViewAccount>();
            }
            foreach (var i in subs)
            {
                i.subs = BuildTreeViews(i.id, candicates);
            }
            return subs;
        }

        public static string GetAccountCategoryName(long id, GarasTestContext _Context)
        {
            string name = "";
            var AccountCategoryObjDB = _Context.AccountCategories.Find(id);
            if (AccountCategoryObjDB != null)
            {
                name = AccountCategoryObjDB.AccountCategoryName;
            }
            return name;
        }
        public static int LoadParentCategory(int ParentCategoryID, GarasTestContext _Context)
        {
            var inventoryItemCategory = _Context.InventoryItemCategories.Where(a => a.Id == ParentCategoryID).FirstOrDefault();
            if (inventoryItemCategory != null)
            {
                if (inventoryItemCategory.ParentCategoryId != null && inventoryItemCategory.ParentCategoryId != ParentCategoryID)
                {
                    ParentCategoryID = LoadParentCategory((int)inventoryItemCategory.ParentCategoryId, _Context);
                }
            }
            return ParentCategoryID;
        }



        public static int GetParentCategory(int ParentCategoryID, List<InventoryItemCategory> InventoryItemCategoryList)
        {
            var inventoryItemCategory = InventoryItemCategoryList.Where(a => a.Id == ParentCategoryID).FirstOrDefault();
            if (inventoryItemCategory != null)
            {
                if (inventoryItemCategory.ParentCategoryId != null && inventoryItemCategory.ParentCategoryId != ParentCategoryID)
                {
                    ParentCategoryID = GetParentCategory((int)inventoryItemCategory.ParentCategoryId, InventoryItemCategoryList);
                }
            }
            return ParentCategoryID;
        }

        /*public static string GetInventoryItemCategory(int id, GarasTestContext _Context)
        {
            string name = "";
            var LoadObjDB = _Context.InventoryItemCategories.Where(a => a.Id == id).FirstOrDefault();
            if (LoadObjDB != null)
            {
                name = LoadObjDB.Name;
            }
            return name;
        }*/

        public static string GetProjectName(long id, GarasTestContext _Context)
        {
            string name = "";
            var LoadObjDB = _Context.VProjectSalesOffers.Where(x => x.Id == id).FirstOrDefault();
            if (LoadObjDB != null)
            {
                name = LoadObjDB.ProjectName;
            }
            return name;
        }

        public static string GetProjectSerial(long id, GarasTestContext _Context)
        {
            string ProjSerial = "";
            if (id != 0)
            {
                var ProjDB = _Context.Projects.Where(a => a.Id == id).FirstOrDefault();
                if (ProjDB != null)
                {
                    if (ProjDB.ProjectSerial != null)
                    {
                        ProjSerial = ProjDB.ProjectSerial;
                    }
                }
            }
            return ProjSerial;
        }


        //public static string GetClientName(long ClientID, GarasTestContext _Context)
        //{
        //    string ClientName = "";
        //    if (ClientID != 0)
        //    {
        //        var ClientObjDB = _Context.Clients.Where(a => a.Id == ClientID).FirstOrDefault();
        //        if (ClientObjDB != null)
        //        {
        //            ClientName = ClientObjDB.Name;
        //        }
        //    }
        //    return ClientName;
        //}

        //public static Byte[] BitmapToBytesCode(Bitmap image)
        //{
        //    using (MemoryStream stream = new MemoryStream())
        //    {
        //        image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
        //        return stream.ToArray();
        //    }
        //}

        public static async Task<bool> CheckFinancialPeriodIsAvaliable(DateTime Date, GarasTestContext _Context)
        {
            bool IsAvaliable = false;

            var FinancialPeriodListDB = await _Context.AccountFinancialPeriods.ToListAsync();



            if (FinancialPeriodListDB.Count() > 0)
            {
                var FinancialPeriodObj = FinancialPeriodListDB.Where(x => x.StartDate <= Date &&
                                                                                    (x.EndDate != null ? x.EndDate >= Date : true)
                                                                                    && x.IsCurrent == true).FirstOrDefault();
                if (FinancialPeriodObj != null)
                {
                    IsAvaliable = true;
                }
            }
            else
            {
                IsAvaliable = true;
            }
            return IsAvaliable;
        }
        public static string GetBeneficiaryTypeName(long id, GarasTestContext _Context)
        {
            string name = "";
            var ObjDB = _Context.DailyTranactionBeneficiaryToTypes.Find(id);
            if (ObjDB != null)
            {
                name = ObjDB.BeneficiaryName;
            }
            return name;
        }
        public static string GetBeneficiaryGeneralName(long id, GarasTestContext _Context)
        {
            string name = "";
            var ObjDB = _Context.DailyTranactionBeneficiaryToGeneralTypes.Find(id);
            if (ObjDB != null)
            {
                name = ObjDB.Name;
            }
            return name;
        }
        public static async Task<bool> CheckUserRoleAsync(long UserId, int RoleID, GarasTestContext _Context)
        {
            bool Res = false;
            var LoadObjDB = await _Context.UserRoles.Where(x => x.UserId == UserId && x.RoleId == RoleID).FirstOrDefaultAsync();
            if (LoadObjDB != null)
            {
                Res = true;
            }
            return Res;
        }
        public static string GetPaymentMethodType(long? id, GarasTestContext _Context)
        {
            string name = "";
            if (id != null)
            {
                var ObjDB = _Context.PaymentMethods.Find(id);
                if (ObjDB != null)
                {
                    name = ObjDB.Name;
                }
            }
            return name;
        }

        public static Tuple<long?, long?> GetOfferIDFromEntryID(long entryId, GarasTestContext _Context)
        {
            long? OfferId = null;
            long? ProjectId = null;
            if (entryId != 0)
            {
                // Project From Entry
                ProjectId = _Context.ClientAccounts.Where(x => x.DailyAdjustingEntryId == entryId).Select(x => x.ProjectId).FirstOrDefault();
                if (ProjectId != null)
                {
                    // Offer from project 
                    OfferId = _Context.Projects.Where(x => x.Id == ProjectId).Select(x => x.SalesOfferId).FirstOrDefault();
                }
            }
            return Tuple.Create(ProjectId, OfferId);
        }

        public static List<AccountOfMovement> GetAccountMovementList_WithListAccountIds(string AccountIdSTRr, bool CalcWithoutPrivate, bool OrderByCreationDatee, DateTime? DateFrom, DateTime? DateTo, long ClientIdd, long SupplierIdd, GarasTestContext _Context)
        {
            DateTime CreationDateFromm = StartYear;
            DateTime CreationDateToo = CurrentEndYear;

            if (DateFrom != null)
            {
                CreationDateFromm = (DateTime)DateFrom;
            }
            if (DateTo != null)
            {
                CreationDateToo = (DateTime)DateTo;
            }

            DateTime AccountBegine = new DateTime(2000, 1, 1);
            // Default From and To Current Year
            var AccountOfMovementList = new List<AccountOfMovement>();
            //var AccountsMovementListDB = _Context.STP_AccountMovement(AccountId, CalcWithoutPrivate, OrderByCreationDate, AccountBegine, CreationDateTo).ToList();
            //var IDSSTR = String.Join(",", AccountIds.Select(x => x.ToString()).ToArray());
            List<long> AccountIds = AccountIdSTRr.Split(',').Select(long.Parse).ToList();
            var AccountIDSTR = new SqlParameter("AccountIDSTR", System.Data.SqlDbType.NVarChar);
            AccountIDSTR.Value = AccountIdSTRr;
            var CalcWithoutPrivateJE = new SqlParameter("CalcWithoutPrivateJE", System.Data.SqlDbType.Bit);
            CalcWithoutPrivateJE.Value = CalcWithoutPrivate;
            var OrderByCreationDate = new SqlParameter("OrderByCreationDate", System.Data.SqlDbType.Bit);
            OrderByCreationDate.Value = OrderByCreationDatee;
            var CreationDateFrom = new SqlParameter("CreationDateFrom", System.Data.SqlDbType.DateTime);
            CreationDateFrom.Value = CreationDateFromm;
            var CreationDateTo = new SqlParameter("CreationDateTo ", System.Data.SqlDbType.DateTime);
            CreationDateTo.Value = CreationDateToo;
            var ClientId = new SqlParameter("ClientId ", System.Data.SqlDbType.BigInt);
            ClientId.Value = ClientIdd;
            var SupplierId = new SqlParameter("SupplierId ", System.Data.SqlDbType.BigInt);
            SupplierId.Value = SupplierIdd;
            var BranchId = new SqlParameter("BranchID", System.Data.SqlDbType.BigInt);
            BranchId.Value = 0;
            object[] param = new object[] { AccountIDSTR, CalcWithoutPrivateJE, OrderByCreationDate, CreationDateFrom, CreationDateTo, ClientId, SupplierId, BranchId };
            var AccountsMovementListDB = _Context.Database.SqlQueryRaw<STP_AccountMovement_WithMultiAccountsWithOtherCU_Result>("Exec STP_AccountMovement_WithMultiAccountsWithOtherCU @AccountIDSTR ,@CalcWithoutPrivateJE ,@OrderByCreationDate ,@CreationDateFrom ,@CreationDateTo, @ClientId, @SupplierId ,@BranchID", param).AsEnumerable().ToList();
            /*var AccountsMovementListDB = _Context.STP_AccountMovement_WithMultiAccountsWithOtherCU(AccountIdSTR, CalcWithoutPrivate, OrderByCreationDate, AccountBegine, CreationDateTo, ClientId, SupplierId).ToList();*/

            if (OrderByCreationDatee)
            {
                AccountsMovementListDB = AccountsMovementListDB.Where(x => x.CreationDate >= CreationDateFromm && x.CreationDate <= CreationDateToo).ToList();
            }
            else
            {
                AccountsMovementListDB = AccountsMovementListDB.Where(x => x.EntryDate >= CreationDateFromm && x.EntryDate <= CreationDateToo).ToList();
            }
            if (AccountsMovementListDB.Count() > 0)
            {
                var IDSDailyJournalEntry = AccountsMovementListDB.Where(x => AccountIds.Contains(x.ID)).Select(x => x.EntryID).ToList();


                //foreach (var item in AccountsMovementListDB)
                //{
                //    var Obj = new AccountOfMovement();
                //    Obj.CreationDate = item.CreationDate.ToShortDateString();
                //    Obj.EntryDate = item.EntryDate.ToShortDateString();
                //    Obj.Credit = item.Credit;
                //    Obj.Debit = item.Debit;
                //    Obj.Accumulative = item.Acc_Calc;
                //    Obj.FromOrTo = item.FromOrTo;
                //    Obj.DailyJournalId = item.EntryID;
                //    // Get List
                //    var ListVM = new List<AccountOfMovement>();
                //    var ListAccount = _Context.V_AccountOfJournalEntryWithDaily.Where(x => x.EntryID == item.EntryID && x.FromOrTo != item.FromOrTo).ToList();
                //    foreach (var itemAcc in ListAccount)
                //    {
                //        ListVM.Add(new AccountOfMovement
                //        {
                //            AccountName = itemAcc.AccountName,
                //            Credit = itemAcc.Credit,
                //            Debit = itemAcc.Debit,
                //            Accumulative = itemAcc.Amount,
                //            FromOrTo = itemAcc.FromOrTo,
                //        });
                //    }
                //    Obj.ListVM = ListVM;
                //    AccountOfMovementList.Add(Obj);
                //}
                var AccountOfJournalEntryList = _Context.VAccountOfJournalEntryWithDailies.Where(a => IDSDailyJournalEntry.Contains(a.EntryId) && a.Active == true).ToList();
                //AccountOfMovementList = AccountsMovementListDB.Where(x => IDSDailyJournalEntry.Contains(x.EntryID))
                //    .Select(x => new AccountOfMovement
                //    {
                //        AccountID = x.ID,
                //        CreationDate = x.CreationDate.ToShortDateString(),
                //        EntryDate = x.EntryDate.ToShortDateString(),
                //        AccountName = x.AccountName,
                //        Serial = x.Serial,
                //        AccountCode = x.AccountNumber,
                //        AccountCategory = x.AccountCategoryName,
                //        AccountType = x.AccountTypeName,
                //        Currency = x.CurrencyName,
                //        Credit = x.Credit,
                //        Debit = x.Debit,
                //        Accumulative = x.Acc_Calc,
                //        DailyJournalId = x.EntryID,
                //        Description = x.Description,
                //        Document = x.DocumentNumber,
                //        CreatedBy = x.DailyCreatedFirstName + " " + x.DailyCreatedLastName,
                //        MethodName = x.MethodName,
                //        FromOrTo = x.FromOrTo,
                //        ReleatedAccount = AccountOfJournalEntryList.Where(a => a.EntryID == x.EntryID && a.FromOrTo != x.FromOrTo).Count() == 1 ?
                //                          AccountOfJournalEntryList.Where(a => a.EntryID == x.EntryID && a.FromOrTo != x.FromOrTo).FirstOrDefault().AccountName
                //                         :
                //                          AccountOfJournalEntryList.Where(a => a.EntryID == x.EntryID && a.FromOrTo != x.FromOrTo).Count() + " Accounts",
                //        ReleatedAccountList = AccountOfJournalEntryList.Where(a => a.EntryID == x.EntryID && a.FromOrTo != x.FromOrTo).Select(a=>a.AccountName).ToList()

                //    }).ToList();







                //if (AccountsMovementListDB.Count() > 0)
                //{

                //    var AccountOfMovementListSP = AccountsMovementListDB.Select(item=> new AccountOfMovement {

                //        AccountID = item.ID,
                //    CreationDate = item.CreationDate.ToShortDateString(),
                //    EntryDate = item.EntryDate.ToShortDateString(),
                //    AccountName = item.AccountName,
                //    Serial = item.Serial,
                //    AccountCode = item.AccountNumber,
                //    AccountCategory = item.AccountCategoryName,
                //    AccountType = item.AccountTypeName,
                //    Currency = item.CurrencyName,
                //    Credit = item.Credit,
                //    Debit = item.Debit,
                //    Accumulative = item.Acc_Calc,
                //    DailyJournalId = item.EntryID,
                //    Description = item.Description,
                //    Document = item.DocumentNumber,
                //    CreatedBy = item.DailyCreatedFirstName + " " + item.DailyCreatedLastName,
                //    MethodName = item.MethodName,
                //    FromOrTo = item.FromOrTo,
                //    AccountEntryComment = item.EntryAccountComment,





                //}).tolist();
                if (AccountsMovementListDB.Count() > 0)
                {



                    var IDSAccountOfJournalEntry = AccountsMovementListDB.Where(x => x.aofjeID != null).Select(x => x.aofjeID).Distinct().ToList();
                    var SupplierAccountsList = _Context.SupplierAccounts.Where(x => IDSAccountOfJournalEntry.Contains((long)x.AccountOfJeid)).ToList();
                    var ClientAccountsList = _Context.ClientAccounts.Include(a=>a.Client).Where(x => IDSAccountOfJournalEntry.Contains((long)x.AccountOfJeid)).ToList();
                    var AccountOfMovementListdb = AccountsMovementListDB.Select((item, index) =>
                    {
                        bool IsCredit = item.Credit != 0;
                        var CountOfReleatedAccount = AccountOfJournalEntryList.Where(a => a.EntryId == item.EntryID && (IsCredit ? a.Debit != 0 : a.Credit != 0)).Count();
                        string ReleatedAccount = "";
                        if (CountOfReleatedAccount == 1)
                        {
                            ReleatedAccount = AccountOfJournalEntryList.Where(a => a.EntryId == item.EntryID && (IsCredit ? a.Debit != 0 : a.Credit != 0)).FirstOrDefault().AccountName;
                        }
                        else
                        {
                            ReleatedAccount = CountOfReleatedAccount + " Accounts";
                        }

                        long? SupplierID = null;
                        string SupplierName = null;
                        long? POID = null;
                        string AccountDescription = null;
                        long? ClientID = null;
                        long? ProjectID = null;
                        string ProjectName = null;
                        string ClientName = null;

                        var SupplierAccountDb = SupplierAccountsList.Where(x => x.AccountOfJeid == item.aofjeID).FirstOrDefault();
                        if (SupplierAccountDb != null)
                        {
                            SupplierID = SupplierAccountDb.SupplierId;
                            SupplierName = SupplierAccountDb.Supplier != null ? SupplierAccountDb.Supplier.Name : "";
                            POID = SupplierAccountDb.PurchasePoid;
                            AccountDescription = SupplierAccountDb.Description;
                        }
                        var ClientAccountDb = ClientAccountsList.Where(x => x.AccountOfJeid == item.aofjeID).FirstOrDefault();
                        if (ClientAccountDb != null)
                        {
                            ClientID = ClientAccountDb.ClientId;
                            ClientName = ClientAccountDb.Client != null ? ClientAccountDb.Client.Name : "";
                            ProjectID = ClientAccountDb.ProjectId;
                            AccountDescription = ClientAccountDb.Description;
                            ProjectName = ClientAccountDb.ProjectId != null ? GetProjectName((long)ClientAccountDb.ProjectId, _Context) : "";
                        }
                        return new AccountOfMovement
                        {

                            AccountID = item.ID,
                            CreationDate = item.CreationDate.ToShortDateString(),
                            EntryDate = item.EntryDate.ToShortDateString(),
                            AccountName = item.AccountName,
                            Serial = item.Serial,
                            AccountCode = item.AccountNumber,
                            AccountCategory = item.AccountCategoryName,
                            AccountType = item.AccountTypeName,
                            Currency = item.CurrencyName,
                            Credit = item.Credit,
                            Debit = item.Debit,
                            Accumulative = item.Acc_Calc,
                            DailyJournalId = item.EntryID,
                            Description = item.Description,
                            Document = item.DocumentNumber,
                            CreatedBy = item.DailyCreatedFirstName + " " + item.DailyCreatedLastName,
                            MethodName = item.MethodName,
                            FromOrTo = item.FromOrTo,
                            AccountEntryComment = item.EntryAccountComment,
                            ReleatedAccount = ReleatedAccount,
                            ReleatedAccountList = AccountOfJournalEntryList.Where(a => a.EntryId == item.EntryID && (IsCredit ? a.Debit != 0 : a.Credit != 0)).Select(a => a.AccountName).ToList(),
                            SupplierID = SupplierID,
                            SupplierName = SupplierName,
                            POID = POID,
                            AccountDescription = AccountDescription,
                            ClientID = ClientID,
                            ClientName = ClientName,
                            ProjectID = ProjectID,
                            ProjectName = ProjectName,
                            RateToLocalCU = item.RateToLocalCU,
                            CreditOtherCU = item.CreditOtherCU,
                            DebitOtherCU = item.DebitOtherCU,
                            ACCOtherCU = item.ACCOtherCU,
                            AmountOtherCU = item.AmountOtherCU,
                        };
                    }).ToList();







                    //foreach (var item in AccountsMovementListDB)
                    //{
                    //    var AccountOfMovement = new AccountOfMovement();
                    //    AccountOfMovement.AccountID = item.ID;
                    //    AccountOfMovement.CreationDate = item.CreationDate.ToShortDateString();
                    //    AccountOfMovement.EntryDate = item.EntryDate.ToShortDateString();
                    //    AccountOfMovement.AccountName = item.AccountName;
                    //    AccountOfMovement.Serial = item.Serial;
                    //    AccountOfMovement.AccountCode = item.AccountNumber;
                    //    AccountOfMovement.AccountCategory = item.AccountCategoryName;
                    //    AccountOfMovement.AccountType = item.AccountTypeName;
                    //    AccountOfMovement.Currency = item.CurrencyName;
                    //    AccountOfMovement.Credit = item.Credit;
                    //    AccountOfMovement.Debit = item.Debit;
                    //    AccountOfMovement.Accumulative = item.Acc_Calc;
                    //    AccountOfMovement.DailyJournalId = item.EntryID;
                    //    AccountOfMovement.Description = item.Description;
                    //    AccountOfMovement.Document = item.DocumentNumber;
                    //    AccountOfMovement.CreatedBy = item.DailyCreatedFirstName + " " + item.DailyCreatedLastName;
                    //    AccountOfMovement.MethodName = item.MethodName;
                    //    AccountOfMovement.FromOrTo = item.FromOrTo;
                    //    AccountOfMovement.AccountEntryComment = item.EntryAccountComment;


                    //    bool IsCredit = item.Credit != 0;
                    //    //var CountOfReleatedAccount = AccountOfJournalEntryList.Where(a => a.EntryID == item.EntryID && a.FromOrTo != item.FromOrTo).Count() ;
                    //    var CountOfReleatedAccount = AccountOfJournalEntryList.Where(a => a.EntryID == item.EntryID && (IsCredit ? a.Debit != 0 : a.Credit != 0)).Count();
                    //    if (CountOfReleatedAccount == 1)
                    //    {
                    //        AccountOfMovement.ReleatedAccount = AccountOfJournalEntryList.Where(a => a.EntryID == item.EntryID && (IsCredit ? a.Debit != 0 : a.Credit != 0)).FirstOrDefault().AccountName;
                    //    }
                    //    else
                    //    {
                    //        AccountOfMovement.ReleatedAccount = CountOfReleatedAccount + " Accounts";
                    //    }
                    //    //AccountOfMovement.ReleatedAccountList = AccountOfJournalEntryList.Where(a => a.EntryID == item.EntryID && a.FromOrTo != item.FromOrTo).Select(a => a.AccountName).ToList();
                    //    AccountOfMovement.ReleatedAccountList = AccountOfJournalEntryList.Where(a => a.EntryID == item.EntryID && (IsCredit ? a.Debit != 0 : a.Credit != 0)).Select(a => a.AccountName).ToList();
                    //    var SupplierAccountDb = _Context.SupplierAccounts.Where(x => x.AccountOfJEId == item.aofjeID).FirstOrDefault();
                    //    if (SupplierAccountDb != null)
                    //    {
                    //        AccountOfMovement.SupplierID = SupplierAccountDb.SupplierID;
                    //        AccountOfMovement.SupplierName = SupplierAccountDb.Supplier != null ? SupplierAccountDb.Supplier.Name : "";
                    //        AccountOfMovement.POID = SupplierAccountDb.PurchasePOID;
                    //        AccountOfMovement.AccountDescription = SupplierAccountDb.Description;
                    //    }
                    //    var ClientAccountDb = _Context.ClientAccounts.Where(x => x.AccountOfJEId == item.aofjeID).FirstOrDefault();
                    //    if (ClientAccountDb != null)
                    //    {
                    //        AccountOfMovement.ClientID = ClientAccountDb.ClientID;
                    //        AccountOfMovement.ClientName = ClientAccountDb.Client != null ? ClientAccountDb.Client.Name : "";
                    //        AccountOfMovement.ProjectID = ClientAccountDb.ProjectID;
                    //        AccountOfMovement.AccountDescription = ClientAccountDb.Description;
                    //        AccountOfMovement.ProjectName = ClientAccountDb.ProjectID != null ? Common.GetProjectName((long)ClientAccountDb.ProjectID) : "";
                    //    }
                    //    AccountOfMovementList.Add(AccountOfMovement);
                    //}

                    AccountOfMovementList = AccountOfMovementListdb;
                }

            }
            return AccountOfMovementList;
        }

        public static string CheckUserIsExist(long UserId, GarasTestContext _Context)
        {
            string Res = null;
            var LoadObjDB = _Context.Users.Where(x => x.Id == UserId).FirstOrDefault();
            if (LoadObjDB != null)
            {
                Res = LoadObjDB.FirstName + " " + LoadObjDB.LastName;
            }
            return Res;
        }

        public static decimal GetIncomeStatmentNetProfit(GarasTestContext _Context)
        {
            int CurrentYear = DateTime.Now.Year;
            decimal NetProfit = 0;
            //var AccountJournalEntryDB = _Context.proc_AccountOfJournalEntryLoadAll().Where(x => x.EntryDate.Year == CurrentYear  && x.Active == true).AsQueryable();
            var TotalAccountJournalEntryAmountIncome = _Context.AccountOfJournalEntries.Where(x => x.EntryDate.Year == CurrentYear && x.Active == true).Where(x => x.DtmainType == "Income").Sum(x => x.Amount);
            var TotalAccountJournalEntryAmountExpenses = _Context.AccountOfJournalEntries.Where(x => x.EntryDate.Year == CurrentYear && x.Active == true).Where(x => x.DtmainType == "Expenses").Sum(x => x.Amount);
            NetProfit = Math.Abs(TotalAccountJournalEntryAmountIncome) - Math.Abs(TotalAccountJournalEntryAmountExpenses);
            return NetProfit;
        }

        public static decimal GetTotalAmountInventoryItem(long InventoryStoreID, GarasTestContext _Context)
        {
            decimal totalValue = 0;
            var InventoryStoreItemQuerableDB = _Context.VInventoryStoreItemPrices.Where(x => x.Active == true && x.StoreActive == true).AsQueryable();
            if (InventoryStoreID != 0)
            {
                InventoryStoreItemQuerableDB = InventoryStoreItemQuerableDB.Where(x => x.InventoryStoreId == InventoryStoreID).AsQueryable();
            }
            //var InventoryStoreItemListDB = InventoryStoreItemQuerableDB.ToList();
            totalValue = InventoryStoreItemQuerableDB.Select(x => x.SumaverageUnitPrice != null && x.SumaverageUnitPrice != 0 ? x.SumaverageUnitPrice : x.SumcustomeUnitPrice != null ? x.SumcustomeUnitPrice : 0).Sum() ?? 0;
            //var totalValuea = _Context.V_InventoryStoreItemPrice.Where(x => x.Active == true && x.StoreActive == true).Select(x => (x.SUMAverageUnitPrice != null && x.SUMAverageUnitPrice != 0) ? x.SUMAverageUnitPrice : (x.SUMCustomeUnitPrice != null ? x.SUMCustomeUnitPrice : 0)).Sum() ?? 0;



            //var SUMAverageUnitPriceQuerable = InventoryStoreItemQuerableDB.Where(x => x.CalculationType == 1 && x.SUMAverageUnitPrice != null).Select(x => (decimal)x.SUMAverageUnitPrice).Sum();
            //var SUMAverageUnitPrice = _Context.V_InventoryStoreItemPrice.Where(x => x.Active == true && x.StoreActive == true && x.CalculationType == 1 && x.SUMAverageUnitPrice != null).Sum(x => (decimal)x.SUMAverageUnitPrice);


            //   //var SUMMaxUnitPrice = InventoryStoreItemQuerableDB.Where(x => x.CalculationType == 2 && x.SUMMaxUnitPrice != null)?.Select(x => (decimal)x.SUMMaxUnitPrice)?.Sum()??0;

            //   var SUMLastUnitPrice = _Context.V_InventoryStoreItemPrice.Where(x => x.Active == true && x.StoreActive == true && x.CalculationType == 3 && x.SUMLastUnitPrice != null).Sum(x => (decimal)x.SUMLastUnitPrice);
            //   var SUMLastUnitPriceQuerable = InventoryStoreItemQuerableDB.Where(x => x.CalculationType == 3 && x.SUMLastUnitPrice != null).Select(x => (decimal)x.SUMLastUnitPrice).Sum();

            //   var SUMCustomeUnitPrice = InventoryStoreItemQuerableDB.Where(x => x.CalculationType == 4 && x.SUMCustomeUnitPrice != null).Select(x => (decimal)x.SUMCustomeUnitPrice).Sum();
            //   totalValue = SUMAverageUnitPrice + 0 + SUMLastUnitPrice + SUMCustomeUnitPrice;

            //foreach (var Store in InventoryStoreItemListDB)
            //{
            //    if (Store.CalculationType == 1)
            //    {
            //        if (Store.SUMAverageUnitPrice != null)
            //        {
            //            totalValue = totalValue + (decimal)Store.SUMAverageUnitPrice;
            //        }
            //    }
            //    else if (Store.CalculationType == 2)
            //    {
            //        if (Store.SUMMaxUnitPrice != null)
            //        {
            //            totalValue = totalValue + (decimal)Store.SUMMaxUnitPrice;
            //        }
            //    }
            //    else if (Store.CalculationType == 3)
            //    {
            //        if (Store.SUMLastUnitPrice != null)
            //        {
            //            totalValue = totalValue + (decimal)Store.SUMLastUnitPrice;
            //        }
            //    }
            //    else if (Store.CalculationType == 4)
            //    {
            //        if (Store.SUMCustomeUnitPrice != null)
            //        {
            //            totalValue = totalValue + (decimal)Store.SUMCustomeUnitPrice;
            //        }
            //    }
            //}


            //foreach (var Store in InventoryItemsListDB)
            //{
            //    if (Store.CalculationType == 1)
            //    {
            //        totalValue = totalValue + (decimal)Store.SUMAverageUnitPrice;
            //    }
            //    else if (Store.CalculationType == 2)
            //    {
            //        totalValue = totalValue + (decimal)Store.SUMMaxUnitPrice;
            //    }
            //    else if (Store.CalculationType == 3)
            //    {
            //        totalValue = totalValue + (decimal)Store.SUMLastUnitPrice;
            //    }
            //    else if (Store.CalculationType == 4)
            //    {
            //        totalValue = totalValue + (decimal)Store.SUMCustomeUnitPrice;
            //    }
            //}
            return totalValue;
        }

        public static decimal GetSalesForceCollectedPercent(decimal TotalAmountVolume, GarasTestContext _Context)
        {

            decimal collected = 0;

            var clientAccounts = _Context.ClientAccounts.Where(x => x.Active == true && x.CreationDate <= DateTime.Now).ToList();
            if (clientAccounts.Count() > 0)
            {
                foreach (var ClientAccOBJ in clientAccounts)
                {
                    if (ClientAccOBJ.AmountSign == "plus")
                    {
                        collected = collected + ClientAccOBJ.Amount;
                    }
                    else if (ClientAccOBJ.AmountSign == "minus")
                    {
                        collected = collected - ClientAccOBJ.Amount;
                    }
                }
            }
            decimal CollectedPercent = TotalAmountVolume != 0 ? collected / TotalAmountVolume * 100 : 0;

            return CollectedPercent;
        }

        public static decimal GetPurchasingCollectedPercent(decimal TotalAmountVolume, GarasTestContext _Context)
        {

            decimal collected = 0;
            var SupplierAccounts = _Context.SupplierAccounts.Where(x => x.Active == true && x.CreationDate <= DateTime.Now).ToList();
            if (SupplierAccounts.Count() > 0)
            {
                foreach (var SupplierAccOBJ in SupplierAccounts)
                {
                    if (SupplierAccOBJ.AmountSign == "plus")
                    {
                        collected = collected + SupplierAccOBJ.Amount;
                    }
                    else if (SupplierAccOBJ.AmountSign == "minus")
                    {
                        collected = collected - SupplierAccOBJ.Amount;
                    }
                }
            }
            decimal CollectedPercent = TotalAmountVolume != 0 ? collected / TotalAmountVolume * 100 : 0;

            return CollectedPercent;
        }

        public static decimal GetNetProfitIncomeStatment(GarasTestContext _Context)
        {
            decimal Amount = 0;
            Amount = _Context.Accounts.Where(x => x.ParentCategory == 0 && (x.AccountCategoryId == 4 || x.AccountCategoryId == 5)).Select(x => x.Accumulative).Sum();
            #region Old Calc ...
            //var AccountsListDB = _Context.proc_AccountsLoadAll().Where(x => x.ParentCategory == 0 && (x.AccountCategoryID == 4 || x.AccountCategoryID == 5)).ToList();
            //var AcumelativeIncomeEntrySum = _Context.proc_AccountsLoadAll().Where(x => x.ParentCategory == 0 && x.AccountCategoryID == 4).Select(x => x.Accumulative).Sum();
            //var AcumelativeIncomeEntrySum = _Context.proc_AccountOfJournalEntryLoadAll().Where(x => IDAccountsIncomeDB.Contains(x.AccountID) && x.Active == true).Select(x => x.Amount).Sum();

            //var AcumelativeExpensesEntrySum = _Context.proc_AccountsLoadAll().Where(x => x.ParentCategory == 0 && x.AccountCategoryID == 5).Select(x => x.Accumulative).Sum();
            // var AcumelativeExpensesEntrySum = _Context.proc_AccountOfJournalEntryLoadAll().Where(x => IDAccountsExpensesDB.Contains(x.AccountID) && x.Active == true).Select(x => x.Amount).Sum();

            //if (AcumelativeIncomeEntrySum > AcumelativeExpensesEntrySum)
            //{
            //}
            //Amount = AcumelativeIncomeEntrySum + AcumelativeExpensesEntrySum;
            #endregion
            return Amount;
        }

        public static long GetTotalCountOfProjects(string ProjectsStatus, GarasTestContext _Context)
        {
            long TotalProjectsCount = 0;

            Expression<Func<VProjectSalesOffer, bool>> whereClause;

            switch (ProjectsStatus.ToLower())
            {
                case "open":
                    whereClause = a => a.Closed == false && a.Active == true;
                    break;
                case "closed":
                    whereClause = a => a.Closed == true && a.Active == true;
                    break;
                case "deactivated":
                    whereClause = a => a.Active == false;
                    break;
                default:
                    whereClause = a => a.Active == true;
                    break;
            }

            var projects = new List<VProjectSalesOffer>();

            TotalProjectsCount = _Context.VProjectSalesOffers.Where(whereClause).Count();

            return TotalProjectsCount;
        }

        public static decimal GetTotalCostOfProjects(string ProjectsStatus, GarasTestContext _Context)
        {
            decimal TotalProjectsCost = 0;

            Expression<Func<VProjectSalesOffer, bool>> whereClause;

            switch (ProjectsStatus.ToLower())
            {
                case "open":
                    whereClause = a => a.Closed == false && a.Active == true;
                    break;
                case "closed":
                    whereClause = a => a.Closed == true && a.Active == true;
                    break;
                case "deactivated":
                    whereClause = a => a.Active == false;
                    break;
                default:
                    whereClause = a => a.Active == true;
                    break;
            }

            var projects = new List<VProjectSalesOffer>();

            projects = _Context.VProjectSalesOffers.Where(whereClause).ToList();

            foreach (var project in projects)
            {
                TotalProjectsCost += project.ExtraCost ?? 0 + project.FinalOfferPrice ?? 0;
            }

            return TotalProjectsCost;
        }

        public static decimal GetTotalCollectedCostOfProjects(string ProjectsStatus, GarasTestContext _Context)
        {
            decimal TotalProjectsCollectedCost = 0;

            Expression<Func<VProjectSalesOffer, bool>> whereClause;

            switch (ProjectsStatus.ToLower())
            {
                case "open":
                    whereClause = a => a.Closed == false && a.Active == true;
                    break;
                case "closed":
                    whereClause = a => a.Closed == true && a.Active == true;
                    break;
                case "deactivated":
                    whereClause = a => a.Active == false;
                    break;
                default:
                    whereClause = a => a.Active == true;
                    break;
            }

            var projects = new List<VProjectSalesOffer>();

            projects = _Context.VProjectSalesOffers.Where(whereClause).ToList();

            foreach (var project in projects)
            {
                decimal ProjectCollected = 0;

                var clientAccounts = _Context.ClientAccounts.Where(x => x.ProjectId == project.Id);
                foreach (var clientAccount in clientAccounts)
                {
                    if (clientAccount.AmountSign == "plus")
                    {
                        ProjectCollected = ProjectCollected + clientAccount.Amount;
                    }
                    else if (clientAccount.AmountSign == "minus")
                    {
                        ProjectCollected = ProjectCollected - clientAccount.Amount;
                    }
                }

                TotalProjectsCollectedCost += ProjectCollected;
            }

            return TotalProjectsCollectedCost;
        }

        public static string GetInventoryItemExpDateFromMatrialAddingOrder(long id, GarasTestContext _Context)
        {
            string ExpDate = null;
            if (id != 0)
            {
                var ExpDateInventoryItemMatrialByOrderOBJDB = _Context.VInventoryAddingOrderItems.Where(x => x.InventoryItemId == id).Select(x => x.ExpDate).FirstOrDefault();
                if (ExpDateInventoryItemMatrialByOrderOBJDB != null)
                {
                    ExpDate = ((DateTime)ExpDateInventoryItemMatrialByOrderOBJDB).ToShortDateString();
                }
            }
            return ExpDate;
        }
        public static long GetNoOFInventoryItem(GarasTestContext _Context)
        {
            long NoOfItem = _Context.InventoryItems.Where(x => x.Active == true).Select(x => x.Id).Distinct().Count();
            return NoOfItem;
        }
        
        public static string GetIncentoryCalculationMethod(int id, GarasTestContext _Context)
        {
            string name = "";
            if (id == 1)
            {
                name = "Average";
            }
            else if (id == 2)
            {
                name = "Max";
            }
            else if (id == 3)
            {
                name = "Last";
            }
            else if (id == 4)
            {
                name = "Custom";
            }
            return name;
        }
        public static string getInventoryItemName(long id, GarasTestContext _Context)
        {
            string name = "";
            var LoadObjDB = _Context.InventoryItems.Find(id);
            if (LoadObjDB != null)
            {
                name = LoadObjDB.Name;
            }
            return name;
        }

        public static string GeyInventoryStoreItemSerial(long id, GarasTestContext _Context)
        {
            string name = "";
            var LoadObjDB = _Context.VInventoryStoreItemPrices.Where(x => x.InventoryItemId == id).FirstOrDefault();
            if (LoadObjDB != null)
            {
                name = LoadObjDB.Code;
            }
            return name;
        }

        public static string SaveSmallFileIff(string PathAddress, IFormFile FileContent, string FileName, string FileExtension, IWebHostEnvironment host, int height, int width)
        {
            string path = Path.Combine(host.WebRootPath, PathAddress);

            //Check if directory exist
            string directory = Path.GetDirectoryName(PathAddress);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path); //Create directory if it doesn't exist
            }

            string FullFileName = DateTime.Now.ToFileTime() + "_" + FileName.Trim().Replace(" ", "") + "." + FileExtension;
            string FilePath = Path.Combine(path, FullFileName);

            using (var memoryStream = new MemoryStream())
            {
                FileContent.CopyTo(memoryStream);
                memoryStream.Position = 0;
                using (Image originalImage = Image.FromStream(memoryStream))
                {
                    using (Bitmap resizedImage = new Bitmap(width, height))
                    {
                        using (Graphics graphics = Graphics.FromImage(resizedImage))
                        {
                            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            graphics.DrawImage(originalImage, 0, 0, width, height);
                            resizedImage.Save(FilePath);
                        }
                    }
                }
            }
            var DbSavingPath = PathAddress + FullFileName;
            return DbSavingPath;
        }


    }
}
