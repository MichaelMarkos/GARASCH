namespace NewGaras.Infrastructure.Models.Library
{
    public class GetBorrowedBooks
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string UserMobile { get; set; }
        public long BookId { get; set; }
        public string BookName { get; set; }
        public string BorrowingDate { get; set; }
    }
}