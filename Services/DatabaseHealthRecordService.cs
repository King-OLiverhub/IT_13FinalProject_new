using IT_13FinalProject.Data;
using IT_13FinalProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT_13FinalProject.Services
{
    public class DatabaseHealthRecordService : IHealthRecordService
    {
        private readonly ApplicationDbContext _context;

        public DatabaseHealthRecordService(ApplicationDbContext context)
        {
            _context = context;
            
            // Add sample data if database is empty
            if (!_context.HealthRecords.Any())
            {
                // Add sample patients first
                if (!_context.Patients.Any())
                {
                    var samplePatients = new List<Patient>
                    {
                        new Patient
                        {
                            PatientId = 1,
                            FirstName = "Himeko",
                            LastName = "Murata",
                            Email = "himeko_murata@gmail.com",
                            Phone = "123-456-7890",
                            Address = "123 Main St, City",
                            DateOfBirth = DateTime.Today.AddYears(-30),
                            Gender = "Female",
                            BloodType = "A+",
                            EmergencyContact = "John Murata - 098-765-4321",
                            CreatedAt = DateTime.Now
                        },
                        new Patient
                        {
                            PatientId = 2,
                            FirstName = "Yae",
                            LastName = "Miko",
                            Email = "miko.yae@mail.com",
                            Phone = "098-765-4321",
                            Address = "456 Oak Ave, Town",
                            DateOfBirth = DateTime.Today.AddYears(-28),
                            Gender = "Female",
                            BloodType = "B+",
                            EmergencyContact = "Sakura Miko - 087-654-3210",
                            CreatedAt = DateTime.Now
                        }
                    };
                    
                    _context.Patients.AddRange(samplePatients);
                    _context.SaveChanges();
                }
                
                var sampleRecords = new List<Models.HealthRecord>
                {
                    new Models.HealthRecord
                    {
                        RecordId = 1,
                        PatientId = 1,
                        DoctorId = 1,
                        NurseId = 1,
                        ChiefComplaint = "Chest discomfort",
                        SymptomDuration = "2 weeks",
                        VisitType = "New",
                        Department = "Cardiology",
                        TimeInOut = "08:00 - 09:30",
                        SubjectiveFindings = "Patient reports intermittent chest pain.",
                        ObjectiveFindings = "Normal rhythm, slight murmur.",
                        AssessmentDiagnosis = "Rule out angina.",
                        Icd10Code = "I20.0",
                        AdditionalTestsOrder = "ECG, Blood tests",
                        DoctorRemarks = "Monitor closely, follow up in 2 weeks",
                        NurseRemarks = "Patient stable, vitals normal",
                        Recommendations = "Avoid strenuous activities",
                        MedicineName = "Aspirin",
                        Dosage = "81 mg",
                        Frequency = "Once daily",
                        Duration = "30 days",
                        TreatmentNotes = "Take with food",
                        FollowUpDate = DateTime.Today.AddDays(14),
                        VisitStatus = "Pending Assessment",
                        DoctorSignature = "Dr. Hinata Murata",
                        ApprovalDate = DateTime.Today,
                        RecordDate = DateTime.Today.AddDays(-2),
                        Diagnosis = "Chest pain, unspecified",
                        Treatment = "Medical management",
                        Prescription = "Aspirin 81mg daily",
                        SpecialInstructions = "Follow up if symptoms worsen",
                        DoctorInitialRemarks = "Initial assessment complete"
                    },
                    new Models.HealthRecord
                    {
                        RecordId = 2,
                        PatientId = 2,
                        DoctorId = 2,
                        NurseId = 2,
                        ChiefComplaint = "Migraine",
                        SymptomDuration = "3 days",
                        VisitType = "Follow-up",
                        Department = "Neurology",
                        TimeInOut = "10:00 - 11:15",
                        SubjectiveFindings = "Headache improved with medication",
                        ObjectiveFindings = "Neurological exam normal",
                        AssessmentDiagnosis = "Migraine without aura",
                        Icd10Code = "G43.0",
                        AdditionalTestsOrder = "None",
                        DoctorRemarks = "Continue current treatment",
                        NurseRemarks = "Patient responding well to treatment",
                        Recommendations = "Avoid trigger foods",
                        MedicineName = "Sumatriptan",
                        Dosage = "50 mg",
                        Frequency = "As needed",
                        Duration = "10 days",
                        TreatmentNotes = "Take at onset of symptoms",
                        FollowUpDate = DateTime.Today.AddDays(30),
                        VisitStatus = "Discharged",
                        DoctorSignature = "Dr. Clint Miko",
                        ApprovalDate = DateTime.Today.AddDays(-3),
                        RecordDate = DateTime.Today.AddDays(-5),
                        Diagnosis = "Migraine without aura",
                        Treatment = "Medication management",
                        Prescription = "Sumatriptan 50mg PRN",
                        SpecialInstructions = "Return if symptoms worsen",
                        DoctorInitialRemarks = "Stable condition"
                    }
                };
                
                _context.HealthRecords.AddRange(sampleRecords);
                _context.SaveChanges();
            }
        }

        public IReadOnlyList<HealthRecord> GetAll()
        {
            return _context.HealthRecords
                .Include(r => r.Patient)
                .ToList()
                .Select(r => ConvertToServiceModel(r))
                .ToList();
        }

        public HealthRecord? GetById(string id)
        {
            var record = _context.HealthRecords
                .Include(r => r.Patient)
                .FirstOrDefault(r => r.RecordId.ToString() == id);
            
            return record != null ? ConvertToServiceModel(record) : null;
        }

        public void Add(HealthRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            var entity = ConvertToEntity(record);
            _context.HealthRecords.Add(entity);
            _context.SaveChanges();
        }

        public void Update(HealthRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record));

            var entity = _context.HealthRecords
                .FirstOrDefault(r => r.RecordId.ToString() == record.Id);
            
            if (entity != null)
            {
                UpdateEntityFromModel(entity, record);
                _context.SaveChanges();
            }
        }

        public void Delete(string id)
        {
            var entity = _context.HealthRecords
                .FirstOrDefault(r => r.RecordId.ToString() == id);
            
            if (entity != null)
            {
                _context.HealthRecords.Remove(entity);
                _context.SaveChanges();
            }
        }

        private HealthRecord ConvertToServiceModel(Models.HealthRecord entity)
        {
            return new HealthRecord
            {
                Id = entity.RecordId.ToString(),
                PatientId = entity.PatientId,
                PatientName = $"{entity.Patient?.FirstName} {entity.Patient?.LastName}".Trim(),
                PatientEmail = entity.Patient?.Email,
                DateOfCheckup = entity.RecordDate,
                VisitStatus = entity.VisitStatus,
                Department = entity.Department,
                ChiefComplaint = entity.ChiefComplaint,
                SymptomDuration = entity.SymptomDuration,
                TimeInOut = entity.TimeInOut,
                VisitType = entity.VisitType,
                SubjectiveFindings = entity.SubjectiveFindings,
                ObjectiveFindings = entity.ObjectiveFindings,
                AssessmentDiagnosis = entity.AssessmentDiagnosis,
                ICD10Code = entity.Icd10Code,
                AdditionalTestsOrder = entity.AdditionalTestsOrder,
                DoctorRemarks = entity.DoctorRemarks,
                NurseRemarks = entity.NurseRemarks,
                Recommendations = entity.Recommendations,
                MedicineName = entity.MedicineName,
                Dosage = entity.Dosage,
                Frequency = entity.Frequency,
                Duration = entity.Duration,
                TreatmentNotes = entity.TreatmentNotes,
                FollowUpDate = entity.FollowUpDate,
                DoctorSignature = entity.DoctorSignature,
                ApprovalDate = entity.ApprovalDate,
                // Medical history and vital signs will come from VitalSign service
                Allergies = null, // Will be populated from VitalSign service
                PastIllnesses = null, // Will be populated from VitalSign service
                PastSurgeries = null, // Will be populated from VitalSign service
                CurrentMedications = null, // Will be populated from VitalSign service
                FamilyHistory = null, // Will be populated from VitalSign service
                ImmunizationHistory = null, // Will be populated from VitalSign service
                BloodPressure = null, // Will be populated from VitalSign service
                HeartRate = null, // Will be populated from VitalSign service
                RespiratoryRate = null, // Will be populated from VitalSign service
                Temperature = null, // Will be populated from VitalSign service
                OxygenSaturation = null, // Will be populated from VitalSign service
                Weight = null, // Will be populated from VitalSign service
                Height = null, // Will be populated from VitalSign service
                BodyMassIndex = null // Will be populated from VitalSign service
            };
        }

        private Models.HealthRecord ConvertToEntity(HealthRecord model)
        {
            return new Models.HealthRecord
            {
                RecordId = int.Parse(model.Id),
                PatientId = 1, // You'll need to map this properly
                RecordDate = model.DateOfCheckup ?? DateTime.Now,
                VisitStatus = model.VisitStatus,
                Department = model.Department,
                ChiefComplaint = model.ChiefComplaint,
                Diagnosis = model.AssessmentDiagnosis,
                MedicineName = model.MedicineName,
                Recommendations = model.Recommendations,
                FollowUpDate = model.FollowUpDate
            };
        }

        private void UpdateEntityFromModel(Models.HealthRecord entity, HealthRecord model)
        {
            entity.RecordDate = model.DateOfCheckup ?? entity.RecordDate;
            entity.VisitStatus = model.VisitStatus;
            entity.Department = model.Department;
            entity.ChiefComplaint = model.ChiefComplaint;
            entity.SymptomDuration = model.SymptomDuration;
            entity.TimeInOut = model.TimeInOut;
            entity.VisitType = model.VisitType;
            entity.SubjectiveFindings = model.SubjectiveFindings;
            entity.ObjectiveFindings = model.ObjectiveFindings;
            entity.AssessmentDiagnosis = model.AssessmentDiagnosis;
            entity.Icd10Code = model.ICD10Code;
            entity.AdditionalTestsOrder = model.AdditionalTestsOrder;
            entity.DoctorRemarks = model.DoctorRemarks;
            entity.NurseRemarks = model.NurseRemarks;
            entity.Recommendations = model.Recommendations;
            entity.MedicineName = model.MedicineName;
            entity.Dosage = model.Dosage;
            entity.Frequency = model.Frequency;
            entity.Duration = model.Duration;
            entity.TreatmentNotes = model.TreatmentNotes;
            entity.FollowUpDate = model.FollowUpDate;
            entity.DoctorSignature = model.DoctorSignature;
            entity.ApprovalDate = model.ApprovalDate;
        }
    }
}
