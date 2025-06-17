using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using NewGaras.Infrastructure.DBContext;
using NewGaras.Infrastructure.Helper.TenantService;
using NewGaras.Infrastructure;
using NewGaras.Infrastructure.Interfaces.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewGaras.Domain.Models;
using NewGaras.Infrastructure.Models.Inventory;
using NewGaras.Infrastructure.Models.Library;

namespace NewGaras.Domain.Services.Library
{
    public class LibraryService : ILibraryService
    {
        private readonly ITenantService _tenantService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _host;
        private HearderVaidatorOutput validation;
        public HearderVaidatorOutput Validation
        {
            get
            {
                return validation;
            }
            set
            {
                validation = value;
            }
        }
        public LibraryService(ITenantService tenantService, IMapper mapper, IWebHostEnvironment host, IUnitOfWork unitOfWork)
        {

            _tenantService = tenantService;
            _mapper = mapper;
            _host = host;
            _unitOfWork = unitOfWork;
        }
        public BaseResponseWithData<List<GetBorrowedBookData>> GetBorrowedBookData()
        {
            var Response = new BaseResponseWithData<List<GetBorrowedBookData>>();
            Response.Result = true;
            Response.Errors = new List<Error>();

            try
            {
                var matrialRelaseItems = _unitOfWork.InventoryMatrialReleases.FindAll(a => a.Status == "Pending", new[] { "ToUser", "MatrialRequest", "MatrialRequest.InventoryMatrialRequestItems", "MatrialRequest.InventoryMatrialRequestItems.InventoryItem" });

                var PendingInventoryItemsIDs = matrialRelaseItems.SelectMany(a => a.MatrialRequest.InventoryMatrialRequestItems.Select(b => b.InventoryItemId)).ToList();


                var inventoryStoreItemList = _unitOfWork.InventoryStoreItems.FindAll(a => (a.OperationType.Contains("Release Order") || a.OperationType.Contains("Client Returns") || a.OperationType.Contains("Opening Balance")) && PendingInventoryItemsIDs.Contains(a.InventoryItemId), new[] { "InventoryItem" });

                var InventoryMatrialReleaseIDs = inventoryStoreItemList.Select(a => a.OrderId).ToList();

                var InventoryMatrialReleaseList = _unitOfWork.InventoryMatrialReleases.FindAll(a => InventoryMatrialReleaseIDs.Contains(a.Id) && a.Status.Contains("Pending"), new[] { "ToUser", "InventoryMatrialReleaseItems" }).ToList();

                var inventoryItemsIDs = inventoryStoreItemList.Select(a => a.InventoryItemId).Distinct();
                var inventoryItemsData = _unitOfWork.InventoryItems.FindAll(a => inventoryItemsIDs.Contains(a.Id));

                var groupedData = inventoryStoreItemList.ToList().GroupBy(a => a.InventoryItemId);

                var newBorrowedBookDataList = new List<GetBorrowedBookData>();


                var listOfMatrialRequest = _unitOfWork.InventoryMatrialRequestItems.FindAll(a => inventoryItemsIDs.Contains(a.InventoryItemId), new[] { "InventoryMatrialRequest" }).ToList();

                //var listOfBooksUsers = listOfMatrialRequest

                foreach (var data in groupedData)
                {
                    var newBorrowedBookData = new GetBorrowedBookData();

                    var itemData = data.ToList();
                    var totalBooks = itemData.Where(a => a.OperationType.Contains("Opening Balance")).Sum(b => b.Balance1);


                    newBorrowedBookData.BookID = data.Key;
                    newBorrowedBookData.Title = inventoryItemsData.Where(a => a.Id == data.Key).FirstOrDefault().Name;
                    newBorrowedBookData.Available = itemData.Sum(a => a.FinalBalance);
                    newBorrowedBookData.Borrowed = (double)totalBooks - (double?)newBorrowedBookData.Available;

                    var ordersIds = itemData.Select(c => c.Id);

                    //var usersInItem = matrialRelaseItems.Where(a => PendingInventoryItemsIDs.Contains(a.MatrialRequest.InventoryMatrialRequestItems.(b => b.InventoryItemId))).ToList();
                    var itemUsers = new List<string>();

                    var booksUserList = listOfMatrialRequest.Where(a => a.InventoryItemId == data.Key).Select(
                        a => a.InventoryMatrialRequest?.FromUser?.FirstName + " " + a.InventoryMatrialRequest?.FromUser?.LastName).ToList();

                    itemUsers.AddRange(booksUserList);

                    newBorrowedBookData.Borrowers = itemUsers;
                    newBorrowedBookDataList.Add(newBorrowedBookData);
                }


                Response.Data = newBorrowedBookDataList;
                return Response;
            }
            catch (Exception ex)
            {
                Response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                Response.Errors.Add(error);
                return Response;
            }
        }

