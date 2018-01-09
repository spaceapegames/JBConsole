using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JBConsoleUILogScrollingOptions : MonoBehaviour
{
	[SerializeField] private Button scrollToTopButton = null;
	[SerializeField] private Button autoScrollButton = null;
	
	private RectTransform rectTransform = null;

	public Action onAutoScrollButtonPressed = delegate { };
	public Action onScrollToTopButtonPressed = delegate { };
	
	public RectTransform RectTransform
	{
		get { return rectTransform; }
	}
	
	private void Awake()
	{
		rectTransform = gameObject.GetComponent<RectTransform>();
		if (scrollToTopButton != null)
		{
			scrollToTopButton.onClick.AddListener(ScrollToTopButtonPressed);
		}

		if (autoScrollButton != null)
		{
			autoScrollButton.onClick.AddListener(AutoScrollButtonPressed);
		}
	}

	private void ScrollToTopButtonPressed()
	{
		onScrollToTopButtonPressed();
	}

	private void AutoScrollButtonPressed()
	{
		onAutoScrollButtonPressed();
	}
	
}
