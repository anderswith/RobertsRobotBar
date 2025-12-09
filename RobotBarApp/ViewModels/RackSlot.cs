using RobotBarApp.BE;

namespace RobotBarApp.ViewModels;

public class RackSlot : ViewModelBase
{
    public int Position { get; }

    private Ingredient _ingredient;
    public Ingredient Ingredient
    {
        get => _ingredient;
        set => SetProperty(ref _ingredient, value);
    }

    public RackSlot(int pos)
    {
        Position = pos;
    }
}