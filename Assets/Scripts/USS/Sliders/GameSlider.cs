using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEngine.UIElements
{
    [UxmlElement]
    public partial class GameSlider : VisualElement
    {
        #region constants
        public static readonly string ussClassName = "slider";

        public static readonly string trackBarsContainerUssClassName = "track-bars-container";
        public static readonly string maskTrackBarUssClassName = "track-mask";
        public static readonly string trackBarUssClassName = "track";
        public static readonly string filledTrackBarUssClassName = "filled";
        public static readonly string filledTrackMaskUssClassName = "filled";
        public static readonly string emptyTrackMaskUssClassName = "empty";
        public static readonly string emptyTrackBarUssClassName = "empty";

        public static readonly string thumbUssClassName = "thumb";
        public static readonly string activeThumbUssClassName = thumbUssClassName + "-used";

        public static readonly string labelsContainerUssClassName = "labels-container";
        public static readonly string labelUssClassName = "label";
        public static readonly string leftLabelUssClassName = "left";
        public static readonly string rightLabelUssClassName = "right";

        // Amount of space that is not occured by track(empty space)
        protected const float unusedTrackSpacePercent = 0.15f;
        protected const float oneSideUnusedSpace = unusedTrackSpacePercent / 2;
        protected const float usedTrackSpace = 1 - unusedTrackSpacePercent;
        protected float trackWidth => resolvedStyle.width;
        #endregion

        public event Action OnValueChange;
        public event Action OnMaxValueChange;
        public event Action OnMinValueChange;

        [UxmlAttribute]
        public float Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = Mathf.Clamp(value, _minValue, _maxValue);
                OnValueChange.Invoke();
            }
        }
        [UxmlAttribute]
        public float MinValue
        {
            get
            {
                return _minValue;
            }
            set
            {
                if (value > _maxValue) return;
                _minValue = value;
                OnMinValueChange.Invoke();
            }
        }
        [UxmlAttribute]
        public float MaxValue
        {
            get
            {
                return _maxValue;
            }
            set
            {
                if (value < _minValue) return;
                _maxValue = value;
                OnMaxValueChange.Invoke();
            }
        }

        private float _value = 0.5f;
        private float _minValue;
        private float _maxValue = 1;
        private bool _mouseDownOnSlider = false;

        #region elements
        protected VisualElement m_TrackBarsContainer;
        protected VisualElement m_FilledTrackMask;
        protected VisualElement m_FilledTrackBar;
        protected VisualElement m_EmptyTrackMask;
        protected VisualElement m_EmptyTrackBar;

        protected VisualElement m_Thumb;

        protected VisualElement m_LabelsContainer;
        protected Label m_LeftLabel;
        protected Label m_RightLabel;
        #endregion

        public GameSlider() : base()
        {
            #region elements
            /* 
             * Init structure of elements:
             * Slider
             * * m_TrackBarsContainer
             * * * m_FilledTrackMask
             * * * * m_FilledTrackBar
             * * * m_EmptyTrackMask
             * * * * m_EmptyTrackBar
             * * * m_Thumb
             * * m_LabelsContainer
             * * * m_LeftLabel
             * * * m_RightLabel
            */

            AddToClassList(ussClassName);

            m_TrackBarsContainer = new();
            m_TrackBarsContainer.AddToClassList(trackBarsContainerUssClassName);
            Add(m_TrackBarsContainer);

            m_FilledTrackMask = new();
            m_FilledTrackMask.AddToClassList(maskTrackBarUssClassName);
            m_FilledTrackMask.AddToClassList(filledTrackMaskUssClassName);
            m_TrackBarsContainer.Add(m_FilledTrackMask);

            m_FilledTrackBar = new();
            m_FilledTrackBar.AddToClassList(trackBarUssClassName);
            m_FilledTrackBar.AddToClassList(filledTrackBarUssClassName);
            m_FilledTrackMask.Add(m_FilledTrackBar);

            m_EmptyTrackMask = new();
            m_EmptyTrackMask.AddToClassList(maskTrackBarUssClassName);
            m_EmptyTrackMask.AddToClassList(emptyTrackMaskUssClassName);
            m_TrackBarsContainer.Add(m_EmptyTrackMask);

            m_EmptyTrackBar = new();
            m_EmptyTrackBar.AddToClassList(trackBarUssClassName);
            m_EmptyTrackBar.AddToClassList(emptyTrackBarUssClassName);
            m_EmptyTrackMask.Add(m_EmptyTrackBar);

            m_Thumb = new();
            m_Thumb.AddToClassList(thumbUssClassName);
            m_TrackBarsContainer.Add(m_Thumb);

            m_LabelsContainer = new();
            m_LabelsContainer.AddToClassList(labelsContainerUssClassName);
            Add(m_LabelsContainer);

            m_LeftLabel = new();
            m_LeftLabel.AddToClassList(labelUssClassName);
            m_LeftLabel.AddToClassList(leftLabelUssClassName);
            m_LabelsContainer.Add(m_LeftLabel);

            m_RightLabel = new();
            m_RightLabel.AddToClassList(labelUssClassName);
            m_RightLabel.AddToClassList(rightLabelUssClassName);
            m_LabelsContainer.Add(m_RightLabel);
            #endregion

            m_TrackBarsContainer.RegisterCallback<MouseDownEvent>(
                evt =>
                {
                    Value = GetSliderValueFromClick(evt.localMousePosition.x / trackWidth);
                    _mouseDownOnSlider = true;
                    m_Thumb.AddToClassList(activeThumbUssClassName);
                });
            if (panel?.visualTree != null)
                panel.visualTree.RegisterCallback<MouseUpEvent>(
                    evt =>
                    {
                        _mouseDownOnSlider = false;
                        m_Thumb.RemoveFromClassList(activeThumbUssClassName);
                    });
            else
                m_TrackBarsContainer.RegisterCallback<MouseUpEvent>(
                evt =>
                {
                    _mouseDownOnSlider = false;
                     m_Thumb.RemoveFromClassList(activeThumbUssClassName);
                });
            m_TrackBarsContainer.RegisterCallback<MouseMoveEvent>(
                    evt =>
                    {
                        if (_mouseDownOnSlider)
                            Value = GetSliderValueFromClick(evt.localMousePosition.x / trackWidth);
                    });
            RegisterCallback<GeometryChangedEvent>(
                evt =>
                {
                    UpdateTracks();
                    UpdateThumbPosition();
                });
            UpdateTracks();
            m_LeftLabel.text = MinValue.ToString();
            m_RightLabel.text = MaxValue.ToString();

            OnValueChange += UpdateThumbPosition;
            OnMinValueChange += () => m_LeftLabel.text = MinValue.ToString();
            OnMaxValueChange += () => m_RightLabel.text = MaxValue.ToString();
        }

        // Gets value 0..1 of slider and maked it 
        // MinValue...MaxValue
        protected float ApplyMinMaxToValue(float value) => value * (MaxValue - MinValue) + MinValue;
        // Converts a value from MinValue...MaxValue range to 0...1 range
        protected float ApplyValueToNormalized(float value) => (value - MinValue) / (MaxValue - MinValue);

        // clickPosition - value 0...1
        // where 0 is click completly to the left
        // and 1 is click completly to the right of the track
        protected float GetSliderValueFromClick(float clickPosition)
        {
            clickPosition = Mathf.Clamp(clickPosition, oneSideUnusedSpace, 1 - oneSideUnusedSpace);

            return ApplyMinMaxToValue((clickPosition - oneSideUnusedSpace) / (usedTrackSpace));
        }

        // Returns value from 0 to usedTrackSpace
        // where value is how much left offset %
        // some element with absolute position should have
        // to match position with given value
        protected float GetOffsetFromValue(float value) => ApplyValueToNormalized(value) * usedTrackSpace;

        private void UpdateThumbPosition()
        {
            m_Thumb.style.left = GetOffsetFromValue(Value) * trackWidth;
            UpdateTrackMasks();
        }

        private void UpdateTrackMasks()
        {
            float filledTrackPixels = GetOffsetFromValue(Value) * trackWidth + m_Thumb.resolvedStyle.width / 2;
            m_FilledTrackMask.style.width = filledTrackPixels;
            m_EmptyTrackMask.style.left = filledTrackPixels;
            m_EmptyTrackMask.style.width = trackWidth - filledTrackPixels;
        }

        private void UpdateTracks()
        {
            UpdateTrackMasks();
            m_FilledTrackBar.style.width = resolvedStyle.width;
            m_EmptyTrackBar.style.width = resolvedStyle.width;
        }
    }
}
