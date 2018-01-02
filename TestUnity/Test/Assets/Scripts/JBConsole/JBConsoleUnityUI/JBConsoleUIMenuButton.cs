using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JBConsoleUIMenuButton : MonoBehaviour
{
    [SerializeField] protected Button button;
    [SerializeField] protected Text label;

    protected bool toggleValue = false;
    protected JBConsoleStateMenuItem menuItem;
    
    public JBConsoleStateMenuItem MenuItem { get { return menuItem; }}
    public System.Action<JBConsoleStateMenuItem> OnButton = delegate {};
    
    protected virtual void Awake()
    {
        SetupLabel();
        if (button != null)
        {
            button.onClick.AddListener(ButtonClicked);
        }
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);   
    }

    public void Setup(JBConsoleStateMenuItem menuItem)
    {
        this.menuItem = menuItem;
        ToggleValue = menuItem.ToggleValue;
        SetupLabel();
    }
    
    private void SetupLabel()
    {
        if (label != null)
        {
            label.text = menuItem.Text;
        }
    }

    public virtual bool ToggleValue
    {
        get { return toggleValue; }
        set
        {
            toggleValue = value; 
            IsActiveChanged();
        }
    }

    protected virtual void IsActiveChanged()
    {
        
    }   
    
    private void ButtonClicked()
    {
        OnButton(menuItem);
    }

}
