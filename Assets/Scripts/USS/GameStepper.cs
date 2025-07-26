using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEngine.UIElements
{
    [UxmlElement]
    public partial class GameStepper : VisualElement
    {
        #region constants
        public static readonly string ussClassName = "stepper";

        public static readonly string stepperInputUssClassName = "stepper-input";
        public static readonly string stepperButtonUssClassName = "stepper-button";
        public static readonly string stepperButtonLeftUssClassName = "left";
        public static readonly string stepperButtonRightUssClassName = "right";
        #endregion

        #region elements
        protected TextField m_Input;
        protected Button m_ButtonLeft;
        protected Button m_ButtonRight;
        #endregion

        public event Action OnValueChange;

        [UxmlAttribute]
        public int Value
        {
            get => _value;
            set
            {
                var oldValue = _value;
                _value = Mathf.Clamp(value, MinValue, MaxValue);
                if (_value != oldValue)
                    OnValueChange?.Invoke();
            }
        }
        [UxmlAttribute]
        public int MinValue = 0;
        [UxmlAttribute]
        public int MaxValue = 1;
        [UxmlAttribute]
        public int ButtonStep = 1;
        [UxmlAttribute]
        public string ButtonTemplatePath = "Assets/USS/DefaultElements/Buttons/IconButton/IconButton.uxml";
        [UxmlAttribute]
        public string InputTemplatePath = "Assets/USS/DefaultElements/InputField/InputField.uxml";

        private int _value;

        public GameStepper() : base()
        {
            #region elements
            /* 
                * Init structure of elements:
                * Stepper
                * * m_ButtonLeft
                * * m_Input
                * * m_ButtonRight
            */

            AddToClassList(ussClassName);

            var buttonLeft =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ButtonTemplatePath)
                .CloneTree();
            buttonLeft.AddToClassList(stepperButtonUssClassName);
            buttonLeft.AddToClassList(stepperButtonLeftUssClassName);
            Add(buttonLeft);
            m_ButtonLeft = buttonLeft.Q<Button>(className: "unity-button");

            var inputField =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(InputTemplatePath)
                .CloneTree();
            inputField.AddToClassList(stepperInputUssClassName);
            Add(inputField);
            m_Input = inputField.Q<TextField>(className: "unity-text-field");

            var buttonRight =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ButtonTemplatePath)
                .CloneTree();
            buttonRight.AddToClassList(stepperButtonUssClassName);
            buttonRight.AddToClassList(stepperButtonRightUssClassName);
            Add(buttonRight);
            m_ButtonRight = buttonRight.Q<Button>(className: "unity-button");
            #endregion

            UpdateInputRenderer();
            OnValueChange += UpdateInputRenderer;
            m_Input.RegisterCallback<BlurEvent>(evt => UpdateInputRenderer());
            m_ButtonLeft.clicked += () => Value = Value - 1;
            m_ButtonRight.clicked += () => Value = Value + 1;
        }

        private void UpdateInputRenderer()
        {
            if (int.TryParse(m_Input.value.Trim(), out int newValue))
                Value = newValue;
            m_Input.value = "";

            SetPlaceholderText(Value.ToString() + "/" + MaxValue);
        }

        private void SetPlaceholderText(string placeholder)
        {
            string placeholderClass = TextField.ussClassName + "__placeholder";

            onFocusOut();
            m_Input.RegisterCallback<FocusInEvent>(evt => onFocusIn());
            m_Input.RegisterCallback<FocusOutEvent>(evt => onFocusOut());

            void onFocusIn()
            {
                if (m_Input.ClassListContains(placeholderClass))
                {
                    m_Input.value = string.Empty;
                    m_Input.RemoveFromClassList(placeholderClass);
                }
            }

            void onFocusOut()
            {
                if (string.IsNullOrEmpty(m_Input.text))
                {
                    m_Input.SetValueWithoutNotify(placeholder);
                    m_Input.AddToClassList(placeholderClass);
                }
            }
        }
    }
}
