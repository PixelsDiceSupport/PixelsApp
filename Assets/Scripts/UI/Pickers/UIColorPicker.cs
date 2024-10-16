﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIColorPicker : MonoBehaviour
{
    [Header("Controls")]
    public Button backButton;
    public Text titleText;
    public UIColorEditor editor;

    public bool isShown => gameObject.activeSelf;

    Color currentColor;
    System.Action<bool, Color> closeAction;

    /// <summary>
    /// Invoke the color picker
    /// </sumary>
    public void Show(string title, Color previousColor, System.Action<bool, Color> closeAction)
    {
        if (isShown)
        {
            Debug.LogWarning("Previous Color picker still active");
            ForceHide();
        }

        gameObject.SetActive(true);
        currentColor = previousColor;
        titleText.text = title;
        editor.SelectColor(previousColor);
        this.closeAction = closeAction;
        // Register at last as the call to SelectColor() above will trigger the event
        editor.onColorSelected += Hide;
    }

    /// <summary>
    /// If for some reason the app needs to close the dialog box, this will do it!
    /// Normally it closes itself when you tap ok or cancel
    /// </sumary>
    public void ForceHide()
    {
        Hide(false, currentColor);
    }

    void Awake()
    {
        backButton.onClick.AddListener(Back);
    }

    void Hide(Color color)
    {
        Hide(true, color);
    }

    void Hide(bool result, Color color)
    {
        editor.onColorSelected -= Hide;
        gameObject.SetActive(false);
        closeAction?.Invoke(result, color);
        closeAction = null;
    }

    void Back()
    {
        Hide(false, currentColor);
    }
}
