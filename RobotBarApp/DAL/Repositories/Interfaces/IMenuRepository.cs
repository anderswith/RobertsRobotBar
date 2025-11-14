using RobotBarApp.BE;

namespace RobotBarApp.DAL.Repositories.Interfaces;

public interface IMenuRepository
{
    void AddMenu(Menu menu);
    IEnumerable<Menu> GetAllMenus();
    public Menu? GetMenuById(Guid menuId);
    void DeleteMenu(Menu menu);
    void UpdateMenu(Menu menu);

}