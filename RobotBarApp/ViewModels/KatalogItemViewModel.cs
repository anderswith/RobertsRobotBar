using System;
using System.Windows.Input;
using System.Windows.Media;

namespace RobotBarApp.ViewModels;

public class KatalogItemViewModel : ViewModelBase
{
    public Guid Id { get; }
    public string Name { get; }
    public ImageSource Image { get; }
    public KatalogItemType ItemType { get; }

    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

    public KatalogItemViewModel(
        Guid id,
        string name,
        ImageSource image,
        KatalogItemType itemType,
        Action<Guid, KatalogItemType> onDelete,
        Action<Guid, KatalogItemType> onEdit)
    {
        Id = id;
        Name = name;
        Image = image;
        ItemType = itemType;

        DeleteCommand = new RelayCommand(_ => onDelete(Id, ItemType));
        EditCommand   = new RelayCommand(_ => onEdit(Id, ItemType));
    }
}