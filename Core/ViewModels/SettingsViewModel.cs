using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Gorzdrav.Core.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        [DataContract]
        private class Settings
        {
            [DataMember]
            public PatientViewModel Patient { get; set; }

            [DataMember]
            public IList<PatientViewModel> Patients { get; set; }
        }

        private static readonly DataContractJsonSerializer Serializer = new DataContractJsonSerializer(typeof(Settings), new DataContractJsonSerializerSettings
        {
            DateTimeFormat = new DateTimeFormat("dd.MM.yyyy"),
            IgnoreExtensionDataObject = true
        });

        [Reactive]
        public PatientViewModel SelectedPatient { get; set; }

        public ReactiveList<PatientViewModel> Patients { get; } = new ReactiveList<PatientViewModel>();

        public ReactiveCommand<Unit, PatientViewModel> Add { get; }

        public ReactiveCommand<Unit, Unit> Delete { get; }

        public ReactiveCommand<Unit, Unit> Close { get; }

        public SettingsViewModel()
        {
            LoadSettings();

            var save = ReactiveCommand.Create(SaveSettings);

            var d0 = this.ObservableForProperty(x => x.SelectedPatient)
                         .DistinctUntilChanged().Select(_ => Unit.Default)
                         .Merge(Patients.Changed.Select(_ => Unit.Default))
                         .Throttle(TimeSpan.FromSeconds(0.3))
                         .InvokeCommand(save);
            
            Add = Interactions.AddPatient.ToReactiveCommand();
            var d1 = Add.Subscribe(patient =>
            {
                if (patient == null || Patients.Contains(patient))
                    return;

                Patients.Add(patient);
            });

            Delete = ReactiveCommand.Create(() =>
            {
                Patients.Remove(SelectedPatient);
            });
            
            Close = ReactiveCommandEx.CreateEmpty();

            InitCleanup(d0, d1);
        }

        private void SaveSettings()
        {
            var settings = new Settings
            {
                Patient = SelectedPatient,
                Patients = Patients,
            };

            if (File.Exists(Consts.SettingsFileName))
            {
                File.Delete(Consts.SettingsFileName);
            }

            using (var stream = File.OpenWrite(Consts.SettingsFileName))
            {
                Serializer.WriteObject(stream, settings);
            }
        }

        private void LoadSettings()
        {
            if (!File.Exists(Consts.SettingsFileName))
                return;

            using (var stream = File.OpenRead(Consts.SettingsFileName))
            {
                var settings = (Settings)Serializer.ReadObject(stream);

                SelectedPatient = settings.Patient;
                Patients.AddRange(settings.Patients);
            }
        }
    }
}