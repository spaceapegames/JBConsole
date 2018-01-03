
public delegate void ExternalUIToolbarButtonPressed(ConsoleMenu? newConsoleMenu);
public delegate void ExternalUIMenuButtonPressed(JBConsoleStateMenuItem menuItem);
public delegate void ExternalUISearchTermChanged(string searchTerm);

public interface JBConsoleExternalUI
{
    void SetActive(bool shouldEnable, JBConsoleState jbConsoleState);
    void AddToolbarButtonListener(ExternalUIToolbarButtonPressed listener);
    void RemoveToolbarButtonListener(ExternalUIToolbarButtonPressed listener);
    void AddMenuButtonListener(ExternalUIMenuButtonPressed listener);
    void RemoveMenuButtonListener(ExternalUIMenuButtonPressed listener);
    void AddSearchTermChangedListener(ExternalUISearchTermChanged listener);
    void RemoveSearchTermChangedListener(ExternalUISearchTermChanged listener);    
    void StateChanged(JBConsoleState state);
}