        public BaseResponseWithData<GetBorrowedBookForEachUserList> GetBorrowedBookForEachUser()
        {
            var response = new BaseResponseWithData<GetBorrowedBookForEachUserList>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var booksData = _unitOfWork.InventoryMatrialReleases.FindAll(a => true, new[] { "ToUser", "MatrialRequest", "MatrialRequest.InventoryMatrialRequestItems", "MatrialRequest.InventoryMatrialRequestItems.InventoryItem" }).ToList();

                var booksGroupByUser = booksData.GroupBy(a => a.ToUser).ToList();

                var fullBooksData = new GetBorrowedBookForEachUserList();

                var listOfBooks = new List<GetBorrowedBookForEachUser>();

                foreach (var book in booksGroupByUser)
                {
                    var borrwedUser = book.Key;

                    var currentBook = new GetBorrowedBookForEachUser();

                    currentBook.UserName = borrwedUser.FirstName + " " + borrwedUser.LastName;


                    var fullBookData = book.ToList();

                    currentBook.totalBooksBorrowed = fullBookData.Select(a => a.Status == "Pending").Count();
                    currentBook.LastBorrowingDate = fullBookData.LastOrDefault().CreationDate.ToShortDateString();
                    currentBook.LastBorrowedBookName = fullBookData.LastOrDefault().MatrialRequest.InventoryMatrialRequestItems.FirstOrDefault().InventoryItem.Name;


                    listOfBooks.Add(currentBook);
                }

                fullBooksData.BooksList = listOfBooks;

                response.Data = fullBooksData;

                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);
                return response;
            }
        }

        public BaseResponseWithData<GetBooksDashboardData> GetBooksDashboardData()
        {
            var response = new BaseResponseWithData<GetBooksDashboardData>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var fullBookData = _unitOfWork.InventoryStoreItems.FindAllQueryable(a => true);


                var totalNumOfBooks = fullBookData.Select(a => a.InventoryItemId).Distinct().Count();

                var totalNumOfCopies = fullBookData.Where(b => b.OperationType == "Opening Balance").Select(a => a.InventoryItemId).Count();

                var totalBooksInLibrary = fullBookData.Sum(a => a.Balance1);

                var totalBorrowedBooks = Math.Abs(totalNumOfCopies - totalBooksInLibrary);

                var totalBorrowers = _unitOfWork.InventoryMatrialReleases.Count(a => a.Status == "Pending");


                var fianlData = new GetBooksDashboardData()
                {
                    TotalBooks = totalNumOfBooks,
                    TotalCopies = totalNumOfCopies,
                    BooksInLibrary = totalBooksInLibrary,
                    BooksBorrowed = totalBorrowedBooks,
                    TotalBorrowers = totalBorrowers,
                };
                response.Data = fianlData;

                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);
                return response;
            }
        }

        public BaseResponseWithData<GetBorrowedBooksList> GetBorrowedBooksList()
        {
            var response = new BaseResponseWithData<GetBorrowedBooksList>()
            {
                Result = true,
                Errors = new List<Error>()
            };

            try
            {
                var booksData = _unitOfWork.InventoryMatrialReleases.FindAll(a => true, new[] { "ToUser", "MatrialRequest", "MatrialRequest.InventoryMatrialRequestItems", "MatrialRequest.InventoryMatrialRequestItems.InventoryItem" }).ToList();

                var fullBooksData = new GetBorrowedBooksList();

                var listOfBooks = new List<GetBorrowedBooks>();

                foreach (var book in booksData)
                {
                    var borrwedUser = book.ToUser;

                    var currentBook = new GetBorrowedBooks();

                    currentBook.UserId = borrwedUser.Id;
                    currentBook.UserName = borrwedUser.FirstName + " " + borrwedUser.LastName;
                    currentBook.UserMobile = borrwedUser.Mobile;
                    currentBook.BookId = book.Id;
                    currentBook.BookName = book.MatrialRequest.InventoryMatrialRequestItems.FirstOrDefault().InventoryItem.Name;
                    currentBook.BorrowingDate = book.CreationDate.ToShortDateString();
                    listOfBooks.Add(currentBook);
                }

                listOfBooks = listOfBooks.OrderBy(a=> a.UserName).OrderBy(a=>a.BorrowingDate).ToList();

                fullBooksData.BooksList = listOfBooks;

                response.Data = fullBooksData;

                return response;
            }
            catch (Exception ex)
            {
                response.Result = false;
                Error error = new Error();
                error.ErrorCode = "Err10";
                error.ErrorMSG = ex.InnerException != null ? ex.InnerException.Message : ex.Message; ;
                response.Errors.Add(error);
                return response;
            }
        }

    }
}
