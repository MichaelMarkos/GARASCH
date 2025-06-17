using NewGarasAPI.Models.Project.UsedInResponses;

namespace NewGarasAPI.Models.Project.Responses
{
    public class InventoryMatrialReleaseUnionInternalBackOrderItemsResponse
    {
        [DataMember]
        public bool result {  get; set; }

        [DataMember]
        public List<Error> errors { get; set; }

        [DataMember]
        public List<InventoryMatrialReleaseUnionInternalBackOrderItems> ListOfItems { get; set; }

        [DataMember]
        public PaginationHeader paginationHeader { get; set; }

    }
}
