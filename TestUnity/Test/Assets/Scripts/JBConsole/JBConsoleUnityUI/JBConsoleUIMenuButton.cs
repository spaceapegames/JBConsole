using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JBConsoleUIMenuButton : MonoBehaviour
{
    [SerializeField] protected Button button;
    [SerializeField] protected Text[] labels;

    protected bool toggleValue = false;
    protected JBConsoleStateMenuItem menuItem;
    
    public JBConsoleStateMenuItem MenuItem { get { return menuItem; }}
    public Button Button { get { return button; }}
    public System.Action<JBConsoleStateMenuItem> OnButton = delegate {};
    public System.Action<JBConsoleUIMenuButton> OnRefreshVisuals = delegate {};
    
    protected virtual void Awake()
    {
        SetupLabels();
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
        SetupLabels();
        OnRefreshVisuals(this);
    }
    
    private void SetupLabels()
    {
        if (labels != null)
        {
            for (var i = 0; i < labels.Length; i++)
            {
                if (labels[i] != null)
                {
                    labels[i].text = menuItem.Text;
                }                
            }
        }
    }

    public virtual bool ToggleValue
    {
        get { return toggleValue; }
        private set
        {
            toggleValue = value; 
            IsActiveChanged();
        }
    }

    protected virtual void IsActiveChanged()
    {
        OnRefreshVisuals(this);
    }   
    
    private void ButtonClicked()
    {
        OnButton(menuItem);
    }

}
