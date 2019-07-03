﻿using System;
using System.Collections;
using System.Collections.Generic;
using Animations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using static MultiSlider;

public class MultiSliderHandle : MonoBehaviour, IPointerDownHandler, IDragHandler
{
	[SerializeField]
	RectTransform _selectedOvr = null;

	[SerializeField]
	Text _valueTxt = null;

	Canvas _canvas = null;

	Image _image;
	MultiSlider _slider;
	Vector2 _dragOffset;

	//public bool IsSelected { get { return EventSystem.current.currentSelectedGameObject == gameObject; } }
	public bool Selected { get { return _slider.ActiveHandle == this; } }
	public Color Color { get { return _image.color; } }

	public void ChangeColor(Color color)
	{
		ChangeColor(color, false);
	}

	public void ChangeColor(Color color, bool noRepaint)
	{
		_image.color = color;
		if (!noRepaint)
		{
			_slider.Repaint();
		}
	}

	public MultiSliderHandle Duplicate()
	{
		var dupHandle = GameObject.Instantiate<MultiSliderHandle>(this, transform.parent);
		//Keep same color dupHandle.ChangeColor(Palette.Instance.ActiveColor);
		_slider.Repaint(); //TODO slider should be notified instead
		_slider.SelectHandle(dupHandle);
		return dupHandle;
	}

	public void DuplicateSelf()
	{
		var dupHandle = Duplicate();
		dupHandle.transform.localPosition = transform.localPosition;
	}

	public void RemoveSelf()
	{
		_slider.RemoveHandle(this);
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		_dragOffset = transform.position - Input.mousePosition;

		EventSystem.current.SetSelectedGameObject(gameObject);
		_slider.SelectHandle(this);
	}

	void IDragHandler.OnDrag(PointerEventData eventData)
	{
		var rect = (_slider.transform as RectTransform).rect;
		Vector2 min = _slider.transform.TransformPoint(rect.xMin, rect.yMin, 0);
		Vector2 max = _slider.transform.TransformPoint(rect.xMax, rect.yMax, 0);

		if (_slider.Direction == SliderDirection.Horizontal)
		{
			float x = Mathf.Clamp(Input.mousePosition.x + _dragOffset.x, min.x, max.x);
            float y = Mathf.Lerp(min.y, max.y, _slider.HandlePosition);
			transform.position = new Vector2(x, y);
		}
		else
		{
			float x = Mathf.Lerp(min.x, max.x, _slider.HandlePosition);
			float y = Mathf.Clamp(Input.mousePosition.y + _dragOffset.y, min.y, max.y);
            transform.position = new Vector2(x, y);
		}

		Snap();

		RepaintValue();
		_slider.Repaint();
	}

	void OnHandleSelected(MultiSliderHandle handle)
	{
		Repaint();

		Palette.Instance.ColorSelected -= ChangeColor;
		if (Selected)
		{
			Palette.Instance.ColorSelected += ChangeColor;
		}
	}

	void Snap()
	{
		float unit = GetComponentInParent<TimelineView>().Unit; //TODO
		float snap = GetComponentInParent<TimelineView>().SnapInterval * unit;

		var rect = (_slider.transform as RectTransform).rect;
		Vector2 min = _slider.transform.TransformPoint(rect.xMin, rect.yMin, 0);
		Vector2 max = _slider.transform.TransformPoint(rect.xMax, rect.yMax, 0);

		if (_slider.Direction == SliderDirection.Horizontal)
		{
            float x = min.x + snap * Mathf.RoundToInt((transform.position.x - min.x) / snap);
			transform.position = new Vector2(x, transform.position.y);
		}
		else
		{
            float y = min.y + snap * Mathf.RoundToInt((transform.position.y - min.y) / snap);
            transform.position = new Vector2(transform.position.x, y);
		}
	}

	void Repaint()
	{
		transform.localScale = Vector2.one * (Selected ? 1.2f : 1f);
		if (_selectedOvr != null)
		{
			_selectedOvr.gameObject.SetActive(Selected);
		}
		if (_canvas != null)
		{
			_canvas.overrideSorting = Selected;
		}
		RepaintValue();
	}

	void RepaintValue()
	{
		if (_valueTxt != null)
		{
			float unit = GetComponentInParent<TimelineView>().Unit; //TODO

			var rect = (_slider.transform as RectTransform).rect;
			Vector2 min = _slider.transform.TransformPoint(rect.xMin, rect.yMin, 0);

			// Slider value
			float value;
			if (_slider.Direction == SliderDirection.Horizontal)
			{
				float x = transform.position.x - min.x;
				value = (_slider.transform.localPosition.x + x) / unit;
			}
			else
			{
				float y = transform.position.y - min.y;
				value = (_slider.transform.localPosition.y + y) / unit;
			}
			_valueTxt.text = value.ToString();
		}
	}

	void OnEnable()
	{
		_slider = GetComponentInParent<MultiSlider>();
		_slider.HandleSelected += OnHandleSelected;
		//Repaint();
	}

	void OnDisable()
	{
		Palette.Instance.ColorSelected -= ChangeColor;
		_slider.HandleSelected -= OnHandleSelected;
		_slider = null;
	}

	void Awake()
	{
		_canvas = GetComponent<Canvas>();
		_image = GetComponent<Image>();
		_selectedOvr.gameObject.SetActive(false);
	}

	// Use this for initialization
	void Start()
	{
		// If deleted before start run, we won't find the TimelineView
		if (GetComponentInParent<TimelineView>() != null)
		{
			Snap();
			RepaintValue();
		}
	}
}
