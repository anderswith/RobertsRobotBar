using System.ComponentModel.DataAnnotations;

namespace RobotBarApp.BE;

public class Drink
{
    [Key]
    public Guid DrinkId { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }
    public int UseCount { get; set; }
    public bool IsMocktail { get; set; }
    
    public ICollection<DrinkContent> DrinkContents { get; set; }
    public ICollection<MenuContent> MenuContents { get; set; }
    public ICollection<DrinkScript> DrinkScripts { get; set; }
}