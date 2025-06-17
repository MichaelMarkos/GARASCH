using NewGaras.Infrastructure.DTO.Medical.MedicalReservation;
using NewGaras.Infrastructure.Models.Medical;
using NewGaras.Infrastructure.Models.Medical.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewGaras.Infrastructure.Interfaces.ServicesInterfaces.Medical
{
    public interface IReservationService
    {
        public HearderVaidatorOutput Validation { get; set; }
        public BaseResponseWithId<long> AddMedicalExaminationOffer(AddMedicalExaminationOfferDTO offer);

        public BaseResponseWithId<long> EditMedicalExaminationOffer(EditMedicalExaminationOfferDTO offer);

        public BaseResponseWithData<List<GetMedicalExaminationOfferList>> GetMedicalExaminationOfferList();

        public BaseResponseWithId<long> AddMedicalReservation(AddMedicalReservationDTO data);

        public BaseResponseWithData<GetMedicalReservationList> GetMedicalReservationList(GetGetMedicalReservationFilters filters);
        public BaseResponseWithId<long> MoveReservationsToAnotherDoctor(MoveReservationsToAnotherDoctorDTO data, long userID);
        public BaseResponseWithId<long> AddClientPatientInfo(AddClientPatientInfoDTO data, long userID);
        public BaseResponseWithData<GetClientPatientInfoDTO> GetClientPatientInfo(long ClientID);
        public BaseResponseWithId<long> EditClientPatientInfo(EditClientPatientInfoDTO data, long userID);
        public SelectDDLResponse GetPaymentMethods();
        public BaseResponseWithData<List<int>> GetListOfSerialReserved(GetListOfSerialReservedFilters filters);
        public SelectDDLResponse GetPatientTypeDDl();
        public BaseResponseWithId<long> MoveReservationListToAnotherSchedule(MoveReservationListToAnotherSchedule dto);

        public BaseResponseWithData<GetMedicalReservationDTO> GetMedicalReservationById(long Id);
    }
}
