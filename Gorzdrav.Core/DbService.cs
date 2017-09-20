using System.Collections.Generic;
using System.Linq;
using Gorzdrav.Core.ViewModels;
using LiteDB;
using ReactiveUI;

namespace Gorzdrav.Core
{
    public class DbService : IDbService
    {
        private readonly LiteDatabase _database;
        private readonly LiteCollection<PatientViewModel> _patients;
        private readonly LiteCollection<PatientViewModel> _selectedPatient;

        static DbService()
        {
            BsonMapper.Global.Entity<PatientViewModel>().IgnoreRx().Ignore(x => x.FullName);
            BsonMapper.Global.Entity<ClinicViewModel>().IgnoreRx();
            BsonMapper.Global.Entity<DistrictViewModel>().IgnoreRx();
        }

        public DbService()
        {
            _database = new LiteDatabase(Consts.ConnectionString);
            _patients = _database.GetCollection<PatientViewModel>("Patients");
            _selectedPatient = _database.GetCollection<PatientViewModel>("SelectedPatient");
            
            Patients = new ReactiveList<PatientViewModel>(_patients.FindAll());
            SelectedPatient = _selectedPatient.FindAll().SingleOrDefault();
        }

        public ReactiveList<PatientViewModel> Patients { get; }

        public PatientViewModel SelectedPatient { get; set; }

        public void Dispose()
        {
            if (_database == null) return;
            
            _selectedPatient.Delete(Query.All());
            _selectedPatient.Insert(SelectedPatient);
            
            _patients.Delete(Query.All());
            _patients.Insert(Patients);

            _database.Dispose();
        }
    }
}