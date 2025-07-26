using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEngine.UIElements
{

    [UxmlElement]
    public partial class GameDropdown : VisualElement
    {
        #region constants
        public static readonly string ussClassName = "dropdown";

        public static readonly string dropDownShowUssClassName = "dropdown-show";
        public static readonly string showTextContainerUssClassName = dropDownShowUssClassName + "-text-container";
        public static readonly string showTextUssClassName = dropDownShowUssClassName + "-text";
        public static readonly string showIconContainerUssClassName = dropDownShowUssClassName + "-icon-container";
        public static readonly string showIconUssClassName = dropDownShowUssClassName + "-icon";
        public static readonly string showIconOpenedUssClassName = "open";
        public static readonly string showIconClosedUssClassName = "close";

        public static readonly string dropDownScrollContainerUssClassName = "dropdown-scroll-container";
        public static readonly string elementsMaskContainerUssClassName = "dropdown-mask-container";
        public static readonly string elementsMaskUssClassName = "dropdown-mask";
        public static readonly string elementsContainerClassName = "elements-container";
    
        public static readonly string elementUssClassName = "dropdown-element";
        public static readonly string elementTextUssClassName = elementUssClassName + "-text";
        public static readonly string elementDividerUssClassName = elementUssClassName + "-divider";
        #endregion

        #region elements
        protected VisualElement m_DropDownShow;
        protected VisualElement m_ShowTextContainer;
        protected Label m_ShowText;
        protected VisualElement m_ShowIconContainer;
        protected VisualElement m_ShowIcon;

        protected VisualElement m_DropDownScrollContainer;
        protected VisualElement m_ElementsMaskContainer;
        protected VisualElement m_ElementsMask;
        protected VisualElement m_ElementsContainer;

        protected List<VisualElement> m_Elements = new();
        #endregion

        public event Action OnValueChange;
        public event Action OnElementsListChange;

        public string Value
        {
            get
            {
                if (_currElementIndex < 0 || _currElementIndex >= ElementsList.Count)
                    return "";
                else
                    return ElementsList[_currElementIndex];
            }

            set
            {
                var choosenValueIndex = ElementsList.IndexOf(value);
                if (choosenValueIndex < 0)
                    throw new ArgumentException(
                        $"Don't have given element({value}) in elements list({ElementsList.ToString()})");
                _currElementIndex = choosenValueIndex;
                OnValueChange.Invoke();
            }
        }
        [UxmlAttribute]
        public string HeaderValue
        {
            get =>
                _uxmlAttributeHeaderValue;
            set
            {
                _uxmlAttributeHeaderValue = value;
                var choosenValueIndex = ElementsList.IndexOf(value);
                if (choosenValueIndex < 0)
                    return;
                _currElementIndex = choosenValueIndex;
                OnValueChange.Invoke();
            }
        }
        public ReadOnlyCollection<string> ElementsList
        {
            get => _elementsList.AsReadOnly();
            set
            {
                if (value == null)
                    _elementsList.Clear();
                else
                    _elementsList = value.ToList().Distinct().ToList();
                OnElementsListChange.Invoke();
            }
        }
        [UxmlAttribute]
        private List<string> Elements
        {

            get => _elementsList;
            set
            {
                _elementsList = value;
                OnElementsListChange.Invoke();
            }
        }

        [UxmlAttribute]
        private float scrollSpeed;

        private float _minTopOffset =>
            Mathf.Min(
                (m_ElementsMask.resolvedStyle.height -
                m_Elements.Count *
                (m_Elements.Count > 0 ? m_Elements[0].resolvedStyle.height : 0)),
                _maxTopOffset);
        private float _maxTopOffset => 0;
        private int _currElementIndex = -1;
        private string _uxmlAttributeHeaderValue = "";
        private List<string> _elementsList = new();
        private bool _dropDownOpened = true;

        public GameDropdown() : base()
        {
            #region elements
            /* 
             * Init structure of elements:
             * DropDown
             * * m_DropDownShow
             * * * m_ShowTextContainer
             * * * * m_ShowText
             * * * m_ShowIconContainer
             * * * * m_ShowIcon
             * * m_DropDownScrollContainer
             * * * m_ElementsMaskContainer
             * * * * m_ElementsMask
             * * * * * m_ElementsContainer
             * * * * * * m_Elements
             * * * * * * * m_ElementLabels
             * * * * * * * m_ElementDividers
             * * * * * * (other elements)
            */

            AddToClassList(ussClassName);

            m_DropDownShow = new();
            m_DropDownShow.AddToClassList(dropDownShowUssClassName);
            Add(m_DropDownShow);

            m_ShowTextContainer = new();
            m_ShowTextContainer.AddToClassList(showTextContainerUssClassName);
            m_DropDownShow.Add(m_ShowTextContainer);

            m_ShowText = new();
            m_ShowText.AddToClassList(showTextUssClassName);
            m_ShowTextContainer.Add(m_ShowText);

            m_ShowIconContainer = new();
            m_ShowIconContainer.AddToClassList(showIconContainerUssClassName);
            m_DropDownShow.Add(m_ShowIconContainer);

            m_ShowIcon = new();
            m_ShowIcon.AddToClassList(showIconUssClassName);
            m_ShowIconContainer.Add(m_ShowIcon);

            m_DropDownScrollContainer = new();
            m_DropDownScrollContainer.AddToClassList(dropDownScrollContainerUssClassName);
            Add(m_DropDownScrollContainer);

            m_ElementsMaskContainer = new();
            m_ElementsMaskContainer.AddToClassList(elementsMaskContainerUssClassName);
            m_DropDownScrollContainer.Add(m_ElementsMaskContainer);

            m_ElementsMask = new();
            m_ElementsMask.AddToClassList(elementsMaskUssClassName);
            m_ElementsMaskContainer.Add(m_ElementsMask);

            m_ElementsContainer = new();
            m_ElementsContainer.AddToClassList(elementsContainerClassName);
            m_ElementsMask.Add(m_ElementsContainer);
            #endregion

            OnElementsListChange += UpdateElementListRenderer;
            OnValueChange += UpdateCurrentElementRenderer;
            UpdateElementListRenderer();
            UpdateCurrentElementRenderer();
            CloseDropDown();

            m_DropDownShow.RegisterCallback<ClickEvent>(evt => ToggleDropDownList());
            m_DropDownScrollContainer.RegisterCallback<WheelEvent>(evt => Scroll(evt.delta.y * -1));
        }

        public void AddElement(string element, int index = -1)
        {
            if (ElementsList.Contains(element))
                throw new ArgumentException($"{element} element is not unique, " +
                    $"it is already presents in the element list {ElementsList}");
            bool currElementChanged = false;
            if (index >= 0)
            {
                if (index > ElementsList.Count)
                    index = ElementsList.Count;
                if (index <= _currElementIndex)
                {
                    _currElementIndex++;
                    currElementChanged = true;
                }
            }
            else
                index = ElementsList.Count;
            _elementsList.Insert(index, element);
            OnElementsListChange.Invoke();
            if (currElementChanged)
                OnValueChange.Invoke();
        }

        public void RemoveElement(string element)
        {
            if (!ElementsList.Contains(element))
                throw new ArgumentException($"{element} element is not found " +
                    $"in the element list {ElementsList}");
            RemoveElement(ElementsList.IndexOf(element));
        }

        public void RemoveElement(int elementIndex)
        {
            if (elementIndex >= ElementsList.Count)
                throw new ArgumentException($"{elementIndex} element index " +
                    $"is bigger than element list count ({ElementsList.Count})");
            if (elementIndex < 0)
                throw new ArgumentException($"{elementIndex} element index " +
                    $"cannot be smaller than 0");
            bool currElementChanged = false;
            if (_currElementIndex == elementIndex)
            {
                _currElementIndex = -1;
                currElementChanged = true;
            }
            else if (_currElementIndex > elementIndex)
            {
                _currElementIndex--;
                currElementChanged = true;
            }
            _elementsList.RemoveAt(elementIndex);
            OnElementsListChange.Invoke();
            if (currElementChanged)
                OnValueChange.Invoke();
        }

        private void Scroll(float scrollAmount)
        {
            m_ElementsContainer.style.top = Mathf.Clamp(
                m_ElementsContainer.resolvedStyle.top + scrollAmount * scrollSpeed, 
                _minTopOffset, 
                _maxTopOffset);
        }

        private void UpdateElementListRenderer()
        {
            if (m_Elements == null) return;
            while (m_Elements.Count > 0)
            {
                m_Elements.Last().RemoveFromHierarchy();
                m_Elements.RemoveAt(m_Elements.Count - 1);
            }

            for (int i = 0; i < ElementsList.Count; i++)
            {
                VisualElement currElement = new();
                Label currElementLabel = new();
                VisualElement currElementDivider = new();
            
                currElement.AddToClassList(elementUssClassName);
                currElementLabel.AddToClassList(elementTextUssClassName);
                currElementDivider.AddToClassList(elementDividerUssClassName);
            
                currElement.Add(currElementLabel);
                currElement.Add(currElementDivider);
                m_ElementsContainer.Add(currElement);
                m_Elements.Add(currElement);

                currElementLabel.text = ElementsList[i].ToString();
                int index = i;
                currElement.RegisterCallback<ClickEvent>(
                    (evt) =>
                    {
                        if (_dropDownOpened)
                        {
                            Value = ElementsList[index];
                            ToggleDropDownList();
                        }
                    });
            }
        }

        private void UpdateCurrentElementRenderer()
        {
            if (Value != m_ShowText.text)
                m_ShowText.text = Value;
        }

        private void ToggleDropDownList()
        {
            if (_dropDownOpened)
                CloseDropDown();
            else
                OpenDropDown();
        }
    
        private void CloseDropDown()
        {
            if (!_dropDownOpened) return;

            m_DropDownScrollContainer.SetEnabled(false);
            m_ShowIcon.AddToClassList(showIconClosedUssClassName);
            m_ShowIcon.RemoveFromClassList(showIconOpenedUssClassName);
            m_ElementsContainer.style.top = 0;
            _dropDownOpened = false;
        }

        private void OpenDropDown()
        {
            if (_dropDownOpened) return;

            m_DropDownScrollContainer.SetEnabled(true);
            m_ShowIcon.AddToClassList(showIconOpenedUssClassName);
            m_ShowIcon.RemoveFromClassList(showIconClosedUssClassName);
            _dropDownOpened = true;
        }
    }
}

