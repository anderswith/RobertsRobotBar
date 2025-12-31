using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Interfaces;

namespace RobotBarApp.ViewModels;

public class CalibrationWizardViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;
    private readonly IRobotLogic _robotLogic;

    public ObservableCollection<CalibrationStep> Steps { get; } = new();

    private int _currentIndex;
    public int CurrentIndex
    {
        get => _currentIndex;
        private set
        {
            if (SetProperty(ref _currentIndex, value))
            {
                OnPropertyChanged(nameof(CurrentStep));
                OnPropertyChanged(nameof(StepCounterText));
                OnPropertyChanged(nameof(CanGoPrevious));
                OnPropertyChanged(nameof(CanGoNext));
            }
        }
    }

    public CalibrationStep? CurrentStep =>
        (CurrentIndex >= 0 && CurrentIndex < Steps.Count) ? Steps[CurrentIndex] : null;

    public string StepCounterText => Steps.Count == 0
        ? ""
        : $"Trin {CurrentIndex + 1} / {Steps.Count}";

    public bool CanGoPrevious => CurrentIndex > 0;
    public bool CanGoNext => Steps.Count > 0 && CurrentIndex < Steps.Count - 1;

    public ICommand PreviousCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand RunStepCommand { get; }
    public ICommand CloseCommand { get; }

    public CalibrationWizardViewModel(
        INavigationService navigation,
        IRobotLogic robotLogic)
    {
        _navigation = navigation;
        _robotLogic = robotLogic;

        SeedSteps();
        CurrentIndex = 0;

        PreviousCommand = new RelayCommand(_ => Previous(), _ => CanGoPrevious);
        NextCommand = new RelayCommand(_ => Next(), _ => CanGoNext);
        RunStepCommand = new RelayCommand(_ => RunCurrentStepScripts(), _ => CurrentStep?.HasScripts == true);
        CloseCommand = new RelayCommand(_ => Close());
    }

    private void SeedSteps()
    {
        // NOTE: ImageSource expects a valid pack URI for embedded resources.
        // For this project the files are .jpg in Resources/KalibreringGuide/

        Steps.Add(new CalibrationStep
        {
            Number = 1,
            Title = "Robot setup",
            Description = "Sørg for, at alle ben er godt skruet fast og at bardisken, flaskestativet og robotten vipper så lidt som muligt.",
            ImageSource = "/RobotBarApp;component/Resources/KalibreringGuide/RobotBen.jpg"
        });

        Steps.Add(new CalibrationStep
        {
            Number = 2,
            Title = "Bar setup",
            Description = "Tjek om robotten og stativet er indstillet på den rigtige højde.",
            ImageSource = "/RobotBarApp;component/Resources/KalibreringGuide/RobotHeight.jpg"
        });

        Steps.Add(new CalibrationStep
        {
            Number = 3,
            Title = "Tøm stativ og kopholder",
            Description = "Sørg for, at flaskestativet og kopholderen på bardisken er tomme. Der må gerne være flaskeholdere på stativet, men ingen flasker.",
            ImageSource = "/RobotBarApp;component/Resources/KalibreringGuide/TomStativ.jpg"
        });

        Steps.Add(new CalibrationStep
        {
            Number = 4,
            Title = "Hjemme position",
            Description =
                "Start robotten op og tjek om den står i “hjemme” positionen. Hvis du er i tvivl, kan du åbne et program og manuelt køre til første viapunkt eller tjekke om koordinaterne på robot leddene passer til dem på billedet.\n\n" +
                "Pas på! Kør robotten langsomt til hjemme positionen for at undgå, at den kører ind i sig selv. Kig evt. på tablet animationen for at bekræfte at bevægelserne ser rigtige ud.",
            ImageSource = "/RobotBarApp;component/Resources/KalibreringGuide/HjemGraderRobotLed.jpg"
        });

        Steps.Add(new CalibrationStep
        {
            Number = 5,
            Title = "Kloen",
            Description = "Åbn kloen hvis den ikke allerede er åben. Det gøres ved at tilføje en RG6 fra URCaps under struktur og trykke på åbn.",
            ImageSource = "/RobotBarApp;component/Resources/KalibreringGuide/KloBetjening.jpg"
        });

        Steps.Add(new CalibrationStep
        {
            Number = 6,
            Title = "Kloen",
            Description = "Placér kalibrerings værktøjet på det markerede område så kanterne passer på kloen.",
            ImageSource = "/RobotBarApp;component/Resources/KalibreringGuide/KloMarkering.jpg"
        });

        Steps.Add(new CalibrationStep
        {
            Number = 7,
            Title = "Kloen",
            Description = "Hold nu ‘luk’ knappen på RG6 nede indtil den stopper selv. Så er robotten klar til at køre kalibreringsscripts.",
            ImageSource = "/RobotBarApp;component/Resources/KalibreringGuide/LukketKlo.jpg"
        });

        Steps.Add(new CalibrationStep
        {
            Number = 8,
            Title = "Kopholderen",
            Description =
                "Stå på ydersiden af bardisken og kør det første script mens du holder øje med kopholderen for at se om robotten rammer punktet på bunden. (der er ventetid i et par sekunder)\n\n" +
                "Hvis punktet rammes rigtigt, kan du gå videre, hvis ikke, prøv at justere bardisken eller sikre at kalibreringsværktøjet sidder rigtigt fast og kør samme script igen indtil punktet bliver ramt.",
            ImageSource = "/RobotBarApp;component/Resources/KalibreringGuide/PunktRamtBardisk.jpg",
            ScriptsToRun = new List<string> { "kalibreringBardisk.urp" }
        });

        Steps.Add(new CalibrationStep
        {
            Number = 9,
            Title = "Stativet",
            Description =
                "Nu vil robotten køre til venstre side af stativet og røre punktet i bunden af den yderste flaskeholder. Start scriptet og gør klar til at kigge igen.\n\n" +
                "Hvis der skal laves justeringer undervejs, er det en god idé at køre hele kalibreringen igennem en gang til, for at sikre at alle punkter bliver ramt rigtigt.",
            ImageSource = "/RobotBarApp;component/Resources/KalibreringGuide/PunktStativVenstre.jpg",
            ScriptsToRun = new List<string> { "kalibreringStativVenstre.urp" }
        });

        Steps.Add(new CalibrationStep
        {
            Number = 10,
            Title = "Stativet",
            Description = "Nu vil robotten køre til højre side af stativet og røre punktet i bunden af den yderste flaskeholder. Start scriptet og gør klar til at kigge igen.",
            ImageSource = "/RobotBarApp;component/Resources/KalibreringGuide/StativHojreTransit.jpg",
            ScriptsToRun = new List<string> { "kalibreringStativHøjre.urp" }
        });

        Steps.Add(new CalibrationStep
        {
            Number = 11,
            Title = "Stativet",
            Description = "Til sidst vil robotten køre i midten af stativet og røre punktet i bunden af den grønne flaskeholder. Start scriptet og gør klar til at kigge igen.",
            ImageSource = "/RobotBarApp;component/Resources/KalibreringGuide/PunktRamtGreen.jpg",
            ScriptsToRun = new List<string> { "kalibreringStativMidten.urp" }
        });

        Steps.Add(new CalibrationStep
        {
            Number = 12,
            Title = "Færdig",
            Description = "Robotten er kalibreret og klar til brug.",
            ImageSource = null
        });
    }

    private void Previous()
    {
        if (CanGoPrevious)
            CurrentIndex--;
    }

    private void Next()
    {
        if (CanGoNext)
            CurrentIndex++;
    }

    private void RunCurrentStepScripts()
    {
        var step = CurrentStep;
        if (step?.HasScripts != true)
            return;

        try
        {
            _robotLogic.RunRobotScripts(step.ScriptsToRun);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }

    private void Close()
    {
        // Return to home/start page used in this app today.
        _navigation.NavigateTo<EventListViewModel>();
    }
}

