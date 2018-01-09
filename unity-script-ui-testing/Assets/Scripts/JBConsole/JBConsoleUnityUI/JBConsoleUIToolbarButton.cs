using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JBConsoleUIToolbarButton : MonoBehaviour
{
    [SerializeField] protected Button button;
    [SerializeField] protected ConsoleMenu menuType;
    [SerializeField] protected Text label;

    protected bool isActive = false;
    
    public ConsoleMenu ConsoleMenuType { get { return menuType; }}
    public System.Action<ConsoleMenu?> OnButton = delegate {};
    
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
    
    public void Setup(ConsoleMenu menuType)
    {
        this.menuType = menuType;
        SetupLabel();
    }

    private void SetupLabel()
    {
        if (label != null)
        {
            label.text = menuType.ToString().ToUpper();
        }
    }

    public virtual bool IsActive
    {
        get { return isActive; }
        set
        {
            isActive = value; 
            IsActiveChanged();
        }
    }

    protected virtual void IsActiveChanged()
    {
        
    }   
    
    private void ButtonClicked()
    {
        OnButton(menuType);
    }

}
