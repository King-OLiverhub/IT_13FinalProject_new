using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace IT_13FinalProject.Services
{
    public class HealthRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        [Required(ErrorMessage = "Patient name is required.")]
        public string PatientName { get; set; } = string.Empty;

        [EmailAddress]
        public string? PatientEmail { get; set; }

        public int? PatientId { get; set; }

        public string? DoctorName { get; set; }
        public string? DoctorEmail { get; set; }
        public string? NurseName { get; set; }
        public string? NurseEmail { get; set; }

        [Required(ErrorMessage = "Date of visit is required.")]
        public DateTime? DateOfCheckup { get; set; } = DateTime.Today;

        public string? VisitStatus { get; set; } = "Pending Assessment";
        public string? VisitType { get; set; }
        public string? Department { get; set; }
        public string? ChiefComplaint { get; set; }
        public string? SymptomDuration { get; set; }
        public string? TimeInOut { get; set; }

        public DateTime? FollowUpDate { get; set; }

        // Medical history
        public string? Allergies { get; set; }
        public string? PastIllnesses { get; set; }
        public string? PastSurgeries { get; set; }
        public string? CurrentMedications { get; set; }
        public string? FamilyHistory { get; set; }
        public string? ImmunizationHistory { get; set; }

        // Vital signs
        public string? BloodPressure { get; set; }
        public string? HeartRate { get; set; }
        public string? RespiratoryRate { get; set; }
        public string? Temperature { get; set; }
        public string? OxygenSaturation { get; set; }
        public string? Weight { get; set; }
        public string? Height { get; set; }
        public string? BodyMassIndex { get; set; }

        // Clinical findings
        public string? SubjectiveFindings { get; set; }
        public string? ObjectiveFindings { get; set; }
        public string? AssessmentDiagnosis { get; set; }
        public string? ICD10Code { get; set; }
        public string? AdditionalTestsOrder { get; set; }
        public string? DoctorRemarks { get; set; }
        public string? NurseRemarks { get; set; }
        public string? Recommendations { get; set; }

        // Treatment / prescription
        public string? MedicineName { get; set; }
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public string? Duration { get; set; }
        public string? TreatmentNotes { get; set; }

        // Follow-up & approval
        public string? VisitOutcome { get; set; }
        public string? DoctorSignature { get; set; }
        public DateTime? ApprovalDate { get; set; }

        public HealthRecord Clone()
        {
            return (HealthRecord)MemberwiseClone();
        }
    }

    public interface IHealthRecordService
    {
        IReadOnlyList<HealthRecord> GetAll();
        HealthRecord? GetById(string id);
        void Add(HealthRecord record);
        void Update(HealthRecord record);
        void Delete(string id);
    }

    public class InMemoryHealthRecordService : IHealthRecordService
    {
        private readonly List<HealthRecord> _records = new();

        public InMemoryHealthRecordService()
        {
            _records.Add(new HealthRecord
            {
                Id = "1",
                PatientId = 1,
                PatientName = "Himeko Murata",
                PatientEmail = "himeko_murata@gmail.com",
                DoctorName = "Dr. Hinata Murata",
                DoctorEmail = "dr.hinata@clinic.com",
                NurseName = "Nurse Erika",
                NurseEmail = "erika.nurse@clinic.com",
                DateOfCheckup = DateTime.Today.AddDays(-2),
                VisitStatus = "Pending Assessment",
                VisitType = "New",
                Department = "Cardiology",
                ChiefComplaint = "Chest discomfort",
                Allergies = "None",
                BloodPressure = "130/85",
                HeartRate = "78",
                RespiratoryRate = "16",
                Temperature = "36.8°C",
                OxygenSaturation = "98%",
                Weight = "58 kg",
                Height = "165 cm",
                BodyMassIndex = "21.3",
                SubjectiveFindings = "Patient reports intermittent chest pain.",
                ObjectiveFindings = "Normal rhythm, slight murmur.",
                AssessmentDiagnosis = "Rule out angina.",
                ICD10Code = "I20.0",
                MedicineName = "Aspirin",
                Dosage = "81 mg",
                Frequency = "Once daily",
                Duration = "30 days",
                VisitOutcome = "Under observation",
                FollowUpDate = DateTime.Today.AddDays(14)
            });

            _records.Add(new HealthRecord
            {
                Id = "2",
                PatientId = 2,
                PatientName = "Yae Miko",
                PatientEmail = "miko.yae@mail.com",
                DoctorName = "Dr. Clint Miko",
                DoctorEmail = "clint.miko@clinic.com",
                NurseName = "Nurse Rika",
                NurseEmail = "rika.nurse@clinic.com",
                DateOfCheckup = DateTime.Today.AddDays(-5),
                VisitStatus = "Discharged",
                VisitType = "Follow-up",
                Department = "Neurology",
                ChiefComplaint = "Migraine",
                Allergies = "Ibuprofen",
                BloodPressure = "118/76",
                HeartRate = "72",
                RespiratoryRate = "15",
                Temperature = "36.5°C",
                OxygenSaturation = "99%",
                SubjectiveFindings = "Headache improved.",
                ObjectiveFindings = "Neurological exam normal.",
                AssessmentDiagnosis = "Migraine without aura.",
                MedicineName = "Sumatriptan",
                Dosage = "50 mg",
                Frequency = "As needed",
                Duration = "10 days",
                TreatmentNotes = "Avoid trigger foods.",
                VisitOutcome = "Stable",
                FollowUpDate = DateTime.Today.AddDays(30)
            });
        }

        public IReadOnlyList<HealthRecord> GetAll() => _records.ToList();

        public HealthRecord? GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return _records.FirstOrDefault(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }

        public void Add(HealthRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            if (string.IsNullOrWhiteSpace(record.Id))
            {
                record.Id = Guid.NewGuid().ToString("N");
            }

            _records.Add(record);
        }

        public void Update(HealthRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            var existingIndex = _records.FindIndex(r => r.Id.Equals(record.Id, StringComparison.OrdinalIgnoreCase));
            if (existingIndex >= 0)
            {
                _records[existingIndex] = record;
            }
        }

        public void Delete(string id)
        {
            _records.RemoveAll(r => r.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        }
    }
}
