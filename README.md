UnityConsole
============

Junkbyte Console for Unity.

This project is a 'port' of Junkbyte Console from Flash/actionscript to Unity.
http://code.google.com/p/flash-console/


## Unity UI Extension
When you first open the Junkbyte editor window it will ask you if you want to create a font reference. This will then create a JBConsoleConfig file in the Resources folder. This config will also reference the UI prefab for the JBConsole.

In your project after you have started the JBConsole you can then instantiate and register it to the JBConsole as an external UI.
```
jbConsoleUIGO = Instantiate(JBConsoleConfig.GetExternalUIPrefab());
var jbConsoleExternalUI = jbConsoleUIGO.GetComponent<JBConsoleExternalUI>();
JBConsole.instance.AddExternalUI(jbConsoleExternalUI);
```

## Toggles
You can now add toggles to the menus with the code
```
JBConsole.AddToggle("ToggleName", delegate
{
    // setter eg, toggle = !toggle;
}, delegate
{
    // getter eg, return toggle;
});
```

