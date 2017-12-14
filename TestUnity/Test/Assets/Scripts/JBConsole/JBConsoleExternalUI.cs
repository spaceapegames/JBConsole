
public delegate void ExternalUIToolbarChanged(ConsoleMenu? newConsoleMenu);

public interface JBConsoleExternalUI
{
    void SetActive(bool shouldEnable, JBConsoleState jbConsoleState);
    void AddToolbarChangedListener(ExternalUIToolbarChanged listener);
    void RemoveToolbarChangedListener(ExternalUIToolbarChanged listener);
}