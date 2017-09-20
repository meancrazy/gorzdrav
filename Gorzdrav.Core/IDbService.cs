using System;
using Gorzdrav.Core.ViewModels;
using ReactiveUI;

namespace Gorzdrav.Core
{
    public interface IDbService : IDisposable
    {
        ReactiveList<PatientViewModel> Patients { get; }

        PatientViewModel SelectedPatient { get; set; }
    }
}