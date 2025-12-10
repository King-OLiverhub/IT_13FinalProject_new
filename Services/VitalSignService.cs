using System;
using System.Collections.Generic;
using System.Linq;
using IT_13FinalProject.Models;

namespace IT_13FinalProject.Services
{
    public class VitalSignService : IVitalSignService
    {
        private readonly List<VitalSign> _vitalSigns = new();

        public VitalSignService()
        {
            // Sample data for demonstration
            _vitalSigns.Add(new VitalSign
            {
                VitalSignId = 1,
                PatientId = 1,
                NurseId = 1,
                DoctorId = 1,
                RecordedDate = DateTime.Today.AddDays(-2),
                ChiefComplaint = "Chest discomfort",
                BloodPressure = "130/85",
                HeartRate = 78,
                RespiratoryRate = 16,
                Temperature = 36.8m,
                OxygenSaturation = 98m,
                Weight = 58.0m,
                Height = 165.0m,
                BodyMassIndex = 21.3m,
                Allergies = "None",
                PastIllnesses = "None",
                PastSurgeries = "None",
                CurrentMedications = "None",
                FamilyHistory = "Hypertension",
                ImmunizationHistory = "Complete",
                TimeInOut = "08:00 - 09:30",
                Department = "Cardiology",
                TypeOfVisit = "New",
                DurationSymptoms = "2 weeks",
                Remarks = "Patient stable",
                DoctorAssisted = "Dr. Hinata Murata",
                NurseName = "Nurse Erika",
                Patient = new Patient { PatientId = 1, FirstName = "Himeko", LastName = "Murata", Email = "himeko_murata@gmail.com" }
            });

            _vitalSigns.Add(new VitalSign
            {
                VitalSignId = 2,
                PatientId = 2,
                NurseId = 2,
                DoctorId = 2,
                RecordedDate = DateTime.Today.AddDays(-5),
                ChiefComplaint = "Migraine",
                BloodPressure = "118/76",
                HeartRate = 72,
                RespiratoryRate = 15,
                Temperature = 36.5m,
                OxygenSaturation = 99m,
                Weight = 55.0m,
                Height = 160.0m,
                BodyMassIndex = 21.5m,
                Allergies = "Ibuprofen",
                PastIllnesses = "Migraine",
                PastSurgeries = "None",
                CurrentMedications = "Sumatriptan",
                FamilyHistory = "Migraine",
                ImmunizationHistory = "Complete",
                TimeInOut = "10:00 - 11:15",
                Department = "Neurology",
                TypeOfVisit = "Follow-up",
                DurationSymptoms = "3 days",
                Remarks = "Headache improved with medication",
                DoctorAssisted = "Dr. Clint Miko",
                NurseName = "Nurse Rika",
                Patient = new Patient { PatientId = 2, FirstName = "Yae", LastName = "Miko", Email = "miko.yae@mail.com" }
            });
        }

        public IReadOnlyList<VitalSign> GetAll() => _vitalSigns.ToList();

        public IReadOnlyList<VitalSign> GetByPatientId(int patientId)
        {
            return _vitalSigns.Where(vs => vs.PatientId == patientId).ToList();
        }

        public VitalSign? GetById(int id)
        {
            return _vitalSigns.FirstOrDefault(vs => vs.VitalSignId == id);
        }

        public VitalSign? GetLatestByPatientId(int patientId)
        {
            return _vitalSigns
                .Where(vs => vs.PatientId == patientId)
                .OrderByDescending(vs => vs.RecordedDate)
                .FirstOrDefault();
        }

        public void Add(VitalSign vitalSign)
        {
            if (vitalSign == null)
                throw new ArgumentNullException(nameof(vitalSign));

            if (vitalSign.VitalSignId == 0)
                vitalSign.VitalSignId = _vitalSigns.Count > 0 ? _vitalSigns.Max(vs => vs.VitalSignId) + 1 : 1;

            vitalSign.CreatedAt = DateTime.UtcNow;
            vitalSign.UpdatedAt = DateTime.UtcNow;
            _vitalSigns.Add(vitalSign);
        }

        public void Update(VitalSign vitalSign)
        {
            if (vitalSign == null)
                throw new ArgumentNullException(nameof(vitalSign));

            var existingIndex = _vitalSigns.FindIndex(vs => vs.VitalSignId == vitalSign.VitalSignId);
            if (existingIndex >= 0)
            {
                vitalSign.UpdatedAt = DateTime.UtcNow;
                _vitalSigns[existingIndex] = vitalSign;
            }
        }

        public void Delete(int id)
        {
            _vitalSigns.RemoveAll(vs => vs.VitalSignId == id);
        }
    }
}
