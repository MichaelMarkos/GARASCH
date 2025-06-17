namespace NewGaras.Infrastructure.Models.ProjectManagement
{
    public class ProgressUsers
    {
        public long? Id { get; set; }
        public long ProjectProgressId { get; set; }
        public long HrUserId { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public decimal HoursNum { get; set; }
        public decimal Evaluation { get; set; }
        public string Comment { get; set; }
        public bool Active { get; set; }

        public int? InventoryItemCategoryId { get; set; }
    }

    public class GetProgressUsers
    {
        public long? Id { get; set; }
        public long ProjectProgressId { get; set; }
        public long HrUserId { get; set; }
        public string HrUserName { get; set; }
        public string HrUserImg { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
        public decimal HoursNum { get; set; }
        public decimal Evaluation { get; set; }
        public string Comment { get; set; }
        public bool Active { get; set; }
        public int? InventoryItemCategoryId { get; set; }
        public string InventoryItemCategoryName { get; set; }
    }
}