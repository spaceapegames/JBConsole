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

## Unity Projects
### unity-build-unity-project
This is where the DLLs get copied to after the build and in this project the UI prefab is constructed against the DLL.
### unity-script-ui-testing
This is a rather hacky project that contains a copy of the JBConsole source and a version of the UI prefab that is linked to these scripts. This is the place to go to try to debug an issue with the UI!
### unity-test-unity-project
This is a more traditional test project that contains the JBConsole package retrieved via nuget and uses it with some simple test buttons etc.
