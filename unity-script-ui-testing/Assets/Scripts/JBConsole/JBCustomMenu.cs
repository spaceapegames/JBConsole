using UnityEngine;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

[System.Serializable]
public class JBCustomMenu
{
	MenuItem root = new MenuItem("root");

	MenuItem currentMenu;
	string[] currentLinks;

	public JBCustomMenu ()
	{
		currentMenu = root;
		root.Children = new List<MenuItem>();
	}
	
	public void AddButton(string name, Action activateCallback)
	{
		if(string.IsNullOrEmpty(name)) return;
		root.AddButtonPath(GetPath(name), 0, activateCallback);
		currentLinks = null;
	}
	
	public void AddToggle(string name, Action activateCallback, Func<bool> getValueCallback)
	{
		if(string.IsNullOrEmpty(name)) return;
		root.AddTogglePath(GetPath(name), 0, activateCallback, getValueCallback);
		currentLinks = null;
	}
	
	public void Remove(string name)
	{
		if(string.IsNullOrEmpty(name)) return;
		root.RemovePath(GetPath(name), 0);
		currentLinks = null;
	}

	public void PopToRoot()
	{
		currentLinks = null;
		currentMenu = root;
	}

	public MenuItem GetCurrentMenuItem()
	{
		return currentMenu;
	}
	
	public string[] GetCurrentMenuLink()
	{
		if(currentLinks == null)
		{
			currentLinks = currentMenu.GetLinks();
		}
		return currentLinks;
	}
	
	public void OnCurrentLinkClicked(int index)
	{
		var newMenu = currentMenu.OnChildClicked(index);
		if(newMenu != null)
		{
			currentMenu = newMenu;
			currentLinks = null;
		}
		else
		{
			PopToRoot();
		}
	}

	string[] GetPath(string name)
	{
		return Regex.Split(name, @"\s*\/\s*");
	}







	public class MenuItem
	{
		public enum MenuItemType
		{
			Button,
			Toggle
		}
		
		public string Name;
		public Action ActivateCallback;
		public Func<bool> GetValueCallback;
		public List<MenuItem> Children;
		public MenuItemType ItemType;
		
		MenuItem parent;

		public MenuItem(string name)
		{
			Name = name;
		}

		public void AddButtonPath(string[] path, int pathIndex, Action activateCallback)
		{
			AddPath(path, pathIndex, MenuItemType.Button, activateCallback, null);
		}

		public void AddTogglePath(string[] path, int pathIndex, Action activateCallback, Func<bool> getValueCallback)
		{
			AddPath(path, pathIndex, MenuItemType.Toggle, activateCallback, getValueCallback);			
		}
		
		public void AddPath(string[] path, int pathIndex, MenuItemType itemType, Action activateCallback, Func<bool> getValueCallback)
		{
			var part = path[pathIndex];
			var child = FindChild(part);
			if(child == null)
			{
				child = new MenuItem(part);
				AddChild(child);
			}

			if(PathIndexHasChild(path, pathIndex))
			{
				child.AddPath(path, pathIndex + 1, itemType, activateCallback, getValueCallback);
			}
			else
			{
				child.ItemType = itemType;
				child.SetCallbacks(activateCallback, getValueCallback);
			}
		}
		
		public void RemovePath(string[] path, int pathIndex)
		{
			var part = path[pathIndex];
			var child = FindChild(part);
			if(child != null)
			{
				if(PathIndexHasChild(path, pathIndex))
				{
					child.RemovePath(path, pathIndex + 1);
					if(child.Children == null || child.Children.Count == 0)
					{
						Children.Remove(child);
					}
				}
				else
				{
					Children.Remove(child);
				}
			}
		}

		bool PathIndexHasChild(string[] path, int pathIndex)
		{
			return path.Length > pathIndex + 1;
		}
		
		MenuItem FindChild(string n)
		{
			return Children != null ? Children.Find(m => m.Name == n) : null;
		}
		
		void SetCallbacks(Action activateCallback, Func<bool> getValueCallback)
		{
			ActivateCallback = activateCallback;
			GetValueCallback = getValueCallback;
			Children = null;
		}

		void AddChild(MenuItem child)
		{
			ActivateCallback = null;
			if(Children == null) Children = new List<MenuItem>();
			Children.Add(child);
			child.parent = this;
		}

		public string GetRawLinkName()
		{
			return Name;
		}

		public bool GetToggleValue()
		{
			return GetValueCallback != null && GetValueCallback();
		}
		
		public string GetLinkName()
		{
			var name = Name;
			if (ItemType == MenuItemType.Toggle)
			{
				var enabled = GetValueCallback != null && GetValueCallback();
				name = enabled ? ("☑ " + name) : ("☐ " + name);
			}
			if(HasChildren())
			{
				return name + " /";
			}
			return name;
		}

		public string[] GetLinks()
		{
			var links = new List<string>();
			if(parent != null) links.Add ("<");
			foreach(var item in Children)
			{
				links.Add(item.GetLinkName());
			}
			return links.ToArray();
		}

		public bool HasChildren()
		{
			return Children != null && Children.Count > 0;
		}
		
		public bool HasParent()
		{
			return parent != null;
		}
		
		public bool IsParentPath(int index)
		{
			if(parent != null) index--;
			return (index < 0 || index >= Children.Count);
		}
		
		public MenuItem OnChildClicked(int index)
		{
			if(parent != null) index--;
			if(index < 0 || index >= Children.Count) return parent;

		    //try
		    {
                var child = Children[index];
                if (child.ActivateCallback != null)
                {
                    child.ActivateCallback();
                    return this;
                }
                else
                {
                    return child;
                }
		    }
		    /*catch (Exception)
            {
		        return parent;
		    }*/
		}
	}
}