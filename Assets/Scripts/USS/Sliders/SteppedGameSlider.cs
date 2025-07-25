using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[UxmlElement]
public partial class SteppedGameSlider : GameSlider
{
    public static readonly string stepUssClassName = "step-mark";

    public event Action OnStepChange;

    [UxmlAttribute]
    public float Step
    {
        get
        {
            return _step;
        }
        set
        {
            _step = Mathf.Clamp(value, 0.01f, MaxValue);
            OnStepChange.Invoke();
        }
    }

    private float _step = 0.5f;
    // Amount of steps inside slider 
    // not including MinValue and MaxValue
    private int _stepsAmount => 
        Mathf.FloorToInt((MaxValue - MinValue) / Step) - 
        (MaxValue % Step == 0 ? 1 : 0);
    private List<VisualElement> m_FilledSteps = new();
    private List<VisualElement> m_EmptySteps = new();
    
    public SteppedGameSlider() : base()
    {
        UpdateStepsRenderer();
        AlignValueToStep();

        RegisterCallback<GeometryChangedEvent>(
            evt =>
            {
                UpdateStepsRenderer();
            });

        OnStepChange += AlignValueToStep;
        OnValueChange += AlignValueToStep;
        OnStepChange += UpdateStepsRenderer;
        OnValueChange += UpdateStepsRenderer;
    }

    private void AlignValueToStep()
    {
        Debug.Log(Value);
        Debug.Log(Step);
        Debug.Log(MinValue);
        Debug.Log(MaxValue);
        if (Value % Step == 0 || Value == MinValue || Value == MaxValue) return;

        float distanceToMin = Mathf.Abs(Value - MinValue);
        float distanceToMax = Mathf.Abs(Value - MaxValue);
        float stepValue = Mathf.Round(Value / Step) * Step;
        float distanceToStep = Mathf.Abs(Value - stepValue);

        if (distanceToMin < distanceToStep && distanceToMin <= distanceToMax)
            Value = MinValue;
        else if (distanceToMax < distanceToStep)
            Value = MaxValue;
        else
            Value = stepValue;
    }

    private float GetStepLeftOffset(int stepIndex, bool isEmpty)
    {
        var result = GetOffsetFromValue(Step * (stepIndex + 1)) * trackWidth;
        if (isEmpty)
            result -= GetOffsetFromValue(Value) * trackWidth + m_Thumb.resolvedStyle.width / 2; // Apply offset from empty mask
        return result;
    }

    private void UpdateStepsRenderer()
    {
        while (m_FilledSteps.Count > _stepsAmount)
        {
            m_FilledSteps.Last().RemoveFromHierarchy();
            m_FilledSteps.RemoveAt(m_FilledSteps.Count - 1);
        }
        while (m_EmptySteps.Count > _stepsAmount)
        {
            m_EmptySteps.Last().RemoveFromHierarchy();
            m_EmptySteps.RemoveAt(m_EmptySteps.Count - 1);
        }

        for (int i = 0; i < _stepsAmount; i++)
        {
            if (m_FilledSteps.Count <= i)
            {
                VisualElement filledStep = new();
                m_FilledTrackMask.Add(filledStep);
                filledStep.AddToClassList(stepUssClassName);
                m_FilledSteps.Add(filledStep);
            }
            m_FilledSteps[i].style.left = GetStepLeftOffset(i, false);
            m_FilledSteps[i].style.width = trackWidth * 0.15f;

            if (m_EmptySteps.Count <= i)
            {
                VisualElement emptyStep = new();
                m_EmptyTrackMask.Add(emptyStep);
                emptyStep.AddToClassList(stepUssClassName);
                m_EmptySteps.Add(emptyStep);
            }
            m_EmptySteps[i].style.left = GetStepLeftOffset(i, true);
            m_EmptySteps[i].style.width = trackWidth * 0.15f;
        }
    }
}
