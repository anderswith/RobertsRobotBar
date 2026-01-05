using System.Collections.ObjectModel;
using System.Windows.Input;
using RobotBarApp.BE;
using RobotBarApp.Services.Interfaces;

namespace RobotBarApp.ViewModels;

public class ScriptCreationWizardViewModel : ViewModelBase
{
    private readonly INavigationService _navigation;
    private readonly object? _returnParameter;

    public ObservableCollection<ScriptGuideStep> Steps { get; } = new();

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

    public ScriptGuideStep? CurrentStep =>
        (CurrentIndex >= 0 && CurrentIndex < Steps.Count) ? Steps[CurrentIndex] : null;

    public string StepCounterText => Steps.Count == 0
        ? ""
        : $"Trin {CurrentIndex + 1} / {Steps.Count}";

    public bool CanGoPrevious => CurrentIndex > 0;
    public bool CanGoNext => Steps.Count > 0 && CurrentIndex < Steps.Count - 1;

    public ICommand PreviousCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand CloseCommand { get; }

    // parameter is optional and used to return to the correct screen
    public ScriptCreationWizardViewModel(INavigationService navigation, object? returnParameter = null)
    {
        _navigation = navigation;
        _returnParameter = returnParameter;

        SeedSteps();
        CurrentIndex = 0;

        PreviousCommand = new RelayCommand(_ => Previous(), _ => CanGoPrevious);
        NextCommand = new RelayCommand(_ => Next(), _ => CanGoNext);
        CloseCommand = new RelayCommand(_ => Close());
    }

    private void SeedSteps()
    {
        Steps.Clear();

        Steps.Add(new ScriptGuideStep
        {
            Number = 1,
            Title = "Gentagelse af program",
            Description = "Åbn et nyt program og kryds gentagelse af, så scriptet kun kører en gang.",
            ImageSource = "/RobotBarApp;component/Resources/ScriptGuide/AfkrydsGentag.jpg"
        });

        Steps.Add(new ScriptGuideStep
        {
            Number = 2,
            Title = "Hjem",
            Description =
                "Sæt det første viapunkt til hjem - enten kør robotten til første punkt i et andet program eller ret på graderne til robot leddene. " +
                "Scripts skal altid starte og slutte ved hjemmepunktet for at have et fælles punkt og en glidende gennemgang fra den ene script til den næste.",
            ImageSource = "/RobotBarApp;component/Resources/ScriptGuide/Hjem.jpg"
        });

        Steps.Add(new ScriptGuideStep
        {
            Number = 3,
            Title = "Bevægelse",
            Description = "Der er to måder man kan flytte robotten på. Den ene er at ændre i led graderne for at få den ønskede position, og den anden er ‘friløb’.",
            ImageSource = "/RobotBarApp;component/Resources/ScriptGuide/Hjem.jpg"
        });

        Steps.Add(new ScriptGuideStep
        {
            Number = 4,
            Title = "Friløb",
            Description = "Friløb er den nemmeste og kræver blot at du holder knappen nede enten på skærmen eller bag på tablet'en.",
            ImageSource = "/RobotBarApp;component/Resources/ScriptGuide/FriKnap.jpg"
        });

        Steps.Add(new ScriptGuideStep
        {
            Number = 5,
            Title = "Friløb",
            Description = "Mens du eller en anden aktiverer friløb, kan man nemt flytte robotten der hvor man gerne vil have den.",
            ImageSource = "/RobotBarApp;component/Resources/ScriptGuide/FlytRobot.jpg"
        });

        Steps.Add(new ScriptGuideStep
        {
            Number = 6,
            Title = "Viapunkter",
            Description =
                "Tilføj viapunkter, klo kommandoer, vente punkter eller andet efter behov. Husk at robotten altid følger den korteste rute mellem to viapunkter, " +
                "så tilføj gerne mange punkter indimellem.",
            ImageSource = "/RobotBarApp;component/Resources/ScriptGuide/Viapunkter.jpg"
        });

        Steps.Add(new ScriptGuideStep
        {
            Number = 7,
            Title = "Viapunkter",
            Description =
                "Tag højde for omgivelserne når du laver scripts. F.eks. når man henter en flaske, løft den lidt op så den er fri fra stativet og træk den ind mod " +
                "robotten før du begynder at dreje så robotten ikke tager den korteste rute og banker ind i andre flasker eller lignende.",
            ImageSource = "/RobotBarApp;component/Resources/ScriptGuide/Viapunkt.jpg"
        });

        Steps.Add(new ScriptGuideStep
        {
            Number = 8,
            Title = "Test",
            Description = "Eksperimentér og test undervejs. Afhængig af dispensere, hælde tutter og væske kan der være behov for forskellige ventetider og/eller teknikker og bevægelser.",
            ImageSource = "/RobotBarApp;component/Resources/ScriptGuide/Test.jpg"
        });

        Steps.Add(new ScriptGuideStep
        {
            Number = 9,
            Title = "Test",
            Description = "Test scripts ved lavere hastighed og vær klar til at stoppe robotten hvis der er noget, der går galt.",
            ImageSource = "/RobotBarApp;component/Resources/ScriptGuide/Test.jpg"
        });

        Steps.Add(new ScriptGuideStep
        {
            Number = 10,
            Title = "Navngivning",
            Description = "Når scriptet er færdigt, skal den gemmes. Brug ikke specialkarakterer som å, ø og æ, da de gør, at scriptet ikke kan køres. Brug lowerCamelCase.",
            ImageSource = "/RobotBarApp;component/Resources/ScriptGuide/NavngivningTablet.jpg"
        });

        Steps.Add(new ScriptGuideStep
        {
            Number = 11,
            Title = "Navngivning",
            Description = ".urp bliver tilføjet automatisk på tablet, men husk at skrive det ind i programmet og brug det samme navn til scriptet. Husk at tjekke for stavefejl.",
            ImageSource = "/RobotBarApp;component/Resources/ScriptGuide/NavngivningProgram.png"
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

    private void Close()
    {
        // If we were opened from a VM that requires a parameter in its constructor,
        // we need to go back with the same parameter.
        switch (_returnParameter)
        {
            case Guid g:
                {
                    // TilfoejDrinkViewModel requires contextId
                    _navigation.NavigateTo<TilfoejDrinkViewModel>(g);
                    return;
                }
            case ScriptGuideReturnTarget.Ingredient:
                {
                    _navigation.NavigateTo<TilfoejIngrediensViewModel>();
                    return;
                }
            default:
                {
                    _navigation.NavigateTo<EventListViewModel>();
                    return;
                }
        }
    }

    public enum ScriptGuideReturnTarget
    {
        Ingredient
    }
}
