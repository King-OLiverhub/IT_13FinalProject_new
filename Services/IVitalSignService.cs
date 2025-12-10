using System.Collections.Generic;
using IT_13FinalProject.Models;

namespace IT_13FinalProject.Services
{
    public interface IVitalSignService
    {
        IReadOnlyList<VitalSign> GetAll();
        IReadOnlyList<VitalSign> GetByPatientId(int patientId);
        VitalSign? GetById(int id);
        VitalSign? GetLatestByPatientId(int patientId);
        void Add(VitalSign vitalSign);
        void Update(VitalSign vitalSign);
        void Delete(int id);
    }
}
